# Farmacia Salud

Sitio web para una farmacia construido con **ASP.NET Core 8 MVC** (C#), **Entity Framework Core** y **SQLite**. Incluye tienda con catálogo de productos, carrito de compras, checkout, formulario de contacto y un **panel de administración autoadministrable** para gestionar productos, categorías, pedidos y mensajes sin tocar código.

## Stack técnico

- **ASP.NET Core 8 MVC** (C#) — backend robusto y de código abierto, sin PHP.
- **Entity Framework Core** + **SQLite** — persistencia de datos, migraciones versionadas.
- **ASP.NET Core Identity** — autenticación y roles para el panel admin.
- **Bootstrap 5** + **Bootstrap Icons** — interfaz responsive.

## Funcionalidades

**Sitio público**
- Catálogo de productos por categoría, con búsqueda y paginación.
- Ficha de producto con detalle, stock y aviso de receta médica.
- Carrito de compras (basado en sesión).
- Checkout con registro de pedido (pago contra entrega).
- Formulario de contacto.

**Panel de administración (`/Admin`, requiere login)**
- Dashboard con métricas (productos, pedidos, ingresos, mensajes sin leer).
- CRUD de productos y categorías.
- Gestión de pedidos con cambio de estado (Pendiente, Confirmado, Entregado, Cancelado).
- Bandeja de mensajes de contacto.
- Cambio de contraseña del propio usuario admin.

## Cómo ejecutar el proyecto

Requiere el [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
cd src/Farmacia.Web
dotnet restore
dotnet run
```

Al iniciar, la aplicación crea automáticamente la base de datos SQLite (`App_Data/farmacia.db`), aplica las migraciones y siembra categorías, productos de ejemplo y un usuario administrador.

La app quedará disponible en la URL que indique la consola (por defecto `http://localhost:5000` o similar).

### Acceso al panel de administración

- URL: `/Account/Login`
- Correo: `admin@farmaciasalud.com`
- Contraseña: `Farmacia#2024!`

**Importante:** cambia esta contraseña antes de exponer el sitio en producción. Inicia sesión y ve a **Panel → Cambiar contraseña** (`/Admin/Account/ChangePassword`) en la barra lateral.

## Despliegue en Railway

El repo incluye un `Dockerfile` listo para Railway:

1. **New Project → Deploy from GitHub repo**, selecciona este repositorio y la rama deseada. Railway detecta el `Dockerfile` automáticamente.
2. **Agrega un volumen persistente** montado en `/app/App_Data` (Settings → Volumes) para que la base SQLite y las claves de sesión sobrevivan a los redeploys.
3. **Genera un dominio** en Settings → Networking → Generate Domain.
4. Cambia la contraseña del admin apenas entres por primera vez (ver sección anterior).

No se requieren variables de entorno adicionales: `ASPNETCORE_ENVIRONMENT=Production` viene fijo en el `Dockerfile` y Railway inyecta `PORT` automáticamente.

## Estructura del proyecto

```
src/Farmacia.Web/
├── Controllers/        # Home, Products, Cart, Checkout, Contact, Account
├── Areas/Admin/         # Panel administrativo (productos, categorías, pedidos, mensajes)
├── Models/              # Entidades y ViewModels
├── Data/                # DbContext, migraciones y datos semilla
├── Services/            # CartService (carrito basado en sesión)
├── Views/                # Vistas Razor
└── wwwroot/              # CSS, JS y librerías estáticas
```
