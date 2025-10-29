using Microsoft.EntityFrameworkCore;
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


// Servicio de correos
builder.Services.AddScoped<EmailService>();

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
app.UseStaticFiles(); // importante para servir CSS, JS e imágenes
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// ------------------------------------------------------------------
// 🧭 Rutas
// ------------------------------------------------------------------

// Por defecto, redirige al Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);

app.Run();
