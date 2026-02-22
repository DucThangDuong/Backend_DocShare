using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class UniversitySection
{
    public int Id { get; set; }

    public int UniversityId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual University University { get; set; } = null!;
}
