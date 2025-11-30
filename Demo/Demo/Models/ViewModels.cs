using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Demo.Models;

#nullable disable warnings

// View Models ----------------------------------------------------------------

public class LoginVM
{

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Required]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}

public class RegisterVM
{

    [StringLength(100)]
    [EmailAddress]
    [Required]
    [Remote("CheckEmail", "Account", ErrorMessage = "Duplicated {0}.")]
    public string Email { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Required]
    public string Password { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [Compare("Password")]
    [DataType(DataType.Password)]
    [Required]
    [Display(Name = "Confirm Password")]
    public string Confirm { get; set; }

    [StringLength(100)]
    [Required]
    public string Name { get; set; }

    [Required]
    public IFormFile Photo { get; set; }
}

public class UpdatePasswordVM
{
    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    [Required]
    public string Current { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    [Required]
    public string New { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [Compare("New")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Required]
    public string Confirm { get; set; }
}

public class UpdateProfileVM
{

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(100)]
    [Required]
    [Display(Name = "Full Name")]
    public string Name { get; set; }

    [Display(Name = "Current Photo")]
    public string? PhotoURL { get; set; }

    [Display(Name = "New Photo")]
    public IFormFile? Photo { get; set; }
}

public class ResetPasswordVM
{
    [Required(ErrorMessage = "New password is required.")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage ="Password must be at least 6 characters.")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage ="Please confirm new password.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage ="Password do not match.")]
    public string ConfirmPassword { get; set; }

    [Required]
    public string Token { get; set; }
}

public class EmailVM
{
    [StringLength(100)]
    [EmailAddress]
    [Required]
    public string Email { get; set; }

    [Required]
    public string Subject { get; set; }

    [Required]
    public string Body { get; set; }

}

public class TicketVM
{
    [StringLength(4)]
    [RegularExpression(@"^T\d{3}$", ErrorMessage = "Invalid {0}. (Format: T001)")]
    [Remote("CheckId", "Ticket", ErrorMessage = "Duplicated {0}.")]
    [Display(Name = "Ticket ID")]
    public string TicketID { get; set; }

    [Required]
    [StringLength(50)]
    public string Type { get; set; }

    [Range(0, 9999.99, ErrorMessage = "{0} must be between {1} and {2}.")]
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "{0} must be a positive number.")]
    public int Stock { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Departure Time")]
    public DateTime DepartureTime { get; set; }

    [Required]
    [StringLength(100)]
    public string Source { get; set; }

    [Required]
    [StringLength(100)]
    public string Destination { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

// Shopping Cart
public class CartItem
{
    public Ticket Ticket { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}

//Payment
//public class PaymentVM
//{
//    public int PaymentID { get; set; }

//    [Display(Name = "Card Number")]
//    [Required(ErrorMessage = "Card number is required")]
//    [StringLength(16, MinimumLength = 16, ErrorMessage = "Card number must be exactly 16 digits")]
//    [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must contain exactly 16 digits")]
//    public string cardNum { get; set; }

//    [Display(Name = "CCV")]
//    [Required(ErrorMessage = "CCV is required")]
//    [StringLength(3, MinimumLength = 3, ErrorMessage = "CCV must be exactly 3 digits")]
//    [RegularExpression(@"^\d{3}$", ErrorMessage = "CCV must contain exactly 3 digits")]
//    public string ccv { get; set; }

//    [Display(Name = "Expired Month")]
//    [Range(1, 12)]
//    public int expire_month { get; set; }

//    [Display(Name = "Expired Year")]
//    [Range(25, 60)]
//    public int expire_year { get; set; }

//}

//public class ChooseCardVM
//{
//    public int SelectedPaymentId { get; set; }
//    public List<Payment> Payments { get; set; } = new();
//}
