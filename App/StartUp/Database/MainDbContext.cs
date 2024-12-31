using App.Modules.Cyan.Data;
using App.Modules.Cyan.Data.Models;
using App.Modules.Users.Data;
using App.StartUp.Options;
using App.StartUp.Services;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NpgsqlTypes;

namespace App.StartUp.Database;

public class MainDbContext(IOptionsMonitor<Dictionary<string, DatabaseOption>> options)
  : DbContext
{
  public const string Key = "MAIN";
  public DbSet<UserData> Users { get; set; } = null!;
  public DbSet<TokenData> Tokens { get; set; } = null!;

  public DbSet<TemplateData> Templates { get; set; } = null!;
  public DbSet<TemplateVersionData> TemplateVersions { get; set; } = null!;
  public DbSet<TemplateProcessorVersionData> TemplateProcessorVersions { get; set; } = null!;
  public DbSet<TemplatePluginVersionData> TemplatePluginVersions { get; set; } = null!;

  public DbSet<PluginData> Plugins { get; set; } = null!;
  public DbSet<PluginVersionData> PluginVersions { get; set; } = null!;

  public DbSet<ProcessorData> Processors { get; set; } = null!;
  public DbSet<ProcessorVersionData> ProcessorVersions { get; set; } = null!;

  // likes
  public DbSet<TemplateLikeData> TemplateLikes { get; set; } = null!;
  public DbSet<PluginLikeData> PluginLikes { get; set; } = null!;
  public DbSet<ProcessorLikeData> ProcessorLikes { get; set; } = null!;

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder
      .AddPostgres(options.CurrentValue, Key)
      .UseExceptionProcessor()
      .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    var user = modelBuilder.Entity<UserData>();
    user.HasIndex(x => x.Username).IsUnique();

    user.HasMany<TokenData>(x => x.Tokens)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId);

    user.HasMany<TemplateData>(x => x.Templates)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId);

    user.HasMany<PluginData>(x => x.Plugins)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId);

    user.HasMany<ProcessorData>(x => x.Processors)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId);

    user.HasMany<TemplateLikeData>(x => x.TemplateLikes)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId);

    user.HasMany<PluginLikeData>(x => x.PluginLikes)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId);

    user.HasMany<ProcessorLikeData>(x => x.ProcessorLikes)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId);

    var templateLikes = modelBuilder.Entity<TemplateLikeData>();
    templateLikes
      .HasIndex(x => new { x.UserId, x.TemplateId })
      .IsUnique();

    var pluginLikes = modelBuilder.Entity<PluginLikeData>();
    pluginLikes
      .HasIndex(x => new { x.UserId, x.PluginId })
      .IsUnique();

    var processorLikes = modelBuilder.Entity<ProcessorLikeData>();
    processorLikes
      .HasIndex(x => new { x.UserId, x.ProcessorId })
      .IsUnique();

    var token = modelBuilder.Entity<TokenData>();
    token.HasIndex(x => x.ApiToken).IsUnique();


    var template = modelBuilder.Entity<TemplateData>();
    template.HasIndex(x => new { x.UserId, x.Name }).IsUnique();

    template
      .HasGeneratedTsVectorColumn(
        p => p.SearchVector,
        "english",
        p => new { p.Name, p.Description })
      .HasIndex(p => p.SearchVector)
      .HasMethod("GIN");

    template.HasMany<TemplateVersionData>(x => x.Versions)
      .WithOne(x => x.Template)
      .HasForeignKey(x => x.TemplateId);

    template.HasMany<TemplateLikeData>(x => x.Likes)
      .WithOne(x => x.Template)
      .HasForeignKey(x => x.TemplateId);

    var templateVersion = modelBuilder.Entity<TemplateVersionData>();

    templateVersion.HasIndex(x => new { x.Id, x.Version }).IsUnique();

    templateVersion.HasMany<TemplateProcessorVersionData>(x => x.Processors)
      .WithOne(x => x.Template)
      .HasForeignKey(x => x.TemplateId);

    templateVersion.HasMany<TemplatePluginVersionData>(x => x.Plugins)
      .WithOne(x => x.Template)
      .HasForeignKey(x => x.TemplateId);


    var plugin = modelBuilder.Entity<PluginData>();
    plugin.HasIndex(x => new { x.UserId, x.Name }).IsUnique();

    plugin
      .HasGeneratedTsVectorColumn(
        p => p.SearchVector,
        "english",
        p => new { p.Name, p.Description })
      .HasIndex(p => p.SearchVector)
      .HasMethod("GIN");

    plugin.HasMany<PluginVersionData>(x => x.Versions)
      .WithOne(x => x.Plugin)
      .HasForeignKey(x => x.PluginId);

    plugin.HasMany<PluginLikeData>(x => x.Likes)
      .WithOne(x => x.Plugin)
      .HasForeignKey(x => x.PluginId);

    var pluginVersion = modelBuilder.Entity<PluginVersionData>();

    pluginVersion.HasIndex(x => new { x.Id, x.Version }).IsUnique();

    var processor = modelBuilder.Entity<ProcessorData>();
    processor.HasIndex(x => new { x.UserId, x.Name }).IsUnique();

    processor
      .HasGeneratedTsVectorColumn(
        p => p.SearchVector,
        "english",
        p => new { p.Name, p.Description })
      .HasIndex(p => p.SearchVector)
      .HasMethod("GIN");

    processor.HasMany<ProcessorVersionData>(x => x.Versions)
      .WithOne(x => x.Processor)
      .HasForeignKey(x => x.ProcessorId);

    processor.HasMany<ProcessorLikeData>(x => x.Likes)
      .WithOne(x => x.Processor)
      .HasForeignKey(x => x.ProcessorId);

    var processorVersion = modelBuilder.Entity<ProcessorVersionData>();
    processorVersion.HasIndex(x => new { x.Id, x.Version }).IsUnique();
  }
}
