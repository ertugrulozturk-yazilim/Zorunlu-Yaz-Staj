'use strict';

const DURUMLAR = Object.freeze(['Yapılacak', 'Devam Ediyor', 'Tamamlandı']);
const state = { gorevler: [], personeller: [], yukleniyor: false, kaydediliyor: false, hazir: false, bekleyenler: new Set() };
const ui = Object.fromEntries(['gorevFormu', 'personel', 'personel-bilgi', 'baslik', 'aciklama', 'karakter', 'kaydet', 'yenile', 'bildirim', 'gorevTablosu', 'arama', 'durum-filtre', 'liste-bilgi', 'toplam', 'yapilacak', 'devam', 'tamamlandi'].map(id => [id, document.getElementById(id)]));

class ApiHatasi extends Error {
    constructor(message, status) { super(message); this.status = status; }
}

async function istek(adres, { method = 'GET', body, version } = {}) {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), 20000);
    try {
        const headers = { Accept: 'application/json' };
        if (body !== undefined) headers['Content-Type'] = 'application/json';
        if (version) headers['If-Match'] = `"${version}"`;
        const response = await fetch(adres, { method, headers, body: body === undefined ? undefined : JSON.stringify(body), cache: 'no-store', signal: controller.signal });
        if (!response.ok) {
            const problem = await response.json().catch(() => ({}));
            const validation = problem.errors ? Object.values(problem.errors).flat().join(' ') : '';
            throw new ApiHatasi(problem.detail || validation || problem.title || `İstek tamamlanamadı (${response.status}).`, response.status);
        }
        return response.status === 204 ? null : await response.json();
    } catch (error) {
        if (error.name === 'AbortError') throw new ApiHatasi('İstek 20 saniyede tamamlanamadı. Azure bağlantısını kontrol edip listeyi yenileyin. Kaydetme sırasında olduysa yeniden eklemeden önce listede kontrol edin.', 0);
        if (error instanceof ApiHatasi) throw error;
        throw new ApiHatasi('Sunucuya ulaşılamadı. API’nin çalıştığını kontrol edip tekrar deneyin.', 0);
    } finally { clearTimeout(timer); }
}

function mesaj(metin, tip = 'success') {
    ui.bildirim.textContent = metin;
    ui.bildirim.className = `notice ${tip === 'success' ? '' : tip}`.trim();
    ui.bildirim.hidden = false;
}

function formDurumu() {
    ui.kaydet.disabled = !state.hazir || state.kaydediliyor || state.yukleniyor;
    ui.kaydet.textContent = state.kaydediliyor ? 'Kaydediliyor…' : 'Görevi kaydet';
    for (const input of [ui.personel, ui.baslik, ui.aciklama]) input.disabled = state.kaydediliyor || !state.hazir;
    ui.yenile.disabled = state.yukleniyor || state.kaydediliyor;
    ui.yenile.textContent = state.yukleniyor ? 'Yükleniyor…' : 'Listeyi yenile';
}

function personelBilgisi() {
    const personel = state.personeller.find(p => p.id === Number(ui.personel.value));
    ui['personel-bilgi'].textContent = personel ? `${personel.departmanAdi} · ${personel.pozisyonAdi}` : '';
}

function personelleriCiz() {
    const secili = ui.personel.value;
    ui.personel.replaceChildren(new Option(''));
    for (const p of state.personeller) ui.personel.add(new Option(p.adSoyad, String(p.id)));
    if (state.personeller.some(p => String(p.id) === secili)) ui.personel.value = secili;
    else if (state.personeller.length === 1) ui.personel.value = String(state.personeller[0].id);
    state.hazir = state.personeller.length > 0;
    if (!state.hazir) ui.personel.replaceChildren(new Option('Personel bulunamadı', ''));
    personelBilgisi();
}

function hucre(metin, className) {
    const td = document.createElement('td');
    if (className) td.className = className;
    td.textContent = metin;
    return td;
}

function tabloyuCiz() {
    const toplamlar = DURUMLAR.map(durum => state.gorevler.filter(g => g.durum === durum).length);
    ui.toplam.textContent = state.gorevler.length;
    [ui.yapilacak, ui.devam, ui.tamamlandi].forEach((element, i) => element.textContent = toplamlar[i]);
    const kisiler = new Map(state.personeller.map(p => [p.id, p.adSoyad]));
    const aranan = ui.arama.value.trim().toLocaleLowerCase('tr-TR');
    const durum = ui['durum-filtre'].value;
    const gorevler = state.gorevler.filter(g => (!durum || g.durum === durum) && `${g.baslik} ${g.aciklama || ''} ${kisiler.get(g.personelId) || ''}`.toLocaleLowerCase('tr-TR').includes(aranan));
    ui.gorevTablosu.replaceChildren();
    ui['liste-bilgi'].textContent = `${gorevler.length} görev gösteriliyor · ${state.gorevler.length} toplam`;
    if (!gorevler.length) {
        const row = document.createElement('tr');
        const td = hucre(state.gorevler.length ? 'Aramana uygun görev bulunamadı.' : 'Henüz görev yok. İlk görevini soldaki formdan ekleyebilirsin.', 'empty-state');
        td.colSpan = 4; row.append(td); ui.gorevTablosu.append(row); return;
    }
    for (const gorev of gorevler) {
        const row = document.createElement('tr');
        row.dataset.id = String(gorev.id);
        const bilgi = document.createElement('td');
        const baslik = document.createElement('div'); baslik.className = 'task-title'; baslik.textContent = gorev.baslik; bilgi.append(baslik);
        if (gorev.aciklama) { const aciklama = document.createElement('div'); aciklama.className = 'task-description'; aciklama.textContent = gorev.aciklama; bilgi.append(aciklama); }
        row.append(bilgi, hucre(kisiler.get(gorev.personelId) || `Personel #${gorev.personelId}`, 'person-name'));
        const durumHucre = document.createElement('td');
        const select = document.createElement('select');
        select.className = `status-select${gorev.durum === 'Devam Ediyor' ? ' progress' : gorev.durum === 'Tamamlandı' ? ' done' : ''}`;
        select.setAttribute('aria-label', `${gorev.baslik} durumu`);
        for (const d of DURUMLAR) select.add(new Option(d, d));
        select.value = gorev.durum;
        select.disabled = state.bekleyenler.has(gorev.id);
        select.addEventListener('change', () => durumDegistir(gorev, select.value));
        durumHucre.append(select); row.append(durumHucre);
        const islem = document.createElement('td');
        const button = document.createElement('button'); button.type = 'button'; button.className = 'delete-button'; button.textContent = 'Sil';
        button.setAttribute('aria-label', `${gorev.baslik} görevini sil`);
        button.disabled = state.bekleyenler.has(gorev.id);
        button.addEventListener('click', () => gorevSil(gorev));
        islem.append(button); row.append(islem); ui.gorevTablosu.append(row);
    }
}

async function yenile({ sessiz = false } = {}) {
    if (state.yukleniyor) return;
    state.yukleniyor = true; formDurumu();
    try {
        const [personeller, gorevler] = await Promise.all([istek('/api/Personel'), istek('/api/Gorev')]);
        state.personeller = personeller; state.gorevler = gorevler;
        personelleriCiz(); tabloyuCiz();
        if (!state.hazir) mesaj('Veritabanında personel bulunamadı. Yeni bir veritabanıysa Veritabani/02_Ornek_Personel.sql dosyasını çalıştırabilirsin.', 'warning');
        else if (!sessiz) mesaj('Görev listesi güncellendi.');
        return true;
    } catch (error) {
        mesaj(error.message, 'error');
        ui['liste-bilgi'].textContent = 'Liste yüklenemedi. Bağlantıyı kontrol edip yeniden dene.';
        if (!state.gorevler.length) { const row = document.createElement('tr'); const td = hucre('Görevler alınamadı. Üstteki açıklamayı kontrol et.', 'empty-state'); td.colSpan = 4; row.append(td); ui.gorevTablosu.replaceChildren(row); }
        return false;
    } finally { state.yukleniyor = false; formDurumu(); }
}

async function gorevEkle(event) {
    event.preventDefault();
    if (state.kaydediliyor || !state.hazir || !ui.gorevFormu.reportValidity()) return;
    if (!ui.baslik.value.trim()) { mesaj('Görev başlığı boş olamaz.', 'error'); ui.baslik.focus(); return; }
    const payload = { personelId: Number(ui.personel.value), baslik: ui.baslik.value.trim(), aciklama: ui.aciklama.value.trim() || null };
    state.kaydediliyor = true; formDurumu();
    try {
        const gorev = await istek('/api/Gorev', { method: 'POST', body: payload });
        state.gorevler.unshift(gorev); ui.baslik.value = ''; ui.aciklama.value = ''; ui.karakter.textContent = '0'; tabloyuCiz(); mesaj('Görev başarıyla eklendi.');
    } catch (error) { mesaj(error.message, 'error'); }
    finally { state.kaydediliyor = false; formDurumu(); }
}

async function durumDegistir(gorev, yeniDurum) {
    if (state.bekleyenler.has(gorev.id)) return;
    state.bekleyenler.add(gorev.id); tabloyuCiz();
    try {
        const updated = await istek(`/api/Gorev/${gorev.id}`, { method: 'PUT', body: { durum: yeniDurum }, version: gorev.rowVersion });
        state.gorevler = state.gorevler.map(g => g.id === gorev.id ? updated : g);
        mesaj('Görev durumu güncellendi.');
    } catch (error) {
        if (error.status === 412 || error.status === 404) {
            const ok = await yenile({ sessiz: true });
            if (ok) mesaj(`${error.message} Liste güncellendi; kaydı kontrol edip yeniden işlem yapabilirsin.`, 'warning');
        } else mesaj(error.message, 'error');
    } finally { state.bekleyenler.delete(gorev.id); tabloyuCiz(); }
}

async function gorevSil(gorev) {
    if (state.bekleyenler.has(gorev.id) || !window.confirm(`“${gorev.baslik}” görevini silmek istiyor musun?`)) return;
    state.bekleyenler.add(gorev.id); tabloyuCiz();
    try {
        await istek(`/api/Gorev/${gorev.id}`, { method: 'DELETE', version: gorev.rowVersion });
        state.gorevler = state.gorevler.filter(g => g.id !== gorev.id); mesaj('Görev silindi.');
    } catch (error) {
        if (error.status === 412 || error.status === 404) {
            const ok = await yenile({ sessiz: true });
            if (ok) mesaj(`${error.message} Liste güncellendi; silmeden önce kaydı yeniden incele.`, 'warning');
        } else mesaj(error.message, 'error');
    } finally { state.bekleyenler.delete(gorev.id); tabloyuCiz(); }
}

ui.gorevFormu.addEventListener('submit', gorevEkle);
ui.yenile.addEventListener('click', () => yenile());
ui.personel.addEventListener('change', personelBilgisi);
ui.aciklama.addEventListener('input', () => ui.karakter.textContent = ui.aciklama.value.length);
ui.arama.addEventListener('input', tabloyuCiz);
ui['durum-filtre'].addEventListener('change', tabloyuCiz);
yenile({ sessiz: true });
