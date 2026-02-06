using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VehicleRentalSystem.Models;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TbAccessory> TbAccessories { get; set; }

    public virtual DbSet<TbPayment> TbPayments { get; set; }

    public virtual DbSet<TbRating> TbRatings { get; set; }

    public virtual DbSet<TbRental> TbRentals { get; set; }

    public virtual DbSet<TbRentalAccessory> TbRentalAccessories { get; set; }

    public virtual DbSet<TbUser> TbUsers { get; set; }

    public virtual DbSet<TbVehicle> TbVehicles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost:5432;Database=postgres;Username=postgres;Password=root");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgagent", "pgagent");

        modelBuilder.Entity<TbAccessory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_accessory_pkey");

            entity.ToTable("tb_accessory", "sistema_locacao");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.DailyRate)
                .HasPrecision(10, 2)
                .HasColumnName("daily_rate");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TbPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_payment_pkey");

            entity.ToTable("tb_payment", "sistema_locacao");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.RentalId).HasColumnName("rental_id");

            entity.HasOne(d => d.Rental).WithMany(p => p.TbPayments)
                .HasForeignKey(d => d.RentalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_rental");
        });

        modelBuilder.Entity<TbRating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_rating_pkey");

            entity.ToTable("tb_rating", "sistema_locacao");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.RentalId).HasColumnName("rental_id");

            entity.HasOne(d => d.Rental).WithMany(p => p.TbRatings)
                .HasForeignKey(d => d.RentalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rating_rental");
        });

        modelBuilder.Entity<TbRental>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_rental_pkey");

            entity.ToTable("tb_rental", "sistema_locacao");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ActualEndDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("actual_end_date");
            entity.Property(e => e.DailyRate)
                .HasPrecision(10, 2)
                .HasColumnName("daily_rate");
            entity.Property(e => e.ExpectedEndDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expected_end_date");
            entity.Property(e => e.PenaltyFee)
                .HasPrecision(10, 2)
                .HasColumnName("penalty_fee");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.User).WithMany(p => p.TbRentals)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rental_user");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TbRentals)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rental_vehicle");
        });

        modelBuilder.Entity<TbRentalAccessory>(entity =>
        {
            entity.HasKey(e => new { e.RentalId, e.AccessoryId }).HasName("tb_rental_accessory_pkey");

            entity.ToTable("tb_rental_accessory", "sistema_locacao");

            entity.Property(e => e.RentalId).HasColumnName("rental_id");
            entity.Property(e => e.AccessoryId).HasColumnName("accessory_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(10, 2)
                .HasColumnName("total_price");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(10, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Accessory).WithMany(p => p.TbRentalAccessories)
                .HasForeignKey(d => d.AccessoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ra_accessory");

            entity.HasOne(d => d.Rental).WithMany(p => p.TbRentalAccessories)
                .HasForeignKey(d => d.RentalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ra_rental");
        });

        modelBuilder.Entity<TbUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_user_pkey");

            entity.ToTable("tb_user", "sistema_locacao");

            entity.HasIndex(e => e.Email, "tb_user_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Active)
                .HasDefaultValue(true)
                .HasColumnName("active");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TbVehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_vehicles_pkey");

            entity.ToTable("tb_vehicles", "sistema_locacao");

            entity.HasIndex(e => e.LicensePlate, "tb_vehicles_license_plate_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Brand)
                .HasMaxLength(100)
                .HasColumnName("brand");
            entity.Property(e => e.DailyRate)
                .HasPrecision(10, 2)
                .HasColumnName("daily_rate");
            entity.Property(e => e.Int).HasColumnName("int");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(20)
                .HasColumnName("license_plate");
            entity.Property(e => e.Model)
                .HasMaxLength(100)
                .HasColumnName("model");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
