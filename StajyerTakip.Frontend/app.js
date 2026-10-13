const GorevAPI = {

    baseUrl: 'http://localhost:5237/api/Gorev',

    // TÜM GÖREVLERİ GETİR
    async getir() {

        const response = await fetch(this.baseUrl);

        if (!response.ok) {
            throw new Error('Veriler alınamadı');
        }

        const data = await response.json();

        return data;
    },

    // YENİ GÖREV EKLE
    async ekle(gorev) {

        const response = await fetch(this.baseUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(gorev)
        });

        if (!response.ok) {
            throw new Error('Kayıt başarısız');
        }
    },

    // GÖREV DURUMUNU GÜNCELLE
    async guncelle(id, yeniDurum) {

        const response = await fetch(`${this.baseUrl}/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(yeniDurum)
        });

        if (!response.ok) {
            throw new Error('Güncelleme başarısız');
        }
    },

    // GÖREVİ SİL
    async sil(id) {

        const response = await fetch(`${this.baseUrl}/${id}`, {
            method: 'DELETE'
        });

        if (!response.ok) {
            throw new Error('Silme işlemi başarısız');
        }
    }
};


const UIManager = {

    // TABLOYU OLUŞTUR
    tabloyuCiz(gorevler) {

        const tablo = document.getElementById('gorevTablosu');

        tablo.innerHTML = '';

        gorevler.forEach(gorev => {

            const gBaslik =
                gorev.Baslik ??
                gorev.baslik ??
                "Başlık Bulunamadı";

            const gAciklama =
                gorev.Aciklama ??
                gorev.aciklama ??
                "-";

            const gDurum =
                gorev.Durum ??
                gorev.durum ??
                "Yapılacak";

            const gId =
                gorev.Id ??
                gorev.id;

            tablo.innerHTML += `
                <tr>

                    <td class="fw-semibold">
                        ${gBaslik}
                    </td>

                    <td class="text-muted small">
                        ${gAciklama}
                    </td>

                    <td class="d-flex gap-2">

                        <select
                            class="form-select form-select-sm border-0 bg-light w-auto"
                            onchange="Uygulama.durumDegistir(${gId}, this.value)">

                            <option value="Yapılacak"
                                ${gDurum === 'Yapılacak' ? 'selected' : ''}>
                                Yapılacak
                            </option>

                            <option value="Devam Ediyor"
                                ${gDurum === 'Devam Ediyor' ? 'selected' : ''}>
                                Devam Ediyor
                            </option>

                            <option value="Tamamlandı"
                                ${gDurum === 'Tamamlandı' ? 'selected' : ''}>
                                Tamamlandı
                            </option>

                        </select>

                        <button
                            class="btn btn-sm btn-outline-danger border-0"
                            onclick="Uygulama.gorevSil(${gId})">

                            Sil

                        </button>

                    </td>

                </tr>
            `;
        });
    },


    // MESAJ GÖSTER
    mesajGoster(elementId, mesaj, tip) {

        const element = document.getElementById(elementId);

        element.innerHTML = `
            <div class="alert alert-${tip} py-2">
                ${mesaj}
            </div>
        `;

        setTimeout(() => {
            element.innerHTML = '';
        }, 2500);
    },


    // HATA GÖSTER
    hataGoster() {

        const tablo = document.getElementById('gorevTablosu');

        tablo.innerHTML = `
            <tr>
                <td colspan="3" class="text-center text-danger">
                    Sunucu bağlantı hatası!
                </td>
            </tr>
        `;
    }
};


const Uygulama = {

    // UYGULAMAYI BAŞLAT
    async baslat() {

        try {

            const gorevler = await GorevAPI.getir();

            UIManager.tabloyuCiz(gorevler);

        } catch (error) {

            console.error("Görevler alınırken hata:", error);

            UIManager.hataGoster();
        }
    },


    // GÖREV EKLE
    async gorevEkle(e) {

        e.preventDefault();

        const yeniGorev = {

            PersonelId: 1,

            Baslik:
                document.getElementById('baslik').value,

            Aciklama:
                document.getElementById('aciklama').value,

            Durum: "Yapılacak"
        };


        try {

            await GorevAPI.ekle(yeniGorev);

            document
                .getElementById('gorevFormu')
                .reset();

            UIManager.mesajGoster(
                'formSonuc',
                'Görev başarıyla eklendi!',
                'success'
            );

            await this.baslat();

        } catch (error) {

            console.error("Görev ekleme hatası:", error);

            UIManager.mesajGoster(
                'formSonuc',
                'Hata: Görev eklenemedi.',
                'danger'
            );
        }
    },


    // DURUM DEĞİŞTİR
    async durumDegistir(id, yeniDurum) {

        try {

            await GorevAPI.guncelle(
                id,
                yeniDurum
            );

            UIManager.mesajGoster(
                'listeSonuc',
                'Durum güncellendi.',
                'info'
            );

        } catch (error) {

            console.error(
                "Durum güncelleme hatası:",
                error
            );

            UIManager.mesajGoster(
                'listeSonuc',
                'Hata: Güncellenemedi.',
                'danger'
            );
        }
    },


    // GÖREV SİL
    async gorevSil(id) {

        if (
            confirm(
                "Bu görevi silmek istediğinize emin misiniz?"
            )
        ) {

            try {

                await GorevAPI.sil(id);

                UIManager.mesajGoster(
                    'listeSonuc',
                    'Görev başarıyla silindi.',
                    'success'
                );

                await this.baslat();

            } catch (error) {

                console.error(
                    "Görev silme hatası:",
                    error
                );

                UIManager.mesajGoster(
                    'listeSonuc',
                    'Hata: Görev silinemedi.',
                    'danger'
                );
            }
        }
    }
};


// FORM GÖNDERME
document
    .getElementById('gorevFormu')
    .addEventListener(
        'submit',
        (e) => Uygulama.gorevEkle(e)
    );


// SAYFA YÜKLENDİĞİNDE GÖREVLERİ GETİR
document.addEventListener(
    'DOMContentLoaded',
    () => Uygulama.baslat()
);