using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Published.Core.Entities;

namespace Published.Core.Persistence;

public partial class PublishDbContext : DbContext
{
    public PublishDbContext()
    {
    }

    public PublishDbContext(DbContextOptions<PublishDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<Repo> Repos { get; set; }

    public virtual DbSet<RepoDetail> RepoDetails { get; set; }

    public virtual DbSet<ProcessedPost> ProcessedPosts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("Job", "Published");
            entity.HasKey(e => e.JobId).HasName("PK_Published.Job");
        });

        modelBuilder.Entity<Repo>(entity =>
        {
            entity.ToTable("Repos", "Published");
            entity.HasKey(e => e.RepoId).HasName("PK_Published_Repo.Job");

            entity.HasOne(d => d.Job).WithMany(p => p.Repos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Repos_Job");
        });

        modelBuilder.Entity<RepoDetail>(entity =>
        {
            entity.ToTable("RepoDetails", "Published");
            entity.HasKey(e => e.RepoDetailsId).HasName("PK_Publish.RepoDetails");

            entity.HasOne(d => d.Repo).WithMany(p => p.RepoDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RepoDetails_Repos");
        });

        modelBuilder.Entity<ProcessedPost>(entity =>
        {
            entity.ToTable("ProcessedPosts", "Published");
            entity.HasKey(e => e.ProcessedPostId).HasName("PK_Published.ProcessedPost");

            entity.HasOne(d => d.Repo).WithMany(p => p.ProcessedPosts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProcessedPosts_Repos");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
