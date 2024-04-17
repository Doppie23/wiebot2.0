using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DataBase;

public partial class DataBaseContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public string DbPath { get; }

    public DataBaseContext()
    {
        var path = AppDomain.CurrentDomain.BaseDirectory;
        DbPath = System.IO.Path.Join(path, "database.db");
        Console.WriteLine(DbPath);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseSqlite($"Data Source={DbPath}");
}

////////////////////////////////////////////////////////////////////////////////
///////////////////////////// tables ///////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

[PrimaryKey(nameof(Id), nameof(GuildId))]
public class User
{
    [Required]
    public ulong Id { get; set; }

    [Required]
    public ulong GuildId { get; set; }

    /// <summary>
    ///     Will be null if the user is not part of the rng game
    /// </summary>
    public int? RngScore { get; set; } = null;
    public int OutroScore { get; set; } = 0;
}
