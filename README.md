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

**Importante:** cambia esta contraseña antes de desplegar en producción (puedes hacerlo desde el propio flujo de Identity o editando `Data/SeedData.cs` antes del primer arranque).

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
