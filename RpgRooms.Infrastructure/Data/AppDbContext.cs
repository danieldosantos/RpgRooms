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
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<SavingThrowProficiency> SavingThrowProficiencies => Set<SavingThrowProficiency>();
    public DbSet<SkillProficiency> SkillProficiencies => Set<SkillProficiency>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Spell> Spells => Set<Spell>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<CampaignMember>().HasIndex(m => new { m.CampaignId, m.UserId }).IsUnique();
        b.Entity<Campaign>()
            .HasIndex(c => new { c.OwnerUserId, c.Name })
            .IsUnique();
        b.Entity<Campaign>().Property(c => c.MaxPlayers).HasDefaultValue(50);

        b.Entity<Character>()
            .HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(c => c.CampaignId);

        b.Entity<Character>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.UserId);

        b.Entity<SavingThrowProficiency>()
            .HasOne<Character>()
            .WithMany(c => c.SavingThrowProficiencies)
            .HasForeignKey(p => p.CharacterId);

        b.Entity<SkillProficiency>()
            .HasOne<Character>()
            .WithMany(c => c.SkillProficiencies)
            .HasForeignKey(p => p.CharacterId);

        b.Entity<InventoryItem>()
            .HasOne<Character>()
            .WithMany(c => c.Inventory)
            .HasForeignKey(i => i.CharacterId);

        b.Entity<Spell>()
            .HasOne<Character>()
            .WithMany(c => c.Spells)
            .HasForeignKey(s => s.CharacterId);
    }
}
