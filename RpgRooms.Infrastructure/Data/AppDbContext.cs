using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Core.Domain.Entities;

namespace RpgRooms.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignMember> CampaignMembers => Set<CampaignMember>();
    public DbSet<JoinRequest> JoinRequests => Set<JoinRequest>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<CampaignExit> CampaignExits => Set<CampaignExit>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<CampaignMember>().HasIndex(m => new { m.CampaignId, m.UserId }).IsUnique();
        b.Entity<Campaign>().Property(c => c.MaxPlayers).HasDefaultValue(50);
    }
}
