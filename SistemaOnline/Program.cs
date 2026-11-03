using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using SistemaOnline.Data;
using SistemaOnline.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración propia de cada PC 
var fuentes = ((IConfigurationBuilder)builder.Configuration).Sources;
int posEntorno = fuentes.ToList().FindLastIndex(s =>
    s is EnvironmentVariablesConfigurationSource e && string.IsNullOrEmpty(e.Prefix));
fuentes.Insert(posEntorno < 0 ? fuentes.Count : posEntorno, new JsonConfigurationSource
{
    FileProvider = builder.Environment.ContentRootFileProvider,
    Path = "appsettings.Local.json",
    Optional = true,
    ReloadOnChange = true
});

var puerto = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(puerto))
{
    if (!int.TryParse(puerto, out int numeroPuerto) || numeroPuerto is < 1 or > 65535)
    {
        throw new InvalidOperationException($"La variable PORT='{puerto}' no es un puerto válido.");
    }
    builder.WebHost.ConfigureKestrel(opciones => opciones.ListenAnyIP(numeroPuerto));
}

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        value => $"El valor '{value}' no es válido.");
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        value => "Este campo es obligatorio.");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
        value => $"Se requiere un valor para '{value}'.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (value, field) => $"El valor '{value}' no es válido para '{field}'.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => "Este campo es obligatorio.");
    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(
        value => $"El valor '{value}' no es válido.");
    options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(
        () => "El valor enviado no es válido.");
    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(
        field => $"El valor enviado para '{field}' no es válido.");
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.AccessDeniedPath = "/Login/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromSeconds(5000);
    });

// Motor de base de datos: "MySql" o "SqlServer" 
var proveedorBD = builder.Configuration["DatabaseProvider"] ?? "MySql";
bool usarSqlServer = proveedorBD.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);
if (!usarSqlServer && !proveedorBD.Equals("MySql", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException($"DatabaseProvider='{proveedorBD}' no es válido: usa \"MySql\" o \"SqlServer\".");
}
string nombreCadena = usarSqlServer ? "SQLCadena" : "MySQLCadena";
string cadenaConexion = builder.Configuration.GetConnectionString(nombreCadena)
    ?? throw new InvalidOperationException($"Falta la cadena de conexión ConnectionStrings:{nombreCadena}.");
builder.Services.AddDbContext<APPDBContext>(options =>
{
    if (usarSqlServer)
        options.UseSqlServer(cadenaConexion);
    else
        options.UseMySQL(cadenaConexion);
});

// Envío de correo real por SMTP (ver appsettings.Development.json, sección "Smtp").
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// Logo personalizable (archivo en App_Data/branding o en Branding:Carpeta).
builder.Services.AddSingleton<LogoService>();

builder.Services.AddSession();
var app = builder.Build();

// Crear la base de datos 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<APPDBContext>();
    await db.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

var spanishCulture = new CultureInfo("es-PE");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(spanishCulture, spanishCulture),
    SupportedCultures = new List<CultureInfo> { spanishCulture },
    SupportedUICultures = new List<CultureInfo> { spanishCulture }
};
app.UseRequestLocalization(localizationOptions);

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}")
    .WithStaticAssets();


await app.RunAsync();