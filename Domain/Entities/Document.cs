using System;
using System.Collections.Generic;

namespace Domain.Entities;
public partial class Document
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string FileUrl { get; set; } = null!;

    public string FileHash { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public long SizeInBytes { get; set; }

    public int UploaderId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Uploader { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}

