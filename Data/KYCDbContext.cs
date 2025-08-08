using Microsoft.EntityFrameworkCore;
using KYCApp.Models;

namespace KYCApp.Data;

public partial class KYCDbContext : DbContext
{
    public KYCDbContext()
    {
    }

    public KYCDbContext(DbContextOptions<KYCDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CodigoQr> CodigoQrs { get; set; }

    public virtual DbSet<Visitante> Visitantes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CodigoQr>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CodigoQR__3214EC0799C3E79B");

            entity.ToTable("CodigoQR");

            entity.Property(e => e.Id).HasColumnType("uniqueidentifier");
            entity.Property(e => e.Codigo).HasMaxLength(50);

            entity.HasOne(d => d.Visitante).WithMany(p => p.CodigoQrs)
                .HasForeignKey(d => d.VisitanteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CodigoQR__Visita__3B75D760");
        });

        modelBuilder.Entity<Visitante>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Visitan__3214EC07B8E7F5EE");

            entity.ToTable("Visitantes");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ApellidoMaterno).HasMaxLength(100);
            entity.Property(e => e.ApellidoPaterno).HasMaxLength(100);
            entity.Property(e => e.CorreoElectronico).HasMaxLength(150);
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.FechaVisita).HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
