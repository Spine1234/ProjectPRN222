using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Movie
{
    public int MovieId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? DirectorId { get; set; }

    public int Duration { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? PosterUrl { get; set; }

    public string? TrailerUrl { get; set; }

    public string? Status { get; set; }

    public decimal? Rating { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Director? Director { get; set; }

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
