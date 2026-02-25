using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrar HttpClient (necesario para que el servicio haga peticiones a la API)
//builder.Services.AddHttpClient();

// Registrar tu clase de servicio para que el Controlador pueda encontrarla
//builder.Services.AddScoped<ConsultorioVerde.Web.Services.ApiService>();

builder.Services.AddHttpClient<ApiServiceProxy>();

// 1. Agregar el servicio de Autenticación por Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Ruta a tu login si no está logueado
        options.AccessDeniedPath = "/Account/AccessDenied"; // Ruta si no tiene el Rol necesario
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Cuánto dura la sesión
    });

// Asegúrate de agregar controladores con vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ... otros middlewares (static files, etc)

app.UseRouting();

// ESTAS DOS LÍNEAS SON LAS QUE TE FALTAN:
app.UseAuthentication(); // ¿Quién eres?
app.UseAuthorization();  // ¿Qué tienes permiso de hacer?

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Cambiado de Home a Account

app.Run();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
