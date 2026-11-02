using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using SistemaOnline.Data;
using SistemaOnline.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración propia de cada PC (opcional y fuera de Git): appsettings.Local.json, junto a
// appsettings.json. Sirve para poner el servidor o la contraseña de la base de datos de esa
// máquina sin tocar los archivos compartidos. Se inserta antes de las variables de entorno para
// que estas sigan teniendo prioridad.
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

// Plataformas como Railway indican el puerto en la variable PORT.
var puerto = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(puerto))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{puerto}");
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

builder.Services.AddSession();
var app = builder.Build();

// Crear base de datos
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