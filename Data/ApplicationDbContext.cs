using Microsoft.EntityFrameworkCore;
using ProyectoLuisa.Models;

namespace ProyectoLuisa.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

public DbSet<Evento> Eventos { get; set; }
public DbSet<Horario> Horarios { get; set; }
public DbSet<Archivo> Archivos { get; set; }

public DbSet<Noticia> Noticias { get; set; }


public DbSet<InformacionInstitucional> InformacionInstitucional { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Correo).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Rol).IsRequired().HasMaxLength(20);
                entity.Property(u => u.Activo).HasDefaultValue(false);
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Correo).IsRequired();
                entity.Property(t => t.Token).IsRequired();
                entity.Property(t => t.Expira).IsRequired();
            });
        }
    }
}
