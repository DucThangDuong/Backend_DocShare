using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DocumentVote
{
    public int UserId { get; set; }

    public long DocumentId { get; set; }

    public bool IsLike { get; set; }

    public DateTime? VotedAt { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
