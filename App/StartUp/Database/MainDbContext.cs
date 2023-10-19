using App.Modules.Users.Data;
using App.StartUp.Options;
using App.StartUp.Services;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace App.StartUp.Database;

public class MainDbContext : DbContext
{
  public const string Key = "Main";
  public DbSet<UserData> Users { get; set; } = null!;

  private readonly IOptionsMonitor<Dictionary<string, DatabaseOption>> _options;

  public MainDbContext(IOptionsMonitor<Dictionary<string, DatabaseOption>> options)
  {
    this._options = options;
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder
      .AddPostgres(this._options.CurrentValue, Key)
      .UseExceptionProcessor()
      .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    var user = modelBuilder.Entity<UserData>();
    user.HasIndex(x => x.Username).IsUnique();
    user.HasIndex(x => x.Sub).IsUnique();
  }
}
