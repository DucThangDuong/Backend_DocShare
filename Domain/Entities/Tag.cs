using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
