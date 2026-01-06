using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public string? Role { get; set; }

    public string? AvatarUrl { get; set; }

    public bool? IsActive { get; set; }

    public long StorageLimit { get; set; }

    public long UsedStorage { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
