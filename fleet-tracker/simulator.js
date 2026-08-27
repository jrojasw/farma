/*
 * Simulador de flota: crea vehiculos de demo y les hace recorrer rutas ficticias,
 * enviando pings al mismo endpoint HTTP que usaria un tracker GPS real o una app
 * movil (POST /api/gps/ping). Sirve para ver el dashboard en vivo sin hardware.
 * Cuando conectes GPS reales, simplemente no ejecutes este script.
 */
const db = require('./src/db');

const BASE_URL = process.env.FLEET_URL || 'http://localhost:4000';
const TICK_MS = 2000;

const DEMO_VEHICLES = [
  { plate: 'AB123CD', label: 'Furgon Norte', driverName: 'Marcos Diaz', type: 'van' },
  { plate: 'XY987ZT', label: 'Camion Distribucion', driverName: 'Lucia Fernandez', type: 'truck' },
  { plate: 'QW456ER', label: 'Auto Supervisor', driverName: 'Pedro Gomez', type: 'car' },
  { plate: 'MN321OP', label: 'Moto Express', driverName: 'Sofia Torres', type: 'motorcycle' },
];

// Rutas circulares aproximadas alrededor de Buenos Aires para la demo.
const ROUTES = [
  { center: [-34.6037, -58.3816], radius: 0.02, speedBase: 35 },
  { center: [-34.62, -58.44], radius: 0.03, speedBase: 60 },
  { center: [-34.58, -58.40], radius: 0.015, speedBase: 20 },
  { center: [-34.60, -58.37], radius: 0.01, speedBase: 45 },
];

function ensureVehicles() {
  const existing = db.prepare('SELECT * FROM vehicles').all();
  if (existing.length > 0) return existing;

  const crypto = require('crypto');
  const insert = db.prepare(
    'INSERT INTO vehicles (plate, label, driver_name, type, api_key) VALUES (?, ?, ?, ?, ?)'
  );
  for (const v of DEMO_VEHICLES) {
    insert.run(v.plate, v.label, v.driverName, v.type, crypto.randomBytes(16).toString('hex'));
  }
  return db.prepare('SELECT * FROM vehicles').all();
}

function ensureDemoGeofence() {
  const count = db.prepare('SELECT COUNT(*) AS c FROM geofences').get().c;
  if (count > 0) return;
  db.prepare('INSERT INTO geofences (name, lat, lng, radius_m) VALUES (?, ?, ?, ?)').run(
    'Deposito Central',
    -34.6037,
    -58.3816,
    1500
  );
}

async function sendPing(apiKey, lat, lng, speed, heading) {
  try {
    await fetch(`${BASE_URL}/api/gps/ping`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ apiKey, lat, lng, speed, heading }),
    });
  } catch (err) {
    console.error('No se pudo enviar ping (¿esta corriendo el servidor?):', err.message);
  }
}

function main() {
  const vehicles = ensureVehicles();
  ensureDemoGeofence();

  const angleByVehicle = vehicles.map((_, i) => (i * Math.PI) / 2);

  console.log(`Simulando ${vehicles.length} vehiculos contra ${BASE_URL} cada ${TICK_MS}ms`);

  setInterval(() => {
    vehicles.forEach((vehicle, i) => {
      const route = ROUTES[i % ROUTES.length];
      angleByVehicle[i] = (angleByVehicle[i] + 0.08 + Math.random() * 0.04) % (2 * Math.PI);
      const lat = route.center[0] + route.radius * Math.sin(angleByVehicle[i]);
      const lng = route.center[1] + route.radius * Math.cos(angleByVehicle[i]);
      const heading = ((angleByVehicle[i] * 180) / Math.PI + 360) % 360;
      const speed = Math.max(0, route.speedBase + (Math.random() * 20 - 10));
      sendPing(vehicle.api_key, lat, lng, speed, heading);
    });
  }, TICK_MS);
}

main();
