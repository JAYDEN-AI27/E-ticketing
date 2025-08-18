using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
}

// Entity Classes -------------------------------------------------------------

public class User
{
    [Key,MaxLength(4)]
    public string UserId { get; set; }

    [MaxLength(100)]
    public string Email { get; set; }
    [MaxLength(100)]
    public string Hash { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }

    public string Role => GetType().Name;
    // TODO
    public List<Order> Orders { get; set; } = [];
}

// TODO
public class Admin: User
{

}

// TODO
public class Member: User
{
    [MaxLength(100)]
    public string PhotoURL { get; set; }
}

public class Order
{
    [Key]
    public int OrderID { get; set; }
    public DateTime OrderDate { get; set; }
    [Precision(5,2)]
    public decimal Amount { get; set; }

    public User User { get; set; }
    public List<Ticket> Tickets { get; set; } = [];
}

public class Ticket
{
    [Key,MaxLength(4)]
    public string TicketID { get; set; }
    [MaxLength(100)]
    public string Type { get; set; }
    [Precision(4,2)]
    public decimal UnitPrice { get; set; }
    public int Stock {  get; set; }
    public DateTime DepartureTime {  get; set; }
    [MaxLength(100)]
    public string Source {  get; set; }
    [MaxLength(100)]
    public string Destination { get; set; }

    public List<Order> Orders { get; set; } = [];
}
