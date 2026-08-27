const db = require('./db');
const { haversineDistanceM } = require('./geo');

const getGeofences = db.prepare('SELECT * FROM geofences');
const getState = db.prepare(
  'SELECT inside FROM vehicle_geofence_state WHERE vehicle_id = ? AND geofence_id = ?'
);
const upsertState = db.prepare(`
  INSERT INTO vehicle_geofence_state (vehicle_id, geofence_id, inside)
  VALUES (?, ?, ?)
  ON CONFLICT(vehicle_id, geofence_id) DO UPDATE SET inside = excluded.inside
`);
const insertAlert = db.prepare(
  'INSERT INTO alerts (vehicle_id, type, message) VALUES (?, ?, ?)'
);

const SPEEDING_LIMIT_KMH = 100;

function checkPing(vehicle, lat, lng, speedKmh) {
  const alerts = [];
  const fences = getGeofences.all();

  for (const fence of fences) {
    const dist = haversineDistanceM(lat, lng, fence.lat, fence.lng);
    const isInside = dist <= fence.radius_m ? 1 : 0;
    const prev = getState.get(vehicle.id, fence.id);
    const wasInside = prev ? prev.inside : 0;

    if (isInside !== wasInside) {
      const type = isInside ? 'geofence_enter' : 'geofence_exit';
      const message = isInside
        ? `${vehicle.label} (${vehicle.plate}) entro a la zona "${fence.name}"`
        : `${vehicle.label} (${vehicle.plate}) salio de la zona "${fence.name}"`;
      insertAlert.run(vehicle.id, type, message);
      alerts.push({ type, message, vehicleId: vehicle.id, geofenceId: fence.id, createdAt: new Date().toISOString() });
    }
    upsertState.run(vehicle.id, fence.id, isInside);
  }

  if (speedKmh != null && speedKmh > SPEEDING_LIMIT_KMH) {
    const message = `${vehicle.label} (${vehicle.plate}) exceso de velocidad: ${speedKmh.toFixed(0)} km/h`;
    insertAlert.run(vehicle.id, 'speeding', message);
    alerts.push({ type: 'speeding', message, vehicleId: vehicle.id, createdAt: new Date().toISOString() });
  }

  return alerts;
}

module.exports = { checkPing };
