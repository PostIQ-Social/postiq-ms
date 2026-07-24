using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using User.Core.Entities;

namespace User.Core.Persistence;

public partial class UserDBContext : DbContext
{
    public UserDBContext()
    {
    }

    public UserDBContext(DbContextOptions<UserDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Published> Publisheds { get; set; }

    public virtual DbSet<UserDetail> UserDetails { get; set; }
    public virtual DbSet<UserReferral> UserReferrals { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Published>(entity =>
        {
            entity.ToTable("Published", "User");

            entity.Property(e => e.BaseUrl)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Source)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

        });

        modelBuilder.Entity<UserDetail>(entity =>
        {
            entity.ToTable("UserDetails", "User");

            entity.Property(e => e.UserId).ValueGeneratedOnAdd();
            entity.Property(e => e.AuthId);
            entity.Property(e => e.IsActive);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.ReferralCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy);
            entity.Property(e => e.UpdatedBy);

        });
        modelBuilder.Entity<UserReferral>(entity =>
        {
			entity.ToTable("UserReferral", "User");

            entity.Property(e => e.ReferralId).ValueGeneratedOnAdd();
            entity.Property(e => e.UserId);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.ReferralCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ReferredById);
            entity.Property(e => e.IsActive);
            entity.Property(e => e.CreatedBy);
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ReferredByName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
