using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class University
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<UniversitySection> UniversitySections { get; set; } = new List<UniversitySection>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
