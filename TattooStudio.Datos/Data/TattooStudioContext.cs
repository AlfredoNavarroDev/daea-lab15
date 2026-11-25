using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TattooStudio.Entidades;

namespace TattooStudio.Datos.Data;

public partial class TattooStudioContext : DbContext
{
    public TattooStudioContext(DbContextOptions<TattooStudioContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CitaTatuaje> CitaTatuajes { get; set; }

    public virtual DbSet<Tatuador> Tatuadors { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CitaTatuaje>(entity =>
        {
            entity.ToTable("CitaTatuaje");

            entity.HasIndex(e => e.TatuadorId, "IX_CitaTatuaje_TatuadorId");

            entity.Property(e => e.ClienteCorreo).HasMaxLength(150);
            entity.Property(e => e.ClienteNombre).HasMaxLength(100);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente", "DF_CitaTatuaje_Estado");
            entity.Property(e => e.EstimacionHoras).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.SeñaPagada).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Tatuador).WithMany(p => p.CitaTatuajes)
                .HasForeignKey(d => d.TatuadorId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CitaTatuaje_Tatuador");
        });

        modelBuilder.Entity<Tatuador>(entity =>
        {
            entity.ToTable("Tatuador");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Tatuador_Activo");
            entity.Property(e => e.Especialidad).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Correo, "UQ_Usuario_Correo").IsUnique();

            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.ClaveHash).HasMaxLength(200);
            entity.Property(e => e.Rol).HasMaxLength(30);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
