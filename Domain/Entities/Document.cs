using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Document
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string FileUrl { get; set; } = null!;

    public long SizeInBytes { get; set; }

    public string? Thumbnail { get; set; }

    public int? PageCount { get; set; }

    public int UploaderId { get; set; }

    public int? CategoryId { get; set; }

    public string? Status { get; set; }

    public byte? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int ViewCount { get; set; }

    public int LikeCount { get; set; }

    public int DislikeCount { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<DocumentVote> DocumentVotes { get; set; } = new List<DocumentVote>();

    public virtual ICollection<SavedDocument> SavedDocuments { get; set; } = new List<SavedDocument>();

    public virtual User Uploader { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
