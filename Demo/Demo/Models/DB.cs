using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Models;

#nullable disable warnings

public class DB : DbContext
{
    public DB(DbContextOptions options) : base(options) { }

    // DB Sets
    public DbSet<User> Users { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

}

// Entity Classes -------------------------------------------------------------

public class User
{
    [Key, EmailAddress, MaxLength(100)]
    public string Email { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }
    [MaxLength(100)]
    public string Hash { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }

    public string Role => GetType().Name;

    [MaxLength(100)]
    public string? PhotoURL { get; set; }
    public bool Status { get; set; }

  
    public List<Order> Orders { get; set; } = [];
}


public class Admin : User
{

}

public class Member : User
{

}

public class Order
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderID { get; set; }
    public DateOnly OrderDate { get; set; }
    public bool Paid { get; set; }
    public bool Status { get; set; }
    public string UserEmail { get; set; }
    public User User { get; set; }
    public Payment? Payment { get; set; }
    public List<OrderLine> OrderLines { get; set; } = [];
}

public class Ticket
{
    [Key, MaxLength(4)]
    public string TicketID { get; set; }
    [MaxLength(100)]
    public string Type { get; set; }
    [Precision(6, 2)]
    public decimal UnitPrice { get; set; }
    public int Stock { get; set; }
    public DateTime DepartureTime { get; set; }
    [MaxLength(100)]
    public string Source { get; set; }
    [MaxLength(100)]
    public string Destination { get; set; }

    public bool Status { get; set; }
    public int Sales { get; set; }
    public int LocationId { get; set; }

    public List<OrderLine> Lines { get; set; } = [];
    public Location Location { get; set; }
}

public class OrderLine
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Precision(6, 2)]
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    // Foreign Keys
    public int OrderID { get; set; }
    public string TicketID { get; set; }

    // Navigation Properties
    public Order Order { get; set; }
    public Ticket Ticket { get; set; }
}

public class Payment
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string PaypalCaptureId { get; set; }

    public int OrderID { get; set; }
    public Order Order { get; set; }

}

public class Location
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public List<Ticket> Tickets { get; set; } = [];
}

public class PasswordResetToken
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime Expiry { get; set; }
}


