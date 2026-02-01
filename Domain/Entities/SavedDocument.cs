using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class SavedDocument
{
    public int UserId { get; set; }

    public long DocumentId { get; set; }

    public DateTime? SavedAt { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
