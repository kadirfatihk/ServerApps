using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServerApps.Persistence.Models;

namespace ServerApps.Persistence.Modal
{
    public partial class DvuApplicationAuthenticationDbContext : DbContext
    {
        public DvuApplicationAuthenticationDbContext()
        {
        }

        public DvuApplicationAuthenticationDbContext(DbContextOptions<DvuApplicationAuthenticationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. 
            => optionsBuilder.UseSqlServer("Server=10.41.150.78; Database=DvuApplicationAuthenticationDb; User Id=sa; Password=112233on!; TrustServerCertificate=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0790470E01");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");
                entity.Property(e => e.EmailAddress).HasMaxLength(100);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.JobTitle).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                // User ile PasswordResetToken ilişkisi (1-n)
                entity.HasMany(e => e.PasswordResetTokens)
                      .WithOne(prt => prt.User)
                      .HasForeignKey(prt => prt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PasswordResetTokens");

                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
                entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
                entity.Property(e => e.IsUsed).HasDefaultValue(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
