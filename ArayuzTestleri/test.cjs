const { JSDOM, VirtualConsole } = require('jsdom');
const fs = require('node:fs');
const assert = require('node:assert/strict');
const path = require('node:path');
const root = path.join(__dirname, '..', 'StajyerTakip.API', 'wwwroot');
const pause = ms => new Promise(resolve => setTimeout(resolve, ms));
const checks = [];
async function waitFor(predicate, name) {
    for (let i=0;i<300;i++) { if (predicate()) return; await pause(10); }
    throw new Error('Timed out: '+name);
}
const version = n => {const b=Buffer.alloc(8);b.writeBigInt64BE(BigInt(n));return b.toString('base64');};
let tick=3, nextId=3, mode='normal', errorMessages=[];
const people=[{id:7,adSoyad:'Demo Personel',departmanAdi:'Yazılım',pozisyonAdi:'Stajyer'}, {id:42,adSoyad:'İkinci Personel',departmanAdi:'Yazılım',pozisyonAdi:'Geliştirici'}];
const rows=[{id:1,personelId:7,baslik:'API testleri',aciklama:'Yanıtları kontrol et',durum:'Yapılacak',rowVersion:version(1)},{id:2,personelId:42,baslik:'Dokümantasyon',aciklama:null,durum:'Devam Ediyor',rowVersion:version(2)},{id:3,personelId:7,baslik:'Veritabanı tasarımı',aciklama:null,durum:'Tamamlandı',rowVersion:version(3)}];
const calls=[];
const json=(status,data) => new Response(data===undefined?null:JSON.stringify(data),{status,headers:{'Content-Type':'application/json'}});
async function mockFetch(url, options) {
    const method=options.method || 'GET';calls.push({url,method,headers:options.headers});
    if(mode==='error') return json(503,{detail:'Azure SQL kullanıcı adı ve parolasını kontrol edin.'});
    if(mode==='timeout') {const e=new Error('timeout');e.name='AbortError';throw e;}
    if(url==='/api/Personel') return json(200,people);
    if(url==='/api/Gorev' && method==='GET') return json(200,rows.filter(r=>!r.deleted));
    if(url==='/api/Gorev' && method==='POST') {
        await pause(60);const data=JSON.parse(options.body);
        assert.deepEqual(Object.keys(data).sort(),['aciklama','baslik','personelId']);
        const row={...data,id:++nextId,durum:'Yapılacak',rowVersion:version(++tick)};rows.unshift(row);return json(201,row);
    }
    if(url.startsWith('/api/Gorev/')) {
        const id=Number(url.split('/').at(-1)), row=rows.find(r=>r.id===id&&!r.deleted);
        if(!row) return json(404,{detail:'Görev bulunamadı.'});
        if(options.headers['If-Match']!=='"'+row.rowVersion+'"') return json(412,{detail:'Bu görev başka bir işlemde değiştirildi.'});
        row.rowVersion=version(++tick);
        if(method==='PUT'){row.durum=JSON.parse(options.body).durum;return json(200,row);}
        if(method==='DELETE'){row.deleted=true;return json(204);}
    }
    throw new Error('Unexpected request '+method+' '+url);
}
async function main() {
    const vc=new VirtualConsole();vc.on('jsdomError',e=>errorMessages.push(e.message));
    const dom=new JSDOM(fs.readFileSync(root+'/index.html','utf8'),{url:'http://localhost:5237/',runScripts:'outside-only',virtualConsole:vc});
    const w=dom.window,d=w.document,$=id=>d.getElementById(id);
    w.fetch=mockFetch;w.confirm=()=>true;w.eval(fs.readFileSync(root+'/app.js','utf8'));
    const submit=()=>$('gorevFormu').dispatchEvent(new w.Event('submit',{bubbles:true,cancelable:true}));
    const input=(id,value)=>{$(id).value=value;$(id).dispatchEvent(new w.Event('input',{bubbles:true}));};
    const taskRow=id=>d.querySelector(`tr[data-id="${id}"]`);
    const change=(id,value)=>{const s=taskRow(id).querySelector('select');s.value=value;s.dispatchEvent(new w.Event('change',{bubbles:true}));};
    await waitFor(()=>$('toplam').textContent==='3'&&!$('kaydet').disabled,'initial load');
    assert.equal($('personel').options.length,3);checks.push('Initial task/staff loading and status counters');

    $('personel').value='42';input('baslik','  Mülakat demo görevi  ');input('aciklama','Açıklama');
    assert.equal($('karakter').textContent,'8');
    submit();submit();assert.equal($('kaydet').disabled,true);
    await waitFor(()=>taskRow(4)&&!$('kaydet').disabled,'create task');
    assert.equal(calls.filter(c=>c.method==='POST').length,1);
    assert(taskRow(4).textContent.includes('İkinci Personel'));
    assert.equal(rows.find(r=>r.id===4).baslik,'Mülakat demo görevi');
    assert.equal($('baslik').value,'');checks.push('Selected staff id, trimmed input, duplicate-submit protection and form reset');

    change(4,'Tamamlandı');await waitFor(()=>$('tamamlandi').textContent==='2','status change');
    assert.equal(taskRow(4).querySelector('select').value,'Tamamlandı');checks.push('Status update with current If-Match version');

    $('durum-filtre').value='Yapılacak';$('durum-filtre').dispatchEvent(new w.Event('change'));
    assert.equal(d.querySelectorAll('tr[data-id]').length,1);
    $('durum-filtre').value='';$('durum-filtre').dispatchEvent(new w.Event('change'));
    input('arama','İKİNCİ');assert.equal(d.querySelectorAll('tr[data-id]').length,2);input('arama','');
    checks.push('Status filtering and Turkish case-insensitive staff search');

    const record=rows.find(r=>r.id===4);record.durum='Devam Ediyor';record.rowVersion=version(++tick);
    change(4,'Yapılacak');await waitFor(()=>$('bildirim').classList.contains('warning'),'stale status warning');
    assert.equal(record.durum,'Devam Ediyor');assert.equal(taskRow(4).querySelector('select').value,'Devam Ediyor');
    checks.push('412 conflict reloads current data without overwriting another client');

    taskRow(4).querySelector('button').click();await waitFor(()=>!taskRow(4),'delete');
    assert.equal(record.deleted,true);checks.push('Delete confirmation and current-version delete');

    const title='<img src=x onerror="window.__xss=1">';input('baslik',title);submit();
    await waitFor(()=>taskRow(5),'literal HTML task');
    assert.equal(taskRow(5).querySelector('.task-title').textContent,title);
    assert.equal(d.querySelectorAll('#gorevTablosu img').length,0);assert.equal(w.__xss,undefined);
    checks.push('HTML-looking input remains text; no image or event handler is created');

    input('baslik','   ');const before=calls.filter(c=>c.method==='POST').length;submit();
    assert($('bildirim').textContent.includes('başlığı boş olamaz'));
    assert.equal(calls.filter(c=>c.method==='POST').length,before);checks.push('Blank-title validation sends no request');

    mode='error';$('yenile').click();await waitFor(()=>$('bildirim').textContent.includes('Azure SQL'),'503 error');
    assert.equal($('yenile').disabled,false);checks.push('503 error is visible and refresh is re-enabled');
    mode='normal';$('yenile').click();await waitFor(()=>$('bildirim').textContent==='Görev listesi güncellendi.','recovery');
    checks.push('Refresh recovers after a database error');

    mode='timeout';$('yenile').click();await waitFor(()=>$('bildirim').textContent.includes('20 saniyede'),'abort message');
    assert.equal($('yenile').disabled,false);checks.push('Timeout gives an actionable message and unlocks refresh');
    assert.deepEqual(errorMessages,[]);dom.window.close();
    const report={passed:checks.length,checks,jsdomErrors:errorMessages,scope:'DOM and JavaScript behavior with mocked API responses; not a real-browser visual or live Azure test.'};
    fs.writeFileSync(path.join(__dirname, 'results.json'),JSON.stringify(report,null,2));
    console.log(JSON.stringify(report,null,2));
}
main().catch(error=>{console.error(error);process.exitCode=1;});
