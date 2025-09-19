using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace Demo.Controllers;

public class AccountController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;
    private readonly EmailService es;
    public AccountController(DB db, IWebHostEnvironment en, Helper hp, EmailService es)
    {
        this.db = db;
        this.en = en;
        this.hp = hp;
        this.es = es;
    }

    // GET: Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: Account/Login
    [HttpPost]
    public IActionResult Login(LoginVM vm, string? returnURL)
    {
        var u = db.Users.Find(vm.Email);

        if (u == null || !hp.VerifyPassword(u.Hash, vm.Password) || u.Status == false)
        {
            ModelState.AddModelError("", "Login credentials not matched.");
        }

        if (ModelState.IsValid)
        {
            TempData["Info"] = "Login successfully.";

            hp.SignIn(u!.Email, u.Role, vm.RememberMe);
            
            if (string.IsNullOrEmpty(returnURL))
            {
                return RedirectToAction("Index", "Home");
            }
        }
        
        return View(vm);
    }

    // GET: Account/Logout
    public IActionResult Logout(string? returnURL)
    {
        TempData["Info"] = "Logout successfully.";

        hp.SignOut();

        return RedirectToAction("Index", "Home");
    }

    // GET: Account/AccessDenied
    public IActionResult AccessDenied(string? returnURL)
    {
        return View();
    }



    // ------------------------------------------------------------------------
    // Others
    // ------------------------------------------------------------------------

    // GET: Account/CheckEmail
    public bool CheckEmail(string email)
    {
        return !db.Users.Any(u => u.Email == email);
    }

    // GET: Account/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: Account/Register
    [HttpPost]
    public IActionResult Register(RegisterVM vm)
    {
        if (ModelState.IsValid("Email") &&
            db.Users.Any(u => u.Email == vm.Email))
        {
            ModelState.AddModelError("Email", "Duplicated Email.");
        }

        if (ModelState.IsValid("Photo"))
        {
            var err = hp.ValidatePhoto(vm.Photo);
            if (err != "") ModelState.AddModelError("Photo", err);
        }
        
        if (ModelState.IsValid)
        {
            db.Members.Add(new()
            {
                Email = vm.Email,
                Hash = hp.HashPassword(vm.Password),
                Name = vm.Name,
                PhotoURL = hp.SavePhoto(vm.Photo, "photos"),
                Status = true
            });
            db.SaveChanges();

            TempData["Info"] = "Register successfully. Please login.";
            return RedirectToAction("Login");
        }

        return View(vm);
    }

    // GET: Account/UpdatePassword
    [Authorize]
    public IActionResult UpdatePassword()
    {
        return View();
    }

    // POST: Account/UpdatePassword
    [Authorize]
    [HttpPost]
    public IActionResult UpdatePassword(UpdatePasswordVM vm)
    {
        var u = db.Users.Find(User.Identity!.Name);
        if (u == null) return RedirectToAction("Index", "Home");

        if (!hp.VerifyPassword(u.Hash, vm.Current))
        {
            ModelState.AddModelError("Current", "Current Password not matched.");
        }

        if (ModelState.IsValid)
        {
            u.Hash = hp.HashPassword(vm.New);
            db.SaveChanges();

            TempData["Info"] = "Password updated.";
            hp.SignOut();
            return RedirectToAction("Login");
        }

        return View();
    }

    // GET: Account/UpdateProfile
    public IActionResult UpdateProfile()
    {
        var m = db.Users.Find(User.Identity!.Name);
        if (m == null) return RedirectToAction("Index", "Home");

        var vm = new UpdateProfileVM
        {
            Email = m.Email,
            Name = m.Name,
            PhotoURL = m.PhotoURL,
        };

        return View(vm);
    }

    // POST: Account/UpdateProfile
    //[Authorize(Roles = "Member")]
    [HttpPost]
    public IActionResult UpdateProfile(UpdateProfileVM vm)
    {
        var m = db.Users.Find(User.Identity!.Name);
        if (m == null) return RedirectToAction("Index", "Home");

        if (vm.Photo != null)
        {
            var err = hp.ValidatePhoto(vm.Photo);
            if (err != "") ModelState.AddModelError("Photo", err);
        }

        if (ModelState.IsValid)
        {
            m.Name = vm.Name;

            if (vm.Photo != null)
            {
                hp.DeletePhoto(m.PhotoURL, "photos");
                m.PhotoURL = hp.SavePhoto(vm.Photo, "photos");
            }

            db.SaveChanges();

            TempData["Info"] = "Profile updated.";
            return RedirectToAction();
        }

        vm.Email = m.Email;
        vm.PhotoURL = m.PhotoURL;
        return View(vm);
    }

    // GET: Forget Password
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // POST: Forget Password
    [HttpPost]
    public IActionResult ForgotPassword(string email)
    {
        var user = db.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            ModelState.AddModelError("", "Email not found.");
            return View();
        }

        // Generate token
        string token = Guid.NewGuid().ToString();

        // Save to DB with expiry (e.g. 1 hour)
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            Email = email,
            Token = token,
            Expiry = DateTime.UtcNow.AddHours(1)
        });
        db.SaveChanges();

        // Generate link
        string resetLink = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme)
                           ?? "/Account/ResetPassword";

        // Send email
        es.SendEmail(email, "Password Reset",
        $@"
            <img src='cid:photo' 
                 style='width: 200px; height: 200px; border: 1px solid #333'>

            <p>Dear User,</p>

            Click <a href='{resetLink}'>here</a> to reset your password.

            <h1 style='color: Green'>Thank You</h1>

            <p>From, 🐱 Super Admin</p>
        ");

        TempData["Info"] = "Reset link sent. Please check your email.";
        return RedirectToAction("Login");
    }

    // GET: Account/Reset Password
    public IActionResult ResetPassword(string token)
    {
        var reset = db.PasswordResetTokens.FirstOrDefault(t => t.Token == token && t.Expiry > DateTime.UtcNow);
        if (reset == null)
        {
            TempData["Error"] = "Invalid or expired token.";
            return RedirectToAction("ForgotPassword");
        }

        return View(new ResetPasswordVM { Token = token });
    }
    
    //POST: Account//Reset Password
    [HttpPost]
    public IActionResult ResetPassword(ResetPasswordVM vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var reset = db.PasswordResetTokens.FirstOrDefault(t => t.Token == vm.Token && t.Expiry > DateTime.UtcNow);
        if (reset == null)
        {
            TempData["Error"] = "Invalid or expired token.";
            return RedirectToAction("ForgotPassword");
        }

        var user = db.Users.Find(reset.Email);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("ForgotPassword");
        }

        // Update password
        user.Hash = hp.HashPassword(vm.NewPassword);
        db.PasswordResetTokens.Remove(reset); // delete token after use
        db.SaveChanges();

        TempData["Info"] = "Password reset successful. Please login.";
        return RedirectToAction("Login");
    }

    [HttpPost]
    public IActionResult ChangeUserStatus(string? email)
    {
        if (!string.IsNullOrEmpty(email))
        {
            var s = db.Users.Find(email);
            if (s != null)
            {
                if (s.Status == true)
                {
                    s.Status = false;
                    TempData["Info"] = "User Deactivate.";
                }
                else
                {
                    s.Status = true;
                    TempData["Info"] = "User Activate.";
                }
                db.SaveChanges();
            }
        }

        var referer = Request.Headers.Referer.ToString();
        return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction("Index");
    }
}