const express = require('express');
const crypto = require('crypto');
const db = require('../src/db');
const { checkPing } = require('../src/geofenceChecker');

function generateApiKey() {
  return crypto.randomBytes(16).toString('hex');
}

function sanitizeVehicle(vehicle) {
  if (!vehicle) return vehicle;
  const { api_key, ...rest } = vehicle;
  return rest;
}

module.exports = function buildApiRouter(io) {
  const router = express.Router();

  // ---- Vehicles ----
  router.get('/vehicles', (req, res) => {
    const vehicles = db.prepare('SELECT * FROM vehicles ORDER BY id').all();
    res.json(vehicles.map(sanitizeVehicle));
  });

  router.post('/vehicles', (req, res) => {
    const { plate, label, driverName, type } = req.body || {};
    if (!plate || !label) {
      return res.status(400).json({ error: 'plate y label son requeridos' });
    }
    const apiKey = generateApiKey();
    try {
      const info = db
        .prepare(
          'INSERT INTO vehicles (plate, label, driver_name, type, api_key) VALUES (?, ?, ?, ?, ?)'
        )
        .run(plate, label, driverName || null, type || 'van', apiKey);
      const vehicle = db.prepare('SELECT * FROM vehicles WHERE id = ?').get(info.lastInsertRowid);
      io.emit('vehicle:created', sanitizeVehicle(vehicle));
      // La api_key se devuelve una sola vez en esta respuesta para configurar el tracker/app real.
      res.status(201).json(vehicle);
    } catch (err) {
      res.status(400).json({ error: 'La patente ya existe o los datos son invalidos' });
    }
  });

  router.delete('/vehicles/:id', (req, res) => {
    db.prepare('DELETE FROM vehicles WHERE id = ?').run(req.params.id);
    io.emit('vehicle:deleted', { id: Number(req.params.id) });
    res.status(204).end();
  });

  // ---- GPS ingestion: real trackers / phone apps POST here ----
  router.post('/gps/ping', (req, res) => {
    const { apiKey, lat, lng, speed, heading } = req.body || {};
    if (!apiKey || lat == null || lng == null) {
      return res.status(400).json({ error: 'apiKey, lat y lng son requeridos' });
    }
    const vehicle = db.prepare('SELECT * FROM vehicles WHERE api_key = ?').get(apiKey);
    if (!vehicle) return res.status(401).json({ error: 'apiKey invalida' });

    const now = new Date().toISOString();
    db.prepare(
      'INSERT INTO locations (vehicle_id, lat, lng, speed, heading, recorded_at) VALUES (?, ?, ?, ?, ?, ?)'
    ).run(vehicle.id, lat, lng, speed || 0, heading || 0, now);

    db.prepare(
      `UPDATE vehicles SET last_lat = ?, last_lng = ?, last_speed = ?, last_heading = ?,
       last_seen_at = ?, status = 'active' WHERE id = ?`
    ).run(lat, lng, speed || 0, heading || 0, now, vehicle.id);

    const updated = db.prepare('SELECT * FROM vehicles WHERE id = ?').get(vehicle.id);
    io.emit('vehicle:position', sanitizeVehicle(updated));

    const alerts = checkPing(vehicle, lat, lng, speed);
    for (const alert of alerts) io.emit('alert:new', alert);

    res.status(202).json({ ok: true });
  });

  // ---- History playback ----
  router.get('/vehicles/:id/history', (req, res) => {
    const { from, to } = req.query;
    let query = 'SELECT lat, lng, speed, heading, recorded_at FROM locations WHERE vehicle_id = ?';
    const params = [req.params.id];
    if (from) {
      query += ' AND recorded_at >= ?';
      params.push(from);
    }
    if (to) {
      query += ' AND recorded_at <= ?';
      params.push(to);
    }
    query += ' ORDER BY recorded_at ASC';
    res.json(db.prepare(query).all(...params));
  });

  // ---- Geofences ----
  router.get('/geofences', (req, res) => {
    res.json(db.prepare('SELECT * FROM geofences').all());
  });

  router.post('/geofences', (req, res) => {
    const { name, lat, lng, radiusM } = req.body || {};
    if (!name || lat == null || lng == null || !radiusM) {
      return res.status(400).json({ error: 'name, lat, lng y radiusM son requeridos' });
    }
    const info = db
      .prepare('INSERT INTO geofences (name, lat, lng, radius_m) VALUES (?, ?, ?, ?)')
      .run(name, lat, lng, radiusM);
    const fence = db.prepare('SELECT * FROM geofences WHERE id = ?').get(info.lastInsertRowid);
    io.emit('geofence:created', fence);
    res.status(201).json(fence);
  });

  router.delete('/geofences/:id', (req, res) => {
    db.prepare('DELETE FROM geofences WHERE id = ?').run(req.params.id);
    io.emit('geofence:deleted', { id: Number(req.params.id) });
    res.status(204).end();
  });

  // ---- Alerts ----
  router.get('/alerts', (req, res) => {
    const limit = Number(req.query.limit) || 50;
    res.json(
      db
        .prepare('SELECT * FROM alerts ORDER BY id DESC LIMIT ?')
        .all(limit)
    );
  });

  return router;
};
