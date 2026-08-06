using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using OpticaClaridad.Data;
using OpticaClaridad.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000", "http://localhost:5000");

// Tus servicios existentes
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery();

// =============================================
// AGREGAR AUTENTICACIÓN
// =============================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "CookieAuthentication";
})
.AddCookie("CookieAuthentication", options =>
{
    options.LoginPath = "/Home/Index";
    options.AccessDeniedPath = "/Home/Index";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// Configuraciones
builder.Services.Configure<AccesoConfig>(
    builder.Configuration.GetSection("AccesoConfig")
);

builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection("FileStorage")
);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();

// Servir archivos desde wwwroot
app.UseStaticFiles();

// Servir archivos desde C:\ArchivosOptica (o la ruta configurada)
var rootPath = builder.Configuration.GetValue<string>("FileStorage:RootPath") ?? "C:\\ArchivosOptica";
if (Directory.Exists(rootPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(rootPath),
        RequestPath = "/archivos"
    });
}

app.UseRouting();
app.UseSession();


app.UseAuthentication();  // <-- ANTES de UseAuthorization
app.UseAuthorization();   // <-- DESPUÉS de UseAuthentication

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();