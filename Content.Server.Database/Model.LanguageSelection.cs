using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

internal static class ModelLanguageSelection
{
    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Language>()
            .HasIndex(p => new {HumanoidProfileId = p.ProfileId, p.LanguageEntryName})
            .IsUnique();
    }
}

public sealed class Language
{
    public int Id { get; set; }
    public Profile Profile { get; set; } = null!;
    public int ProfileId { get; set; }
    public string LanguageEntryName { get; set; } = null!;
    public string FluencyName { get; set; } = null!;
    public string Speaks { get; set; } = null!;
    public bool Primary { get; set; }
}