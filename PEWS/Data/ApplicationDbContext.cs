using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PEWS.Models;
using PEWS.Models.Enums;

namespace PEWS.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Region> Regions => Set<Region>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<DisasterType> DisasterTypes => Set<DisasterType>();
    public DbSet<DisasterReport> DisasterReports => Set<DisasterReport>();
    public DbSet<ReportImage> ReportImages => Set<ReportImage>();
    public DbSet<UserReport> UserReports => Set<UserReport>();
    public DbSet<UserReportImage> UserReportImages => Set<UserReportImage>();
    public DbSet<AlertCategory> AlertCategories => Set<AlertCategory>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<WeatherData> WeatherData => Set<WeatherData>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity tables for cleaner schema
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

        // DisasterReport — dual FK to ApplicationUser (report + verification)
        builder.Entity<DisasterReport>()
            .HasOne(r => r.ReportedBy)
            .WithMany(u => u.DisasterReports)
            .HasForeignKey(r => r.ReportedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DisasterReport>()
            .HasOne(r => r.VerifiedBy)
            .WithMany()
            .HasForeignKey(r => r.VerifiedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DisasterReport>()
            .HasOne(r => r.Region)
            .WithMany(r => r.DisasterReports)
            .HasForeignKey(r => r.RegionId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<DisasterReport>()
            .HasOne(r => r.District)
            .WithMany(d => d.DisasterReports)
            .HasForeignKey(r => r.DistrictId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // UserReport — citizen submissions, independent of DisasterReport (see UserReport.cs)
        builder.Entity<UserReport>()
            .HasOne(r => r.ReportedBy)
            .WithMany(u => u.UserReports)
            .HasForeignKey(r => r.ReportedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserReport>()
            .HasOne(r => r.VerifiedBy)
            .WithMany()
            .HasForeignKey(r => r.VerifiedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserReport>()
            .HasOne(r => r.Region)
            .WithMany(r => r.UserReports)
            .HasForeignKey(r => r.RegionId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<UserReport>()
            .HasOne(r => r.District)
            .WithMany(d => d.UserReports)
            .HasForeignKey(r => r.DistrictId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // Alert FK
        builder.Entity<Alert>()
            .HasOne(a => a.CreatedBy)
            .WithMany(u => u.CreatedAlerts)
            .HasForeignKey(a => a.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Alert>()
            .HasOne(a => a.Region)
            .WithMany(r => r.Alerts)
            .HasForeignKey(a => a.RegionId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<Alert>()
            .HasOne(a => a.District)
            .WithMany(d => d.Alerts)
            .HasForeignKey(a => a.DistrictId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // Indexes for performance
        builder.Entity<DisasterReport>()
            .HasIndex(r => r.Status);
        builder.Entity<DisasterReport>()
            .HasIndex(r => r.Severity);
        builder.Entity<UserReport>()
            .HasIndex(r => r.Status);
        builder.Entity<UserReport>()
            .HasIndex(r => r.Severity);
        builder.Entity<DisasterReport>()
            .HasIndex(r => r.CreatedAt);

        builder.Entity<Alert>()
            .HasIndex(a => a.Status);
        builder.Entity<Alert>()
            .HasIndex(a => a.IsPublished);

        builder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead });

        builder.Entity<AuditLog>()
            .HasIndex(a => a.CreatedAt);

        // Seed data
        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        // Seed Roles
        var superAdminRoleId = "1a2b3c4d-0001-0000-0000-000000000001";
        var adminRoleId = "1a2b3c4d-0002-0000-0000-000000000002";
        var officerRoleId = "1a2b3c4d-0003-0000-0000-000000000003";
        var citizenRoleId = "1a2b3c4d-0004-0000-0000-000000000004";

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = superAdminRoleId, Name = "SuperAdmin", NormalizedName = "SUPERADMIN", ConcurrencyStamp = superAdminRoleId },
            new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = adminRoleId },
            new IdentityRole { Id = officerRoleId, Name = "EmergencyOfficer", NormalizedName = "EMERGENCYOFFICER", ConcurrencyStamp = officerRoleId },
            new IdentityRole { Id = citizenRoleId, Name = "Citizen", NormalizedName = "CITIZEN", ConcurrencyStamp = citizenRoleId }
        );

        // Seed Disaster Types
        builder.Entity<DisasterType>().HasData(
            new DisasterType { Id = 1, Name = "Flood", Description = "River or flash flooding events", IconClass = "fas fa-water", Color = "#0d6efd", SortOrder = 1, IsActive = true },
            new DisasterType { Id = 2, Name = "Drought", Description = "Prolonged water scarcity events", IconClass = "fas fa-sun", Color = "#ffc107", SortOrder = 2, IsActive = true },
            new DisasterType { Id = 3, Name = "Storm", Description = "Severe storm events", IconClass = "fas fa-cloud-showers-heavy", Color = "#6f42c1", SortOrder = 3, IsActive = true },
            new DisasterType { Id = 4, Name = "Cyclone", Description = "Tropical cyclone events", IconClass = "fas fa-tornado", Color = "#dc3545", SortOrder = 4, IsActive = true },
            new DisasterType { Id = 5, Name = "Fire", Description = "Wildfire or structural fire events", IconClass = "fas fa-fire", Color = "#fd7e14", SortOrder = 5, IsActive = true },
            new DisasterType { Id = 6, Name = "Earthquake", Description = "Seismic activity events", IconClass = "fas fa-house-damage", Color = "#6c757d", SortOrder = 6, IsActive = true },
            new DisasterType { Id = 7, Name = "Disease Outbreak", Description = "Epidemic or pandemic events", IconClass = "fas fa-virus", Color = "#20c997", SortOrder = 7, IsActive = true },
            new DisasterType { Id = 8, Name = "Landslide", Description = "Slope failure and debris events", IconClass = "fas fa-mountain", Color = "#795548", SortOrder = 8, IsActive = true }
        );

        // Seed Alert Categories
        builder.Entity<AlertCategory>().HasData(
            new AlertCategory { Id = 1, Name = "Weather Alert", Description = "Severe weather warnings", Color = "#0d6efd", IconClass = "fas fa-cloud-bolt", IsActive = true },
            new AlertCategory { Id = 2, Name = "Evacuation Notice", Description = "Mandatory or voluntary evacuation orders", Color = "#dc3545", IconClass = "fas fa-person-running", IsActive = true },
            new AlertCategory { Id = 3, Name = "Health Advisory", Description = "Public health warnings", Color = "#20c997", IconClass = "fas fa-heart-pulse", IsActive = true },
            new AlertCategory { Id = 4, Name = "Infrastructure Alert", Description = "Road, bridge, utility alerts", Color = "#ffc107", IconClass = "fas fa-road-barrier", IsActive = true },
            new AlertCategory { Id = 5, Name = "Security Alert", Description = "Public safety and security notices", Color = "#6f42c1", IconClass = "fas fa-shield-halved", IsActive = true }
        );

        // Seed Regions of Puntland
        builder.Entity<Region>().HasData(
            new Region { Id = 1, Name = "Nugaal", Code = "NUG", Description = "Nugaal region — capital Garowe", RiskLevel = RiskLevel.Medium, Latitude = 8.4054, Longitude = 49.4879, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Region { Id = 2, Name = "Bari", Code = "BAR", Description = "Bari region — capital Bosaso", RiskLevel = RiskLevel.Low, Latitude = 11.2784, Longitude = 49.1826, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Region { Id = 3, Name = "Mudug", Code = "MUD", Description = "Mudug region — capital Galkayo", RiskLevel = RiskLevel.High, Latitude = 6.7769, Longitude = 47.4305, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Region { Id = 4, Name = "Karkar", Code = "KAR", Description = "Karkar region", RiskLevel = RiskLevel.Medium, Latitude = 10.5, Longitude = 49.8, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Region { Id = 5, Name = "Ayn", Code = "AYN", Description = "Ayn region", RiskLevel = RiskLevel.Low, Latitude = 10.9, Longitude = 47.5, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Region { Id = 6, Name = "Sanaag", Code = "SAN", Description = "Sanaag region", RiskLevel = RiskLevel.Medium, Latitude = 10.7, Longitude = 47.3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed Districts
        builder.Entity<District>().HasData(
            new District { Id = 1, Name = "Garowe", RegionId = 1, Description = "Capital of Puntland", Population = 80000, RiskLevel = RiskLevel.Low, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new District { Id = 2, Name = "Eyl", RegionId = 1, Description = "Coastal district of Nugaal", Population = 15000, RiskLevel = RiskLevel.Medium, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new District { Id = 3, Name = "Bosaso", RegionId = 2, Description = "Major port city", Population = 150000, RiskLevel = RiskLevel.Low, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new District { Id = 4, Name = "Qardho", RegionId = 2, Description = "Central Bari district", Population = 30000, RiskLevel = RiskLevel.Low, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new District { Id = 5, Name = "Galkayo", RegionId = 3, Description = "Largest city in Mudug", Population = 200000, RiskLevel = RiskLevel.High, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new District { Id = 6, Name = "Hobyo", RegionId = 3, Description = "Coastal district", Population = 20000, RiskLevel = RiskLevel.Medium, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed Emergency Contacts
        builder.Entity<EmergencyContact>().HasData(
            new EmergencyContact { Id = 1, Name = "Puntland National Police", ContactType = ContactType.Police, PhoneNumber = "888", AlternativePhone = "0615-888888", RegionId = 1, IsAvailable24Hours = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new EmergencyContact { Id = 2, Name = "Garowe General Hospital", ContactType = ContactType.Hospital, PhoneNumber = "0615-123456", RegionId = 1, OperatingHours = "24/7 Emergency", IsAvailable24Hours = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new EmergencyContact { Id = 3, Name = "Bosaso Port Hospital", ContactType = ContactType.Hospital, PhoneNumber = "0617-234567", RegionId = 2, OperatingHours = "24/7 Emergency", IsAvailable24Hours = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new EmergencyContact { Id = 4, Name = "Puntland Fire Brigade", ContactType = ContactType.FireDepartment, PhoneNumber = "0615-999999", RegionId = 1, IsAvailable24Hours = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new EmergencyContact { Id = 5, Name = "PDRA — Puntland Disaster Response Authority", ContactType = ContactType.DisasterResponse, PhoneNumber = "0615-777777", Email = "pdra@puntland.gov.so", RegionId = 1, IsAvailable24Hours = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new EmergencyContact { Id = 6, Name = "Ambulance Services Garowe", ContactType = ContactType.Ambulance, PhoneNumber = "0615-222222", RegionId = 1, IsAvailable24Hours = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new EmergencyContact { Id = 7, Name = "Red Cross Puntland", ContactType = ContactType.RedCross, PhoneNumber = "0615-333333", Email = "puntland@redcross.org", RegionId = 1, IsAvailable24Hours = false, OperatingHours = "Mon-Fri 8:00-17:00", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
