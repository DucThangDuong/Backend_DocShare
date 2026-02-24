using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure;

public partial class DocShareContext : DbContext
{
    public DocShareContext()
    {
    }

    public DocShareContext(DbContextOptions<DocShareContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentVote> DocumentVotes { get; set; }

    public virtual DbSet<SavedDocument> SavedDocuments { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<University> Universities { get; set; }

    public virtual DbSet<UniversitySection> UniversitySections { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserFollow> UserFollows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC0763BE273D");

            entity.HasIndex(e => e.Slug, "UQ__Categori__BC7B5FB6DDC01C85").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Document__3214EC0736321AE9");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_UpdateUsedStorage_OnDelete");
                    tb.HasTrigger("trg_UpdateUsedStorage_OnInsert");
                });

            entity.HasIndex(e => e.IsDeleted, "IX_Documents_IsDeleted");

            entity.HasIndex(e => e.UploaderId, "IX_Documents_UploaderId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue((byte)0);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.Thumbnail).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.Documents)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Docs_Category");

            entity.HasOne(d => d.UniversitySection).WithMany(p => p.Documents)
                .HasForeignKey(d => d.UniversitySectionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Docs_UniSection");

            entity.HasOne(d => d.Uploader).WithMany(p => p.Documents)
                .HasForeignKey(d => d.UploaderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Docs_User");

            entity.HasMany(d => d.Tags).WithMany(p => p.Documents)
                .UsingEntity<Dictionary<string, object>>(
                    "DocumentTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("FK_DocTags_Tag"),
                    l => l.HasOne<Document>().WithMany()
                        .HasForeignKey("DocumentId")
                        .HasConstraintName("FK_DocTags_Doc"),
                    j =>
                    {
                        j.HasKey("DocumentId", "TagId").HasName("PK__Document__CCE92095073B48B4");
                        j.ToTable("DocumentTags");
                    });
        });

        modelBuilder.Entity<DocumentVote>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.DocumentId }).HasName("PK__Document__F62322BCB614AD4B");

            entity.ToTable(tb => tb.HasTrigger("trg_UpdateVoteCounts"));

            entity.Property(e => e.VotedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentVotes)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("FK_Votes_Doc");

            entity.HasOne(d => d.User).WithMany(p => p.DocumentVotes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Votes_User");
        });

        modelBuilder.Entity<SavedDocument>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.DocumentId }).HasName("PK__SavedDoc__F62322BC21A0FB9E");

            entity.Property(e => e.SavedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Document).WithMany(p => p.SavedDocuments)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("FK_Saved_Doc");

            entity.HasOne(d => d.User).WithMany(p => p.SavedDocuments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Saved_User");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tags__3214EC0772A57754");

            entity.HasIndex(e => e.Slug, "IX_Tags_Slug");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Slug)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<University>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Universi__3214EC07E40CCBA9");

            entity.HasIndex(e => e.Code, "UQ__Universi__A25C5AA7262B7A0D").IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<UniversitySection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Universi__3214EC07D57CF24C");

            entity.HasIndex(e => e.UniversityId, "IX_UniversitySections_UniversityId");

            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.University).WithMany(p => p.UniversitySections)
                .HasForeignKey(d => d.UniversityId)
                .HasConstraintName("FK_Sections_Uni");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07BEC75B67");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E47AAA47CD").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534BD709A31").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomAvatar)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasDefaultValue("default-avatar.jpg");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FollowerCount).HasDefaultValue(0);
            entity.Property(e => e.FollowingCount).HasDefaultValue(0);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.GoogleAvatar)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LoginProvider)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnType("datetime");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("User");
            entity.Property(e => e.StorageLimit).HasDefaultValueSql("((5368709120.))");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.University).WithMany(p => p.Users)
                .HasForeignKey(d => d.UniversityId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Users_Universities");
        });

        modelBuilder.Entity<UserFollow>(entity =>
        {
            entity.HasKey(e => new { e.FollowerId, e.FollowedId }).HasName("PK__UserFoll__F7A5FC9FC5A4F063");

            entity.ToTable(tb => tb.HasTrigger("trg_UpdateFollowerFollowingCounts"));

            entity.HasIndex(e => e.FollowedId, "IX_UserFollows_FollowedId");

            entity.HasIndex(e => e.FollowerId, "IX_UserFollows_FollowerId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Followed).WithMany(p => p.UserFollowFolloweds)
                .HasForeignKey(d => d.FollowedId)
                .HasConstraintName("FK_Follows_Followed");

            entity.HasOne(d => d.Follower).WithMany(p => p.UserFollowFollowers)
                .HasForeignKey(d => d.FollowerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Follows_Follower");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
