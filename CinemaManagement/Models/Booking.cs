using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? BookingDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentStatus { get; set; }

    public string? BookingStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
