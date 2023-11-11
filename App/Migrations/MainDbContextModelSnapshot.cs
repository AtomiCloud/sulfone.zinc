﻿// <auto-generated />
using System;
using App.StartUp.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace App.Migrations
{
    [DbContext(typeof(MainDbContext))]
    partial class MainDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0-preview.4.23259.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.PluginData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("Downloads")
                        .HasColumnType("bigint");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Project")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Readme")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<NpgsqlTsVector>("SearchVector")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("tsvector")
                        .HasAnnotation("Npgsql:TsVectorConfig", "english")
                        .HasAnnotation("Npgsql:TsVectorProperties", new[] { "Name", "Description" });

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string[]>("Tags")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SearchVector");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("SearchVector"), "GIN");

                    b.HasIndex("UserId", "Name")
                        .IsUnique();

                    b.ToTable("Plugins");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.PluginLikeData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("PluginId")
                        .HasColumnType("uuid");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("PluginId");

                    b.HasIndex("UserId", "PluginId")
                        .IsUnique();

                    b.ToTable("PluginLikes");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.PluginVersionData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DockerReference")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DockerTag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("PluginId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Version")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("PluginId");

                    b.HasIndex("Id", "Version")
                        .IsUnique();

                    b.ToTable("PluginVersions");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.ProcessorData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("Downloads")
                        .HasColumnType("bigint");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Project")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Readme")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<NpgsqlTsVector>("SearchVector")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("tsvector")
                        .HasAnnotation("Npgsql:TsVectorConfig", "english")
                        .HasAnnotation("Npgsql:TsVectorProperties", new[] { "Name", "Description" });

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string[]>("Tags")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SearchVector");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("SearchVector"), "GIN");

                    b.HasIndex("UserId", "Name")
                        .IsUnique();

                    b.ToTable("Processors");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.ProcessorLikeData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ProcessorId")
                        .HasColumnType("uuid");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ProcessorId");

                    b.HasIndex("UserId", "ProcessorId")
                        .IsUnique();

                    b.ToTable("ProcessorLikes");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.ProcessorVersionData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DockerReference")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DockerTag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ProcessorId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Version")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("ProcessorId");

                    b.HasIndex("Id", "Version")
                        .IsUnique();

                    b.ToTable("ProcessorVersions");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.TemplateData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("Downloads")
                        .HasColumnType("bigint");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Project")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Readme")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<NpgsqlTsVector>("SearchVector")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("tsvector")
                        .HasAnnotation("Npgsql:TsVectorConfig", "english")
                        .HasAnnotation("Npgsql:TsVectorProperties", new[] { "Name", "Description" });

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string[]>("Tags")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SearchVector");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("SearchVector"), "GIN");

                    b.HasIndex("UserId", "Name")
                        .IsUnique();

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.TemplateLikeData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("TemplateId")
                        .HasColumnType("uuid");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TemplateId");

                    b.HasIndex("UserId", "TemplateId")
                        .IsUnique();

                    b.ToTable("TemplateLikes");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.TemplatePluginVersionData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("PluginId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TemplateId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PluginId");

                    b.HasIndex("TemplateId");

                    b.ToTable("TemplatePluginVersions");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.TemplateProcessorVersionData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ProcessorId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TemplateId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ProcessorId");

                    b.HasIndex("TemplateId");

                    b.ToTable("TemplateProcessorVersions");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.TemplateVersionData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("BlobDockerReference")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BlobDockerTag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TemplateDockerReference")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TemplateDockerTag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("TemplateId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Version")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("TemplateId");

                    b.HasIndex("Id", "Version")
                        .IsUnique();

                    b.ToTable("TemplateVersions");
                });

            modelBuilder.Entity("App.Modules.Users.Data.TokenData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ApiToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Revoked")
                        .HasColumnType("boolean");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ApiToken")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("Tokens");
                });

            modelBuilder.Entity("App.Modules.Users.Data.UserData", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.PluginData", b =>
                {
                    b.HasOne("App.Modules.Users.Data.UserData", "User")
                        .WithMany("Plugins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.PluginLikeData", b =>
                {
                    b.HasOne("App.Modules.Cyan.Data.Models.PluginData", "Plugin")
                        .WithMany("Likes")
                        .HasForeignKey("PluginId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("App.Modules.Users.Data.UserData", "User")
                        .WithMany("PluginLikes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plugin");

                    b.Navigation("User");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.PluginVersionData", b =>
                {
                    b.HasOne("App.Modules.Cyan.Data.Models.PluginData", "Plugin")
                        .WithMany("Versions")
                        .HasForeignKey("PluginId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plugin");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.ProcessorData", b =>
                {
                    b.HasOne("App.Modules.Users.Data.UserData", "User")
                        .WithMany("Processors")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.ProcessorLikeData", b =>
                {
                    b.HasOne("App.Modules.Cyan.Data.Models.ProcessorData", "Processor")
                        .WithMany("Likes")
                        .HasForeignKey("ProcessorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("App.Modules.Users.Data.UserData", "User")
                        .WithMany("ProcessorLikes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Processor");

                    b.Navigation("User");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.ProcessorVersionData", b =>
                {
                    b.HasOne("App.Modules.Cyan.Data.Models.ProcessorData", "Processor")
                        .WithMany("Versions")
                        .HasForeignKey("ProcessorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Processor");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.TemplateData", b =>
                {
                    b.HasOne("App.Modules.Users.Data.UserData", "User")
                        .WithMany("Templates")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.TemplateLikeData", b =>
                {
                    b.HasOne("App.Modules.Cyan.Data.Models.TemplateData", "Template")
                        .WithMany("Likes")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("App.Modules.Users.Data.UserData", "User")
                        .WithMany("TemplateLikes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Template");

                    b.Navigation("User");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.TemplatePluginVersionData", b =>
                {
                    b.HasOne("App.Modules.Cyan.Data.Models.PluginVersionData", "Plugin")
                        .WithMany("Templates")
                        .HasForeignKey("PluginId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("App.Modules.Cyan.Data.Models.TemplateVersionData", "Template")
                        .WithMany("Plugins")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plugin");

                    b.Navigation("Template");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.TemplateProcessorVersionData", b =>
                {
                    b.HasOne("App.Modules.Cyan.Data.Models.ProcessorVersionData", "Processor")
                        .WithMany("Templates")
                        .HasForeignKey("ProcessorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("App.Modules.Cyan.Data.Models.TemplateVersionData", "Template")
                        .WithMany("Processors")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Processor");

                    b.Navigation("Template");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.TemplateVersionData", b =>
                {
                    b.HasOne("App.Modules.Cyan.Data.Models.TemplateData", "Template")
                        .WithMany("Versions")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Template");
                });

            modelBuilder.Entity("App.Modules.Users.Data.TokenData", b =>
                {
                    b.HasOne("App.Modules.Users.Data.UserData", "User")
                        .WithMany("Tokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.PluginData", b =>
                {
                    b.Navigation("Likes");

                    b.Navigation("Versions");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.PluginVersionData", b =>
                {
                    b.Navigation("Templates");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.ProcessorData", b =>
                {
                    b.Navigation("Likes");

                    b.Navigation("Versions");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.ProcessorVersionData", b =>
                {
                    b.Navigation("Templates");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.TemplateData", b =>
                {
                    b.Navigation("Likes");

                    b.Navigation("Versions");
                });

            modelBuilder.Entity("App.Modules.Cyan.Data.Models.TemplateVersionData", b =>
                {
                    b.Navigation("Plugins");

                    b.Navigation("Processors");
                });

            modelBuilder.Entity("App.Modules.Users.Data.UserData", b =>
                {
                    b.Navigation("PluginLikes");

                    b.Navigation("Plugins");

                    b.Navigation("ProcessorLikes");

                    b.Navigation("Processors");

                    b.Navigation("TemplateLikes");

                    b.Navigation("Templates");

                    b.Navigation("Tokens");
                });
#pragma warning restore 612, 618
        }
    }
}
