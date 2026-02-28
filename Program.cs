using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN DE SERVICIOS (DI) ---
builder.Services.AddControllersWithViews();

// Registrar HttpClient y Proxy
builder.Services.AddHttpClient<ApiServiceProxy>();

// Configurar Autenticación por Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true; // Renueva el tiempo si el usuario está activo
    });

var app = builder.Build();

// --- 2. CONFIGURACIÓN DEL PIPELINE (EL ORDEN ES VITAL) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// PRIMERO: Permitir archivos CSS, JS e imágenes (wwwroot) sin pedir login
app.UseStaticFiles();

// SEGUNDO: Habilitar el enrutamiento
app.UseRouting();

// TERCERO: Autenticación (¿Quién es?) y Autorización (¿A qué tiene permiso?)
app.UseAuthentication();
app.UseAuthorization();

// CUARTO: Configurar las rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ÚLTIMO: Arrancar la aplicación
app.Run();