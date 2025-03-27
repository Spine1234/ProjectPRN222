using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public int? BookingId { get; set; }

    public int? ShowtimeId { get; set; }

    public int? SeatId { get; set; }

    public decimal Price { get; set; }

    public string? TicketCode { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Seat? Seat { get; set; }

    public virtual Showtime? Showtime { get; set; }
}
