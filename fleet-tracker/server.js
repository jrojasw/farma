const path = require('path');
const express = require('express');
const http = require('http');
const { Server } = require('socket.io');
const db = require('./src/db');
const buildApiRouter = require('./routes/api');

const app = express();
const server = http.createServer(app);
const io = new Server(server, { cors: { origin: '*' } });

app.use(express.json());
app.use(express.static(path.join(__dirname, 'public')));
app.use('/vendor/leaflet', express.static(path.join(__dirname, 'node_modules/leaflet/dist')));
app.use('/api', buildApiRouter(io));

// Mark vehicles offline if no ping in the last 2 minutes.
setInterval(() => {
  const cutoff = new Date(Date.now() - 2 * 60 * 1000).toISOString();
  const stale = db
    .prepare("SELECT id FROM vehicles WHERE status = 'active' AND (last_seen_at IS NULL OR last_seen_at < ?)")
    .all(cutoff);
  const markOffline = db.prepare("UPDATE vehicles SET status = 'offline' WHERE id = ?");
  for (const v of stale) {
    markOffline.run(v.id);
    io.emit('vehicle:offline', { id: v.id });
  }
}, 30 * 1000);

io.on('connection', (socket) => {
  const vehicles = db
    .prepare('SELECT * FROM vehicles ORDER BY id')
    .all()
    .map(({ api_key, ...rest }) => rest);
  socket.emit('bootstrap', {
    vehicles,
    geofences: db.prepare('SELECT * FROM geofences').all(),
    alerts: db.prepare('SELECT * FROM alerts ORDER BY id DESC LIMIT 20').all(),
  });
});

const PORT = process.env.PORT || 4000;
server.listen(PORT, () => {
  console.log(`Fleet Tracker escuchando en http://localhost:${PORT}`);
});
