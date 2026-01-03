using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public partial class DocShareSimpleDbContext : DbContext
    {
        public DocShareSimpleDbContext()
        {
        }

        public DocShareSimpleDbContext(DbContextOptions<DocShareSimpleDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Document> Documents { get; set; }

        public virtual DbSet<Tag> Tags { get; set; }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Document__3214EC0732BA2428");

                entity.HasIndex(e => e.FileHash, "IX_Documents_FileHash");

                entity.HasIndex(e => e.Slug, "UQ__Document__BC7B5FB6E7DE89FD").IsUnique();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Extension)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.FileHash)
                    .HasMaxLength(64)
                    .IsUnicode(false);
                entity.Property(e => e.FileUrl)
                    .HasMaxLength(500)
                    .IsUnicode(false);
                entity.Property(e => e.Slug)
                    .HasMaxLength(250)
                    .IsUnicode(false);
                entity.Property(e => e.Title).HasMaxLength(200);

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
                            j.HasKey("DocumentId", "TagId").HasName("PK__Document__CCE92095B561F0EC");
                            j.ToTable("DocumentTags");
                        });
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Tags__3214EC0713223C17");

                entity.HasIndex(e => e.Slug, "IX_Tags_Slug");

                entity.HasIndex(e => e.Slug, "UQ__Tags__BC7B5FB66CA004F1").IsUnique();

                entity.Property(e => e.Name).HasMaxLength(50);
                entity.Property(e => e.Slug)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07863DCE0C");

                entity.HasIndex(e => e.Username, "UQ__Users__536C85E49F58AFD6").IsUnique();

                entity.HasIndex(e => e.Email, "UQ__Users__A9D10534006133DD").IsUnique();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(255)
                    .IsUnicode(false);
                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.RefreshToken)
                    .HasMaxLength(65)
                    .IsUnicode(false);
                entity.Property(e => e.RefreshTokenExpiryTime)
                    .HasColumnType("datetime");
                entity.Property(e => e.Role)
                    .HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

}