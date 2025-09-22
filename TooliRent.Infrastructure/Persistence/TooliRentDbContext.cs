using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;

namespace TooliRent.Infrastructure.Persistence
{
    public class TooliRentDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public TooliRentDbContext(DbContextOptions<TooliRentDbContext> options) : base(options)
        {
        }

        public DbSet<Tool> Tools => Set<Tool>();
        public DbSet<ToolCategory> ToolCategories => Set<ToolCategory>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingItem> BookingItems => Set<BookingItem>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken>(et =>
            {
                et.HasKey(r => r.Id);
                et.HasIndex(r => new { r.UserId, r.ExpiresAt });
                et.Property(r => r.TokenHash).IsRequired();
                et.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Tool>(entity =>
            {
                entity.Property(t => t.PricePerDay).HasPrecision(18, 2);
                entity.Property(t => t.QuantityAvailable).IsConcurrencyToken();
                entity.HasOne(t => t.Category)
                    .WithMany(c => c.Tools)
                    .HasForeignKey(t => t.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.Property(t => t.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasOne(b => b.User)
                    .WithMany()
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(b => b.Items)
                    .WithOne(i => i.Booking)
                    .HasForeignKey(i => i.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(b => b.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<BookingItem>(entity =>
            {
                entity.HasIndex(bi => new { bi.BookingId, bi.ToolId }).IsUnique();

                entity.HasOne(bi => bi.Tool)
                    .WithMany(t => t.BookingItems)
                    .HasForeignKey(bi => bi.ToolId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ToolCategory>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
            });

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var adminRoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var memberRoleId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

            modelBuilder.Entity<IdentityRole<Guid>>().HasData(
                new IdentityRole<Guid>
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "ROLE-CONC-ADMIN"
                },
                new IdentityRole<Guid>
                {
                    Id = memberRoleId,
                    Name = "Member",
                    NormalizedName = "MEMBER",
                    ConcurrencyStamp = "ROLE-CONC-MEMBER"
                });

            modelBuilder.Entity<ToolCategory>().HasData(
                new ToolCategory { Id = 1, Name = "Borrmaskiner" },
                new ToolCategory { Id = 2, Name = "Skruvdragare" },
                new ToolCategory { Id = 3, Name = "Handverktyg" }
            );

            modelBuilder.Entity<Tool>().HasData(
                new Tool
                {
                    Id = 1,
                    Name = "Bosch Borrmaskin",
                    Description = "Borrmaskin med kabel",
                    PricePerDay = 50,
                    QuantityAvailable = 3,
                    CategoryId = 1,
                    Status = ToolStatus.Available
                },
                new Tool
                {
                    Id = 2,
                    Name = "Milwaukee Borrmaskin",
                    Description = "Borrmaskin med batteri",
                    PricePerDay = 70,
                    QuantityAvailable = 2,
                    CategoryId = 1,
                    Status = ToolStatus.Available
                },
                new Tool
                {
                    Id = 3,
                    Name = "Makita Skruvdragare",
                    Description = "Kompakt skruvdragare",
                    PricePerDay = 60,
                    QuantityAvailable = 4,
                    CategoryId = 2,
                    Status = ToolStatus.Available
                },
                new Tool
                {
                    Id = 4,
                    Name = "DeWalt Skruvdragare",
                    Description = "Kraftfull skruvdragare",
                    PricePerDay = 80,
                    QuantityAvailable = 1,
                    CategoryId = 2,
                    Status = ToolStatus.Available
                },
                new Tool
                {
                    Id = 5,
                    Name = "Hultafors Fogsvans",
                    Description = "Kvalitetssåg från Hultafors",
                    PricePerDay = 5,
                    QuantityAvailable = 4,
                    CategoryId = 3,
                    Status = ToolStatus.Available
                }
            );

            // Users
            var aliceId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var adminId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            // Passwords are hashed versions of "Member123" and "Admin123"
            const string AdminPasswordHash = "AQAAAAIAAYagAAAAEDJmR1AJzv/AZWcfvpYJq+7Yh28SL9TAoa3SJyjPQiP9qj9KSML4lPMo4KJM5FVbAQ==";
            const string AlicePasswordHash = "AQAAAAIAAYagAAAAEE88eyDi+I7LBIsVSeKbK/a/N1OWOlWN30bu1y8YHvQwKA3cAhFR0bxRZi25aBFrbQ==";

            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = aliceId,
                    UserName = "alice@TooliRent.com",
                    NormalizedUserName = "ALICE@TOOLIRENT.COM",
                    Email = "alice@TooliRent.com",
                    NormalizedEmail = "ALICE@TOOLIRENT.COM",
                    FirstName = "Alice",
                    LastName = "Andersson",
                    DisplayName = "Alice Andersson",
                    EmailConfirmed = true,
                    PasswordHash = AlicePasswordHash,
                    SecurityStamp = "SEC-STAMP-ALICE",
                    ConcurrencyStamp = "CONC-STAMP-ALICE",
                    IsActive = true
                },
                new ApplicationUser
                {
                    Id = adminId,
                    UserName = "admin@TooliRent.com",
                    NormalizedUserName = "ADMIN@TOOLIRENT.COM",
                    Email = "admin@TooliRent.com",
                    NormalizedEmail = "ADMIN@TOOLIRENT.COM",
                    FirstName = "Admin",
                    LastName = "User",
                    DisplayName = "Admin User",
                    EmailConfirmed = true,
                    PasswordHash = AdminPasswordHash,
                    SecurityStamp = "SEC-STAMP-ADMIN",
                    ConcurrencyStamp = "CONC-STAMP-ADMIN",
                    IsActive = true
                }
            );

            modelBuilder.Entity<IdentityUserRole<Guid>>().HasData(
                new IdentityUserRole<Guid> { UserId = aliceId, RoleId = memberRoleId },
                new IdentityUserRole<Guid> { UserId = adminId, RoleId = adminRoleId }
            );
        }
    }
}
