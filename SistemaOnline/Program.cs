using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDbContext<APPDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLCadena")));

builder.Services.AddSession();
var app = builder.Build();

// Migrar base de datos al iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<APPDBContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

var spanishCulture = new CultureInfo("es-ES");
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


app.Run();