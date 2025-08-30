using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using X.PagedList.Extensions;

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
        return View();
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
    public IActionResult Demo1()
    {
        return View();
    }

    // ------------------------------------------------------------------------

    // GET: Home/Demo5
    public IActionResult Member_table(string? name, string? sort, string? dir, int page = 1)
    {
        // (1) Searching ------------------------
        ViewBag.Name = name = name?.Trim() ?? "";

        var searched = db.Members.Where(m => m.Name.Contains(name));

        // (2) Sorting --------------------------
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        Func<Member, object> fn = sort switch
        {
            "Photo" => m => m.PhotoURL,
            "Id" => m => m.UserId,
            "Name" => m => m.Name,
            "Email" => m => m.Email,
            _ => m => m.UserId,
        };

        var sorted = dir == "des" ?
                     searched.OrderByDescending(fn) :
                     searched.OrderBy(fn);

        // (3) Paging ---------------------------
        if (page < 1)
        {
            return RedirectToAction(null, new { name, sort, dir, page = 1 });
        }

        var m = sorted.ToPagedList(page, 10);

        if (page > m.PageCount && m.PageCount > 0)
        {
            return RedirectToAction(null, new { name, sort, dir, page = m.PageCount });
        }

        if (Request.IsAjax())
        {
            return PartialView("A_Mtable", m);
        }

        return View(m);
    }
}

//    // ------------------------------------------------------------------------

//    // GET: Home/Demo3
//    public IActionResult Demo3()
//    {
//        var model = db.Events;
//        return View(model);
//    }

//    // POST: Home/Delete
//    public IActionResult Delete(int id)
//    {
//        db.Events.Where(e => e.Id == id).ExecuteDelete();
//        TempData["Info"] = $"Event <b>#{id}</b> deleted.";
//        return RedirectToAction("Demo3");
//    }

//    // POST: Home/Truncate
//    [HttpPost]
//    public ActionResult Truncate()
//    {
//        // TODO
//        db.Database.ExecuteSqlRaw("TRUNCATE TABLE Events");

//        TempData["Info"] = "Events truncated.";

//        return RedirectToAction("Demo3");
//    }

//    // POST: Home/Import
//    [HttpPost]
//    public IActionResult Import(IFormFile file)
//    {
//        if (file != null
//            && file.FileName.EndsWith(".txt")
//            && file.ContentType == "text/plain")
//        {
//            int n = ImportEvents(file);
//            TempData["Info"] = $"{n} events imported.";
//        }

//        return RedirectToAction("Demo3");
//    }

//    private int ImportEvents(IFormFile file)
//    {
//        // Read from uploaded file --> import events
//        // Return number new events inserted
//        // TODO
//        using var stream = file.OpenReadStream();
//        using var reader = new StreamReader(stream);

//        while (!reader.EndOfStream)
//        {
//            var line = reader.ReadLine() ?? "";

//            if (line.Trim() == "") continue;

//            var data = line.Split("\t", StringSplitOptions.TrimEntries);

//            db.Events.Add(new()
//            {
//                Date = DateOnly.Parse(data[0]),
//                Name = data[1],
//            });
//        }

//        return db.SaveChanges();
//    }

//    // ------------------------------------------------------------------------

//    // GET: Home/Demo4
//    public IActionResult Demo4(int month, int year)
//    {
//        // Default min and max to current year
//        // --> Read from events if available
//        int min = DateTime.Today.Year;
//        int max = DateTime.Today.Year;

//        if (db.Events.Any())
//        {
//            min = db.Events.Min(e => e.Date.Year);
//            max = db.Events.Max(e => e.Date.Year);
//        }

//        // If month or year is out of range
//        // --> Redirect with current month and max year
//        if (month < 1 || month > 12 || year < min || year > max)
//        {
//            month = DateTime.Today.Month;
//            year = max;
//            return RedirectToAction(null, new { month, year });
//        }

//        // Pass month and year to UI
//        ViewBag.Month = month;
//        ViewBag.Year = year;

//        // For selection lists
//        ViewBag.MonthList = hp.GetMonthList();
//        ViewBag.YearList = hp.GetYearList(min, max);

//        // ********** Working with dictionary **********

//        var dict = new Dictionary<DateOnly, List<Event>>();

//        // First day (a) and last day (b) of the month
//        // TODO
//        var a = new DateOnly(year, month, 1);
//        var b = a.AddMonths(+1).AddDays(-1);

//        // Adjustment --> first day = Monday, last day = Sunday
//        // TODO
//        while (a.DayOfWeek != DayOfWeek.Monday) a = a.AddDays(-1);
//        while (b.DayOfWeek != DayOfWeek.Sunday) b = b.AddDays(+1);

//        // Fill dictionary with keys (dates) and values (events)
//        // TODO
//        for (var d = a; d <= b; d = d.AddDays(+1))
//        {
//            dict[d] = db.Events.Where(e => e.Date == d).ToList();
//        }
//        return View(dict);
//    }

