// PreMO control page logic.
//
// Flow:
//   1. GET /api/catalog  -> the object map (src/objects.rs): build the cards.
//   2. GET /api/devices  -> the sidebar list (refreshed every 3 s).
//   3. For the selected device, open the WebSocket /api/devices/<ip>/ws:
//        - first message {event:"state"} fills every control,
//        - then {event:"value"|"meters"|"link"|"log"|"progress"} as things change.
//   4. Moving a control sends {cmd:"set",key,value} over the same socket.
//
// The selected device is in the URL (/device/<ip>), so a device page can be
// bookmarked or opened directly.
'use strict';

const $ = (sel, root = document) => root.querySelector(sel);
const el = (tag, attrs = {}, ...kids) => {
  const e = document.createElement(tag);
  for (const [k, v] of Object.entries(attrs)) {
    if (k === 'class') e.className = v;
    else if (k.startsWith('on')) e.addEventListener(k.slice(2), v);
    else e.setAttribute(k, v);
  }
  for (const k of kids) e.append(k);
  return e;
};

let catalog = null;          // {controls, preset_layout, meters}
let currentIp = null;        // selected device
let ws = null;               // its WebSocket
const widgets = new Map();   // control key -> {set(value)}

// ---------------------------------------------------------------- meters --
// PPMMeter.cs: value is a signed dB byte, 0 = top, -60 = bottom. The bar
// index comes from this table (Bar_Number), indexed by |dB|. 40 segments:
// green below 16, orange 16-28, red 29+. Scale labels are (segment - 37).
const BAR_NUMBER = [40, 39, 38, 37, 36, 35, 34, 33, 32, 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21,
  20, 19, 18, 17, 16, 15, 14, 13, 12, 12, 11, 11, 10, 10, 9, 9, 8, 8, 7, 7,
  6, 6, 6, 5, 5, 5, 4, 4, 4, 4, 3, 3, 3, 3, 2, 2, 2, 2, 1, 1, 0];
const barFor = (db) => BAR_NUMBER[Math.min(60, Math.abs(Math.min(0, db)))];
const SCALE = [[40, '+3'], [37, '0'], [32, '-5'], [27, '-10'], [22, '-15'], [17, '-20'], [12, '-25'], [10, '-30'], [7, '-35'], [4, '-45'], [1, '-55']];
const meterEls = [];

function buildMeters() {
  const box = $('#meters');
  box.innerHTML = '';
  meterEls.length = 0;
  catalog.meters.forEach((name) => {
    const leds = [];
    const bar = el('div', { class: 'bar', role: 'img', 'aria-label': `${name} level` });
    for (let i = 0; i < 40; i++) {
      const d = el('div', { class: 'led' });
      bar.append(d);
      leds.push(d);
    }
    const scale = el('div', { class: 'scale', 'aria-hidden': 'true' });
    // Each label sits at the middle of its segment (segment n of 40, from the bottom).
    for (const [n, text] of SCALE) {
      const s = el('span', {}, text);
      s.style.bottom = `${((n - 0.5) / 40) * 100}%`;
      scale.append(s);
    }
    const num = el('div', { class: 'num' }, '—');
    box.append(el('div', { class: 'meter' }, scale, bar, el('div', { class: 'name' }, name), num));
    meterEls.push({ leds, num });
  });
  drawMeters({ channels: [[-60, -60], [-60, -60], [-60, -60], [-60, -60]] });
}

function ledColour(i, lit) {
  const zone = i < 16 ? 'green' : i < 29 ? 'amber' : 'red';
  return `var(--ppm-${zone}${lit ? '' : '-off'})`;
}

function drawMeters(m) {
  m.channels.forEach(([level, peak], ch) => {
    const me = meterEls[ch];
    if (!me) return;
    const lit = barFor(level), hold = barFor(peak);
    me.leds.forEach((d, i) => {
      // Segment i (0-based) is lit up to the level, plus the peak-hold one.
      d.style.background = ledColour(i, i < lit || i === hold - 1);
    });
    me.num.textContent = `${level} / ${peak}`;
  });
}

// --------------------------------------------------------------- controls --
// Headset routing 0-5 used Resources _1.._6, upload routing _7.._12.
const ROUTE_IMG = { headset_route: 1, upload_route: 7 };

function send(obj) {
  if (ws && ws.readyState === WebSocket.OPEN) ws.send(JSON.stringify(obj));
}

function controlWidget(c) {
  const row = el('div', { class: 'ctl', title: c.note || '' });
  const id = `c-${c.key}`;
  const label = el('label', { for: id }, c.label);
  const val = el('span', { class: 'val' }, '—');
  const flash = () => { row.classList.add('flash'); setTimeout(() => row.classList.remove('flash'), 400); };
  let set;

  if (c.kind.type === 'switch') {
    const input = el('input', { type: 'checkbox', id, role: 'switch' });
    if (!c.writable) input.disabled = true;
    input.addEventListener('change', () => send({ cmd: 'set', key: c.key, value: input.checked ? 1 : 0 }));
    row.append(label, el('span', { class: 'toggle' }, input, el('span')));
    set = (v) => { input.checked = !!v; flash(); };
  } else if (c.kind.type === 'choice') {
    const seg = el('div', { class: 'seg', role: 'radiogroup', 'aria-label': c.label });
    const buttons = [];
    for (let i = 0; i <= c.kind.max; i++) {
      const b = el('button', { type: 'button', role: 'radio', 'aria-checked': 'false' }, String(i));
      b.addEventListener('click', () => send({ cmd: 'set', key: c.key, value: i }));
      buttons.push(b);
      seg.append(b);
    }
    row.append(el('label', {}, c.label), val, seg);
    let img = null;
    if (ROUTE_IMG[c.key]) {
      img = el('img', { class: 'route-img', alt: `${c.label} diagram` });
      row.append(img);
    }
    set = (v) => {
      buttons.forEach((b, i) => { b.classList.toggle('on', i === v); b.setAttribute('aria-checked', String(i === v)); });
      val.textContent = String(v);
      if (img) img.src = `/assets/img/route_${String(ROUTE_IMG[c.key] + v).padStart(2, '0')}.jpg`;
      flash();
    };
  } else {
    // level / signed: slider. Send when released (PreMo.exe sent on MouseUp).
    const { min, max, unit } = c.kind;
    const input = el('input', { type: 'range', id, min, max, step: 1, value: min });
    if (!c.writable) input.disabled = true;
    const show = (v) => { val.textContent = `${v > 0 && c.kind.type === 'signed' ? '+' : ''}${v}${unit ? ' ' + unit : ''}`; };
    input.addEventListener('input', () => show(+input.value));
    input.addEventListener('change', () => send({ cmd: 'set', key: c.key, value: +input.value }));
    row.append(label, val, input);
    set = (v) => { if (document.activeElement !== input) input.value = v; show(v); flash(); };
  }
  widgets.set(c.key, { set });
  return row;
}

function buildGroups() {
  const box = $('#groups');
  box.innerHTML = '';
  const groups = new Map();
  for (const c of catalog.controls) {
    if (!groups.has(c.group)) groups.set(c.group, []);
    groups.get(c.group).push(c);
  }
  for (const [name, list] of groups) {
    const card = el('section', { class: 'card' }, el('h3', {}, name));
    list.forEach((c) => card.append(controlWidget(c)));
    box.append(card);
  }
}

// ---------------------------------------------------------------- devices --
async function refreshDevices() {
  try {
    const list = await (await fetch('/api/devices')).json();
    const ul = $('#devices');
    ul.innerHTML = '';
    if (!list.length) ul.append(el('li', { class: 'muted' }, 'No devices yet'));
    for (const d of list) {
      const a = el('a', { href: `/device/${d.ip}`, class: d.ip === currentIp ? 'active' : '' },
        el('span', {}, d.name), el('span', { class: 'ip' }, `${d.ip} · ${d.link}`));
      a.addEventListener('click', (e) => { e.preventDefault(); select(d.ip); });
      ul.append(el('li', {}, a));
    }
  } catch { /* server restarting */ }
}

function log(text) {
  const pre = $('#log');
  const t = new Date().toLocaleTimeString();
  pre.textContent = `${t}  ${text}\n` + pre.textContent.slice(0, 20000);
}

function setLink(link) {
  const p = $('#dev-link');
  p.textContent = link;
  p.className = `pill ${link}`;
}

function applyState(s) {
  $('#dev-name').textContent = s.name;
  $('#dev-ip').textContent = s.ip;
  $('#dev-web').href = `http://${s.ip}/`;
  setLink(s.link);
  for (const [k, v] of Object.entries(s.values)) widgets.get(k)?.set(v);
  drawMeters(s.meters);
  if (s.preset !== null) $('#preset-status').textContent = `Last loaded: preset ${s.preset}`;
}

function select(ip, push = true) {
  currentIp = ip;
  if (push) history.pushState({}, '', `/device/${ip}`);
  $('#empty').hidden = true;
  $('#device').hidden = false;
  $('#log').textContent = '';
  if (ws) ws.close();
  const proto = location.protocol === 'https:' ? 'wss' : 'ws';
  ws = new WebSocket(`${proto}://${location.host}/api/devices/${ip}/ws`);
  ws.onmessage = (m) => {
    const e = JSON.parse(m.data);
    switch (e.event) {
      case 'state': applyState(e.state); break;
      case 'value': widgets.get(e.key)?.set(e.value); break;
      case 'meters': drawMeters(e.meters); break;
      case 'link': setLink(e.link); log(`link: ${e.link}`); break;
      case 'log': log(e.text); break;
      case 'error': log(`error: ${e.text}`); break;
      case 'progress': $('#fw-progress').value = e.percent; break;
    }
  };
  ws.onclose = () => { if (currentIp === ip) setTimeout(() => currentIp === ip && select(ip, false), 2000); };
  refreshDevices();
}

// ------------------------------------------------------------------ forms --
$('#add-device').addEventListener('submit', async (e) => {
  e.preventDefault();
  const ip = $('#add-ip').value.trim();
  const r = await fetch('/api/devices', { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify({ ip }) });
  if (r.ok) { $('#add-ip').value = ''; select(ip); } else alert((await r.json()).error);
});

document.querySelectorAll('#presets button').forEach((b) => b.addEventListener('click', () => {
  const num = +$('#preset-num').value;
  send({ cmd: 'preset', op: b.dataset.op, num });
  log(`preset ${b.dataset.op} ${num}`);
}));

$('#raw-go').addEventListener('click', () => {
  const block = +$('#raw-block').value, ono = +$('#raw-ono').value, value = +$('#raw-value').value;
  send({ cmd: 'raw', block, ono, value });
  log(`raw: block ${block} object ${ono} value ${value}`);
});

$('#fw-go').addEventListener('click', async () => {
  const f = $('#fw-file').files[0];
  if (!f) return alert('Choose a firmware .bin file first.');
  if (!confirm(`Update ${currentIp} with ${f.name} (${f.size} bytes)?`)) return;
  const q = `reboot=${$('#fw-reboot').checked}&delay_ms=${+$('#fw-delay').value}`;
  const r = await fetch(`/api/devices/${currentIp}/firmware?${q}`, { method: 'POST', body: f });
  log(r.ok ? `update started: ${f.name}` : `update refused: ${(await r.json()).error}`);
});

window.addEventListener('popstate', () => {
  const m = location.pathname.match(/^\/device\/([\d.]+)/);
  if (m) select(m[1], false);
});

// ------------------------------------------------------------------- boot --
(async () => {
  catalog = await (await fetch('/api/catalog')).json();
  buildMeters();
  buildGroups();
  await refreshDevices();
  setInterval(refreshDevices, 3000);
  const m = location.pathname.match(/^\/device\/([\d.]+)/);
  if (m) select(m[1], false);
})();
