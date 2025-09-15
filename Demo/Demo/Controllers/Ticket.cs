using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;



namespace Demo.Controllers;

public class TicketController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;

    public TicketController(DB db, IWebHostEnvironment en)
    {
        this.db = db;
        this.en = en;
    }

    public IActionResult Ticket(string? type,string? source,string? destination, string? sort, string? dir, int page = 1)
    {
        // (1) searching ------------------------
        ViewBag.Type = type = type?.Trim() ?? "";
        ViewBag.Source = source = source?.Trim() ?? "";
        ViewBag.Destination = destination = destination?.Trim() ?? "";

        var searched = db.Tickets
            .Where(s => string.IsNullOrEmpty(type) || s.Type.Contains(type))
            .Where(s => string.IsNullOrEmpty(source) || s.Source.Contains(source))
            .Where(s => string.IsNullOrEmpty(destination) || s.Destination.Contains(destination));

        // (2) Sorting --------------------------
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        Func<Ticket, object> fn = sort switch
        {
            "TicketID" => s => s.TicketID,
            "Type" => s => s.Type,
            "UnitPrice" => s => s.UnitPrice, 
            "Stock" => s => s.Stock,
            "DepartureTime" => s => s.DepartureTime,
            "Source" => s => s.Source,
            "Destination" => s => s.Destination,
            _ => s => s.TicketID,
        };

        var sorted = dir == "des" ?
                     searched.OrderByDescending(fn) :
                     searched.OrderBy(fn);

        // (3) Paging ---------------------------
        if (page < 1)
        {
            return RedirectToAction(null, new { type, sort, dir, page = 1 });
        }

        var m = sorted.ToPagedList(page, 10);

        if (page > m.PageCount && m.PageCount > 0)
        {
            return RedirectToAction(null, new { type, sort, dir, page = m.PageCount });
        }

        if (Request.IsAjax())
        {
            return PartialView("_T", m);
        }

        return View(m);
    }

    // GET: Ticket/Detail
    public IActionResult Detail(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToAction("Ticket");
        }

        var model = db.Tickets.Find(id);
        if (model == null)
        {
            return RedirectToAction("Ticket");
        }
        return View(model);
    }

    // GET: Ticket/CheckId 
    public bool CheckId(string TicketID)
    {
        if (string.IsNullOrEmpty(TicketID))
            return false;

        return !db.Tickets.Any(s => s.TicketID == TicketID);
    }

    // GET: Ticket/Insert
    public IActionResult Insert()
    {
        return View();
    }

    // POST: Ticket/Insert
    [HttpPost]
    public IActionResult Insert(TicketVM vm)
    {
        
        if (db.Tickets.Any(s => s.TicketID == vm.TicketID.Trim().ToUpper()))
        {
            ModelState.AddModelError("TicketID", "Duplicated Ticket ID.");
        }

        if (ModelState.IsValid)
        {
            var ticket = new Ticket() 
            {
                TicketID = vm.TicketID.Trim().ToUpper(),
                Type = vm.Type.Trim().ToUpper(),
                UnitPrice = vm.UnitPrice,
                Stock = vm.Stock,
                DepartureTime = vm.DepartureTime,
                Source = vm.Source,
                Destination = vm.Destination,
            };

            db.Tickets.Add(ticket);
            db.SaveChanges();
            TempData["Info"] = "Record inserted.";
            return RedirectToAction("Ticket");
        }

        return View(vm);
    }

    // GET: Ticket/Update
    public IActionResult Update(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToAction("Ticket");
        }

        var s = db.Tickets.Find(id);
        if (s == null)
        {
            return RedirectToAction("Ticket");
        }

        var vm = new TicketVM
        {
            TicketID = s.TicketID,
            Type = s.Type,
            UnitPrice = s.UnitPrice,
            Stock = s.Stock,
            DepartureTime = s.DepartureTime,
            Source = s.Source,
            Destination = s.Destination
        };


        return View(vm);
    }

    // POST: Ticket/Update
    [HttpPost]
    public IActionResult Update(TicketVM vm)
    {
        if (string.IsNullOrEmpty(vm.TicketID))
        {
            return RedirectToAction("Ticket");
        }

        var s = db.Tickets.Find(vm.TicketID);
        if (s == null)
        {
            return RedirectToAction("Ticket");
        }

        if (ModelState.IsValid)
        {
            s.Type = vm.Type.Trim().ToUpper();
            s.UnitPrice = vm.UnitPrice;
            s.Stock = vm.Stock;
            s.DepartureTime = vm.DepartureTime;
            s.Source = vm.Source;
            s.Destination = vm.Destination;

            db.SaveChanges();
            TempData["Info"] = "Record updated.";
            return RedirectToAction("Ticket");
        }

        return View(vm);
    }

    // POST: Ticket/Delete
    [HttpPost]
    public IActionResult ChangeStatus(string? id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            var s = db.Tickets.Find(id);
            if (s != null)
            {
                if(s.Status == true)
                {
                    s.Status = false;
                    TempData["Info"] = "Record Deactivate.";
                }
                else
                {
                    s.Status = true;
                    TempData["Info"] = "Record Activate.";
                }
                db.SaveChanges();
            }
        }

        var referer = Request.Headers.Referer.ToString();
        return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction("Tciket");
    }
}