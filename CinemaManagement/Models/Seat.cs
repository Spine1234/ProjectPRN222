using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Seat
{
    public int SeatId { get; set; }

    public int? RoomId { get; set; }

    public int? SeatTypeId { get; set; }

    public string RowNumber { get; set; } = null!;

    public int SeatNumber { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Room? Room { get; set; }

    public virtual SeatType? SeatType { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
