using Microsoft.AspNetCore.Mvc;
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
        var p = db.Tickets.ToList();

        var sorted = p.OrderByDescending(p => p.Sales)
                      .Take(5);
        return View(sorted);
    }

    // ------------------------------------------------------------------------

    // GET: Home/Email
    public IActionResult Email()
    {
        return View();
    }

    // POST: Home/Email
    [HttpPost]
    public IActionResult Email(EmailVM vm)
    {
        if (ModelState.IsValid)
        {
            // Construct email
            // TODO
            var mail = new MailMessage();
            mail.To.Add(new MailAddress(vm.Email, "My Lovely"));
            mail.Subject = vm.Subject;
            mail.Body = vm.Body;
            mail.IsBodyHtml = vm.IsBodyHtml;

            // File attachment (optional)
            // TODO
            var path = Path.Combine(en.ContentRootPath, "Secret.pdf");
            var att = new Attachment(path);
            mail.Attachments.Add(att);

            // Send email
            // TODO
            hp.SendEmail(mail);

            TempData["Info"] = "Email sent.";
            return RedirectToAction();
        }

        return View(vm);
    }




    // ------------------------------------------------------------------------
    // DateTime Demos
    // ------------------------------------------------------------------------

    // GET: Home/Demo1
    //public IActionResult Demo1()
    //{
    //    return View();
    //}
}