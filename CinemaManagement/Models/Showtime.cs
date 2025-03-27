using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Showtime
{
    public int ShowtimeId { get; set; }

    public int? MovieId { get; set; }

    public int? RoomId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal BasePrice { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Movie? Movie { get; set; }

    public virtual Room? Room { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
