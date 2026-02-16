using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? GoogleId { get; set; }

    public string? FullName { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public string? Role { get; set; }
    public string? LoginProvider { get; set; }

    public string? CustomAvatar { get; set; }

    public string? GoogleAvatar { get; set; }

    public bool? IsActive { get; set; }

    public long StorageLimit { get; set; }

    public long UsedStorage { get; set; }

    public virtual ICollection<DocumentVote> DocumentVotes { get; set; } = new List<DocumentVote>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<SavedDocument> SavedDocuments { get; set; } = new List<SavedDocument>();
}
