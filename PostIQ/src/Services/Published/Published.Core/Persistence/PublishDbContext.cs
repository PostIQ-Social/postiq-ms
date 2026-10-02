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
    public virtual DbSet<PostsCount> PostsCount { get; set; }
    public virtual DbSet<PostLike> PostLikes { get; set; }
    public virtual DbSet<PostComment> PostComments { get; set; }
    public virtual DbSet<CommentLike> CommentLikes { get; set; }

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

        modelBuilder.Entity<PostsCount>(entity =>
        {
            entity.ToTable("PostsCount", "Published");
            entity.HasKey(e => e.CountId).HasName("PK_Published.PostsCount");
            entity.HasIndex(e => e.PostId).IsUnique()
                .HasDatabaseName("IX_Published.PostsCount_PostId");

            entity.HasOne(d => d.Post)
                .WithOne(p => p.PostsCount)
                .HasForeignKey<PostsCount>(d => d.PostId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PostsCount_ProcessedPosts");
        });

        modelBuilder.Entity<PostLike>(entity =>
        {
            entity.ToTable("PostLikes", "Published");
            entity.HasKey(e => e.Id).HasName("PK_PostLikes");
            entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique()
                .HasDatabaseName("UX_PostLikes_PostId_UserId");
        });

        modelBuilder.Entity<PostComment>(entity =>
        {
            entity.ToTable("PostComments", "Published");
            entity.HasKey(e => e.Id).HasName("PK_PostComments");
            entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);

            entity.HasOne(d => d.ParentComment)
                .WithMany(p => p.Replies)
                .HasForeignKey(d => d.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CommentLike>(entity =>
        {
            entity.ToTable("CommentLikes", "Published");
            entity.HasKey(e => e.Id).HasName("PK_CommentLikes");

            entity.HasOne(d => d.Comment)
                .WithMany(p => p.Likes)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
