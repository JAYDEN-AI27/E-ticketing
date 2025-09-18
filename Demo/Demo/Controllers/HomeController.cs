using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;

namespace Demo.Controllers;

public class HomeController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;

    public HomeController(DB db, IWebHostEnvironment en, Helper hp)
    {
        this.db = db;
        this.en = en;
        this.hp = hp;
    }

    // ------------------------------------------------------------------------

    // GET: Home/Index
    public IActionResult Index()
    {
        var p = db.Tickets.Where(t => t.Status == true).ToList();

        var sorted = p.OrderByDescending(p => p.Sales)
                      .Take(5);
        return View(sorted);
    }

    // ------------------------------------------------------------------------

    // GET: Home/Email
    public IActionResult Email()
    {

        var emailList = db.Admins
        .Where(a => a.Status == true)
        .Select(a => new SelectListItem
        {
            Value = a.Email,
            Text = $"{a.Name} ({a.Email})"
        })
        .ToList();

        ViewBag.EmailList = emailList;

        return View("Email", new EmailVM());
    }

    // POST: Home/Email
    [HttpPost]
    public IActionResult Email(EmailVM vm)
    {
        if (ModelState.IsValid)
        {
            // Construct email
            
            var mail = new MailMessage();
            mail.To.Add(new MailAddress(vm.Email, "My Lovely"));
            mail.Subject = vm.Subject;
            mail.Body = vm.Body;
           
            // Send email
            
            hp.SendEmail(mail);

            TempData["Info"] = "Email sent.";
            return RedirectToAction();
        }

        return View(vm);
    }
}