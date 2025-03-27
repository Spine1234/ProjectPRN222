using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    public int? MembershipPoints { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsAdmin { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
