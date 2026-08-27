const DEFAULT_CENTER = [-34.6037, -58.3816]; // Buenos Aires, ajustable
const socket = io();

const map = L.map('map').setView(DEFAULT_CENTER, 13);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
  attribution: '&copy; OpenStreetMap contributors',
  maxZoom: 19,
}).addTo(map);

const vehicleMarkers = new Map(); // id -> marker
const fenceCircles = new Map(); // id -> circle
const vehiclesById = new Map();
let historyLayer = null;
let historyPoints = [];
let historyTimer = null;
let pendingFenceLatLng = null;

const el = (id) => document.getElementById(id);
const connStatus = el('connStatus');

socket.on('connect', () => {
  connStatus.textContent = 'En vivo';
  connStatus.classList.add('connected');
});
socket.on('disconnect', () => {
  connStatus.textContent = 'Desconectado';
  connStatus.classList.remove('connected');
});

function vehicleIcon(status) {
  const color = status === 'active' ? '#35d07f' : status === 'stopped' ? '#ffb648' : '#8b93ab';
  return L.divIcon({
    className: '',
    html: `<div style="width:16px;height:16px;border-radius:50%;background:${color};border:2px solid white;box-shadow:0 0 6px rgba(0,0,0,.5)"></div>`,
    iconSize: [16, 16],
    iconAnchor: [8, 8],
  });
}

function upsertVehicleMarker(v) {
  vehiclesById.set(v.id, v);
  if (v.last_lat == null || v.last_lng == null) return;
  const latlng = [v.last_lat, v.last_lng];
  let marker = vehicleMarkers.get(v.id);
  if (!marker) {
    marker = L.marker(latlng, { icon: vehicleIcon(v.status) }).addTo(map);
    marker.bindTooltip(v.label, { permanent: false, className: 'vehicle-marker-label' });
    vehicleMarkers.set(v.id, marker);
  } else {
    marker.setLatLng(latlng);
    marker.setIcon(vehicleIcon(v.status));
  }
  marker.bindPopup(
    `<strong>${v.label}</strong> (${v.plate})<br/>Conductor: ${v.driver_name || '—'}<br/>Velocidad: ${(v.last_speed || 0).toFixed(0)} km/h<br/>Estado: ${v.status}`
  );
}

function removeVehicleMarker(id) {
  const marker = vehicleMarkers.get(id);
  if (marker) {
    map.removeLayer(marker);
    vehicleMarkers.delete(id);
  }
  vehiclesById.delete(id);
}

function renderVehicleList() {
  const list = el('vehicleList');
  list.innerHTML = '';
  let active = 0;
  let offline = 0;
  for (const v of vehiclesById.values()) {
    if (v.status === 'active') active++;
    else offline++;
    const li = document.createElement('li');
    li.className = 'vehicle-item';
    li.innerHTML = `
      <span class="status-dot ${v.status}"></span>
      <div class="vehicle-info">
        <span class="name">${v.label}</span>
        <span class="meta">${v.plate} · ${v.driver_name || 'sin conductor'}</span>
      </div>
      <span class="vehicle-speed">${v.last_speed ? v.last_speed.toFixed(0) + ' km/h' : '—'}</span>
    `;
    li.addEventListener('click', () => {
      if (v.last_lat != null) map.setView([v.last_lat, v.last_lng], 15);
      vehicleMarkers.get(v.id)?.openPopup();
    });
    list.appendChild(li);
  }
  el('statActive').textContent = active;
  el('statOffline').textContent = offline;
  renderHistorySelect();
}

function renderHistorySelect() {
  const select = el('historyVehicleSelect');
  const current = select.value;
  select.innerHTML = '';
  for (const v of vehiclesById.values()) {
    const opt = document.createElement('option');
    opt.value = v.id;
    opt.textContent = `${v.label} (${v.plate})`;
    select.appendChild(opt);
  }
  if (current) select.value = current;
}

function upsertFenceCircle(f) {
  let circle = fenceCircles.get(f.id);
  if (!circle) {
    circle = L.circle([f.lat, f.lng], {
      radius: f.radius_m,
      color: '#4f8cff',
      fillOpacity: 0.1,
    }).addTo(map);
    circle.bindTooltip(f.name);
    fenceCircles.set(f.id, circle);
  }
}

function removeFenceCircle(id) {
  const c = fenceCircles.get(id);
  if (c) {
    map.removeLayer(c);
    fenceCircles.delete(id);
  }
}

function renderFenceList(fences) {
  const list = el('fenceList');
  list.innerHTML = '';
  for (const f of fences) {
    upsertFenceCircle(f);
    const li = document.createElement('li');
    li.className = 'fence-item';
    li.innerHTML = `<span>${f.name} (${f.radius_m}m)</span>`;
    const del = document.createElement('button');
    del.textContent = '✕';
    del.addEventListener('click', () =>
      fetch(`/api/geofences/${f.id}`, { method: 'DELETE' })
    );
    li.appendChild(del);
    list.appendChild(li);
  }
}

let alertCount = 0;
function prependAlert(a) {
  alertCount++;
  el('statAlerts').textContent = alertCount;
  const list = el('alertList');
  const li = document.createElement('li');
  li.className = `alert-item ${a.type}`;
  const time = new Date(a.created_at || a.createdAt || Date.now()).toLocaleTimeString();
  li.innerHTML = `${a.message}<span class="alert-time">${time}</span>`;
  list.prepend(li);
  while (list.children.length > 40) list.removeChild(list.lastChild);
}

// ---- Socket events ----
socket.on('bootstrap', (data) => {
  for (const v of data.vehicles) upsertVehicleMarker(v);
  renderVehicleList();
  renderFenceList(data.geofences);
  for (const a of data.alerts.reverse()) prependAlert(a);
});
socket.on('vehicle:position', (v) => {
  upsertVehicleMarker(v);
  renderVehicleList();
});
socket.on('vehicle:created', (v) => {
  upsertVehicleMarker(v);
  renderVehicleList();
});
socket.on('vehicle:deleted', ({ id }) => {
  removeVehicleMarker(id);
  renderVehicleList();
});
socket.on('vehicle:offline', ({ id }) => {
  const v = vehiclesById.get(id);
  if (v) {
    v.status = 'offline';
    upsertVehicleMarker(v);
    renderVehicleList();
  }
});
socket.on('geofence:created', (f) => upsertFenceCircle(f));
socket.on('geofence:deleted', ({ id }) => removeFenceCircle(id));
socket.on('alert:new', (a) => prependAlert(a));

// ---- Add vehicle dialog ----
const vehicleDialog = el('vehicleDialog');
el('btnAddVehicle').addEventListener('click', () => vehicleDialog.showModal());
el('btnCancelVehicle').addEventListener('click', () => vehicleDialog.close());
el('vehicleForm').addEventListener('submit', async (e) => {
  const form = new FormData(e.target);
  const body = Object.fromEntries(form.entries());
  await fetch('/api/vehicles', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  e.target.reset();
});

// ---- Add geofence dialog ----
const fenceDialog = el('fenceDialog');
let drawingFence = false;
el('btnDrawFence').addEventListener('click', () => {
  drawingFence = true;
  connStatus.textContent = 'Click en el mapa para ubicar la zona…';
});
map.on('click', (e) => {
  if (!drawingFence) return;
  pendingFenceLatLng = e.latlng;
  drawingFence = false;
  connStatus.textContent = socket.connected ? 'En vivo' : 'Desconectado';
  fenceDialog.showModal();
});
el('btnCancelFence').addEventListener('click', () => fenceDialog.close());
el('fenceForm').addEventListener('submit', async (e) => {
  const form = new FormData(e.target);
  const body = {
    name: form.get('name'),
    radiusM: Number(form.get('radiusM')),
    lat: pendingFenceLatLng.lat,
    lng: pendingFenceLatLng.lng,
  };
  await fetch('/api/geofences', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  e.target.reset();
});

// ---- History playback ----
el('btnLoadHistory').addEventListener('click', async () => {
  const vehicleId = el('historyVehicleSelect').value;
  if (!vehicleId) return;
  const res = await fetch(`/api/vehicles/${vehicleId}/history`);
  historyPoints = await res.json();
  if (historyLayer) map.removeLayer(historyLayer);
  if (!historyPoints.length) return;
  const latlngs = historyPoints.map((p) => [p.lat, p.lng]);
  historyLayer = L.polyline(latlngs, { color: '#ff5c6c', weight: 3 }).addTo(map);
  map.fitBounds(historyLayer.getBounds(), { padding: [30, 30] });

  const slider = el('historySlider');
  slider.max = historyPoints.length - 1;
  slider.value = 0;
  el('historyPlayer').hidden = false;
  updateHistoryFrame(0);
});

el('btnClearHistory').addEventListener('click', () => {
  if (historyLayer) map.removeLayer(historyLayer);
  historyLayer = null;
  historyPoints = [];
  el('historyPlayer').hidden = true;
  clearInterval(historyTimer);
});

let historyMarker = null;
function updateHistoryFrame(idx) {
  const p = historyPoints[idx];
  if (!p) return;
  if (!historyMarker) {
    historyMarker = L.circleMarker([p.lat, p.lng], { radius: 7, color: '#ff5c6c' }).addTo(map);
  } else {
    historyMarker.setLatLng([p.lat, p.lng]);
  }
  el('historyTimestamp').textContent = new Date(p.recorded_at).toLocaleString();
}

el('historySlider').addEventListener('input', (e) => updateHistoryFrame(Number(e.target.value)));

el('btnPlayHistory').addEventListener('click', (e) => {
  const slider = el('historySlider');
  if (historyTimer) {
    clearInterval(historyTimer);
    historyTimer = null;
    e.target.textContent = '▶ Reproducir';
    return;
  }
  e.target.textContent = '⏸ Pausar';
  historyTimer = setInterval(() => {
    let val = Number(slider.value) + 1;
    if (val > Number(slider.max)) {
      clearInterval(historyTimer);
      historyTimer = null;
      e.target.textContent = '▶ Reproducir';
      return;
    }
    slider.value = val;
    updateHistoryFrame(val);
  }, 300);
});
