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
    [Required]
    [Display(Name = "User ID")]
    [Remote("CheckUserId", "Account", ErrorMessage = "Duplicated {0}.")]
    public int UserId { get; set; }

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
    [Display(Name = "User ID")]
    public int? UserId { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(100)]
    [Required]
    public string Name { get; set; }

    [Display(Name = "Current Photo")]
    public string? PhotoURL { get; set; }

    [Display(Name = "New Photo")]
    public IFormFile? Photo { get; set; }
}

public class ResetPasswordVM
{
    [Required, EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";
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

    public bool IsBodyHtml { get; set; }
}

//public class EventVM
//{
//    // TODO
//    [DataType(DataType.Date)]
//    public DateOnly Date { get; set; }

//    // TODO
//    [DataType(DataType.Time)]
//    public TimeOnly Time { get; set; }

//    [StringLength(100)]
//    public string Name { get; set; }
//}
