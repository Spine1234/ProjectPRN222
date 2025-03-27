using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string Name { get; set; } = null!;

    public int Capacity { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
