using Microsoft.EntityFrameworkCore;
using PostIQ.Identity.Models;
using System.Reflection.Emit;

namespace PostIQ.Identity.Data
{
    public partial class IdentityDbContext : DbContext
    {
        public IdentityDbContext()
        {
        }

        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

        public virtual DbSet<SecurityToken> SecurityTokens { get; set; }

        public virtual DbSet<User> Users { get; set; }

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//            => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=postIQ;Trusted_Connection=True;Integrated Security=True;TrustServerCertificate=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07309D6554");

                entity.ToTable("RefreshTokens", "Auth");

                entity.HasIndex(e => e.TokenHash, "IX_RefreshTokens_TokenHash");

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.ReplacedByTokenHash).HasMaxLength(512);
                entity.Property(e => e.TokenHash).HasMaxLength(512);

                
            });

            modelBuilder.Entity<SecurityToken>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Security__3214EC07F02BD82B");

                entity.ToTable("SecurityTokens", "Auth");

                entity.HasIndex(e => new { e.UserId, e.Kind }, "IX_SecurityTokens_UserId_Kind");

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.TokenHash).HasMaxLength(512);

              
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07BF2957D7");

                entity.ToTable("User", "Auth");

                entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Email).HasMaxLength(320);
                entity.Property(e => e.PasswordHash).HasMaxLength(512);
                entity.Property(e => e.PhoneNumber).HasMaxLength(32);
                entity.Property(e => e.Roles).HasMaxLength(256);
                entity.Property(e => e.TwoFactorSecret).HasMaxLength(512);
                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
