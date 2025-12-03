using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;
using ProyectoLuisa.Services;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------
// 🔧 Configuración de servicios
// ------------------------------------------------------------------

// MVC
builder.Services.AddControllersWithViews();

// 🔹 Configuración de MySQL (Railway)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "archivos");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
// Servicio de correos
builder.Services.AddScoped<EmailService>();
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ------------------------------------------------------------------
// 🚀 Creación automática del administrador inicial
// ------------------------------------------------------------------
// 🔹 Crear admin por defecto si no existe
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    if (!context.Usuarios.Any(u => u.Rol == "Admin"))
    {
        var admin = new Usuario
        {
            Nombre = "Administrador del Sistema",
            Correo = "admin@luisa.edu",
            Rol = "Admin",
            Activo = true,
            ContrasenaHash = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes("Admin123"))
            )
        };

        context.Usuarios.Add(admin);
        context.SaveChanges();
        Console.WriteLine("✅ Admin creado: admin@luisa.edu / Contraseña: Admin123");
    }
}

// ------------------------------------------------------------------
// ⚙️ Configuración del pipeline HTTP
// ------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
//app.UseStaticFiles();
/*app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads/archivos"
}); */


app.UseStaticFiles();
// Servir /uploads/eventos
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads", "eventos")),
    RequestPath = "/uploads/eventos"
});

// Servir /uploads/archivos (esto ya lo tenías)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads", "archivos")),
    RequestPath = "/uploads/archivos"
});

 // importante para servir CSS, JS e imágenes
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// ------------------------------------------------------------------
// 🧭 Rutas
// ------------------------------------------------------------------

/* Por defecto, redirige al Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);
*/



app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Inicio}/{action=Index}/{id?}");


// 👇 Nuevo: soporte para áreas (no cambia nada de lo actual)
// ✅ Deja tu ruta por defecto como la tienes (Login)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);


app.Run();
