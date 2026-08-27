# Fleet Tracker

Aplicación de control de flota con geolocalización en tiempo real y un endpoint listo para conectar GPS reales (trackers dedicados o el GPS del celular del conductor). Es un proyecto independiente (Node.js) dentro de este repositorio, sin relación con la tienda de la farmacia.

## Stack

- **Node.js + Express** — servidor HTTP y API REST.
- **Socket.IO** — posiciones en tiempo real hacia el dashboard (sin polling).
- **better-sqlite3** — persistencia simple de vehículos, ubicaciones, geocercas y alertas.
- **Leaflet + OpenStreetMap** — mapa en vivo, servido localmente (sin depender de un CDN).
- Frontend en HTML/CSS/JS vanilla, sin build step.

## Cómo correrlo

```bash
cd fleet-tracker
npm install
npm start
```

Abrí `http://localhost:4000`. Vas a ver el dashboard vacío hasta que haya vehículos con posición.

### Ver una demo en vivo (sin hardware GPS)

En otra terminal:

```bash
npm run simulate
```

Esto crea 4 vehículos de ejemplo y una geocerca ("Depósito Central"), y les envía posiciones falsas cada 2 segundos **usando el mismo endpoint HTTP que usaría un tracker GPS real**. Sirve para ver toda la app funcionando (mapa, alertas, historial) sin conectar nada físico. Cuando conectes GPS reales, simplemente no corras este script.

## Cómo conectar un GPS real

Cada vehículo tiene una `api_key` propia (se genera al crearlo desde el dashboard y se muestra una sola vez). Cualquier dispositivo o app que pueda hacer un `POST` HTTP puede reportar su posición:

```
POST /api/gps/ping
Content-Type: application/json

{
  "apiKey": "<api key del vehiculo>",
  "lat": -34.603722,
  "lng": -58.381592,
  "speed": 42,
  "heading": 180
}
```

Dos caminos típicos para producir ese POST:

1. **App/PWA en el celular del conductor**: la forma más rápida y barata — usa `navigator.geolocation.watchPosition` y hace `fetch` a este endpoint cada N segundos. No requiere hardware adicional.
2. **Tracker GPS dedicado** (OBD-II, hardwired): la mayoría de los trackers comerciales no hablan HTTP JSON directamente, sino protocolos propietarios. La forma estándar de resolverlo es poner **[Traccar](https://www.traccar.org/)** (open source, soporta cientos de modelos) como puente: Traccar recibe el protocolo nativo del tracker y desde ahí reenviás a este endpoint (webhook/forwarding) o consultás su API.

Un vehículo pasa a `offline` automáticamente si no manda un ping en 2 minutos.

## Funcionalidades

- **Mapa en vivo** con el estado de cada vehículo (activo / detenido / sin señal).
- **Alta/baja de vehículos** desde el dashboard, con generación de API key.
- **Geocercas**: se dibujan haciendo click en el mapa; el servidor detecta automáticamente cuándo un vehículo entra o sale y genera una alerta.
- **Alertas** de geocerca y de exceso de velocidad (>100 km/h, configurable en `src/geofenceChecker.js`), en vivo y persistidas.
- **Historial de ruta**: elegís un vehículo, se dibuja el recorrido completo y podés reproducirlo punto a punto con una línea de tiempo.

## Estructura

```
fleet-tracker/
├── server.js              # Express + Socket.IO + limpieza de vehículos offline
├── simulator.js            # Generador de posiciones falsas para demo
├── routes/api.js           # REST: vehículos, ingestión GPS, geocercas, alertas, historial
├── src/
│   ├── db.js                # Esquema SQLite
│   ├── geo.js                # Distancia haversine
│   └── geofenceChecker.js    # Detección de entrada/salida de geocercas + velocidad
└── public/                 # Dashboard (HTML/CSS/JS + Leaflet servido localmente)
```

## Próximos pasos sugeridos

- Autenticación del dashboard (hoy es de acceso libre; agregar login antes de exponerlo).
- Optimización de rutas multi-parada para asignar el vehículo más cercano a un pedido.
- Exportar historial a CSV/Excel para reportes.
- Migrar a Postgres/TimescaleDB si el volumen de posiciones crece mucho (SQLite no es ideal para escrituras muy frecuentes a gran escala).
