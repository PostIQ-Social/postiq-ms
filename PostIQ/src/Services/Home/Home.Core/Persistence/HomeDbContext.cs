using System;
using System.Collections.Generic;
using Home.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Home.Core.Persistence;

public partial class HomeDbContext : DbContext
{
    public HomeDbContext()
    {
    }

    public HomeDbContext(DbContextOptions<HomeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BatchJobStatus> BatchJobStatuses { get; set; }

    public virtual DbSet<PostsCount> PostsCount { get; set; }

    public virtual DbSet<PostLike> PostLikes { get; set; }

    public virtual DbSet<PostComment> PostComments { get; set; }

    public virtual DbSet<CommentLike> CommentLikes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostsCount>(entity =>
        {
            entity.ToTable("PostsCount", "Home");
            entity.HasKey(e => e.CountId);
        });


        modelBuilder.Entity<BatchJobStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK_SyncJob");

            entity.ToTable("BatchJobStatus", "Home");

            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        

        modelBuilder.Entity<PostLike>(entity =>
        {
            entity.ToTable("PostLikes", "Home");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<PostComment>(entity =>
        {
            entity.ToTable("PostComments", "Home");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);


            // Self-referencing relationship for replies
            entity.HasOne(d => d.ParentComment)
                .WithMany(p => p.Replies)
                .HasForeignKey(d => d.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CommentLike>(entity =>
        {
            entity.ToTable("CommentLikes", "Home");
            entity.HasKey(e => e.Id);

            entity.HasOne(d => d.Comment)
                .WithMany(p => p.Likes)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
