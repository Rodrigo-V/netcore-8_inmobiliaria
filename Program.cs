using Microsoft.AspNetCore.Authentication.Cookies;
using Inmobiliaria.Net8.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

// Registrar IHttpContextAccessor para acceder al contexto HTTP en las vistas
builder.Services.AddHttpContextAccessor();

// Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Autenticacion/Login";
        options.LogoutPath = "/Autenticacion/Logout";
        options.AccessDeniedPath = "/Autenticacion/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = ".INMOBILIARIA_AUTH";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// Registrar servicios personalizados
builder.Services.AddScoped<AutenticacionService>();
builder.Services.AddScoped<ClientesLeadsService>();
builder.Services.AddScoped<ClientesMatchService>();
builder.Services.AddScoped<SeguimientoActivoService>();
builder.Services.AddScoped<GestionUsuariosService>();
builder.Services.AddScoped<ConfiguracionImagenesService>();
builder.Services.AddScoped<SincronizacionPortalesService>();
builder.Services.AddScoped<PropiedadesService>();
builder.Services.AddScoped<APIConfiguracionService>();
builder.Services.AddScoped<ConfiguracionPortalesService>();
builder.Services.AddScoped<CargadorExcelPropitService>();

// Registrar servicio de Mercado Libre con HttpClient
builder.Services.AddHttpClient<MercadoLibreService>();

// Configurar sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".INMOBILIARIA_SESSION";
});

var app = builder.Build();

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

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Autenticacion}/{action=Login}/{id?}");

app.Run();
