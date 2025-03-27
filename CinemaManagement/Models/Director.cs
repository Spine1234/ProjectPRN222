using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Director
{
    public int DirectorId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Biography { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Nationality { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
