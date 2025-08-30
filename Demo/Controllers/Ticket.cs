//using Azure;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using X.PagedList.Extensions;



//namespace Demo.Controllers;

//public class TicketController : Controller
//{
//    private readonly DB db;
//    private readonly IWebHostEnvironment en;

//    public TicketController(DB db, IWebHostEnvironment en)
//    {
//        this.db = db;
//        this.en = en;
//    }

//    // 保持使用 Index2
//    public IActionResult Index2(string? type, string? sort, string? dir, int page = 1)
//    {
//        {
//        // (1) Searching ------------------------
//        ViewBag.Type = type = type?.Trim() ?? "";

//        var searched = db.Tickets.Where(s => s.Type.Contains(type));

//        // (2) Sorting --------------------------
//        ViewBag.Sort = sort;
//        ViewBag.Dir = dir;

//        Func<Ticket, object> fn = sort switch
//        {
//            "TicketID" => s => s.TicketID,
//            "Type" => s => s.Type,
//            "Unit Price" => s => s.UnitPrice,
//            "Stock" => s => s.Stock,
//            "DepartureTime" => s => s.DepartureTime,
//            "Source" => s => s.Source,
//            "Destination" => s => s.Destination,
//            _ => s => s.TicketID,
//        };

//        var sorted = dir == "des" ?
//                     searched.OrderByDescending(fn) :
//                     searched.OrderBy(fn);

//        // (3) Paging ---------------------------
//        if (page < 1)
//        {
//            return RedirectToAction(null, new { type, sort, dir, page = 1 });
//        }

//        var m = sorted.ToPagedList(page, 10);

//        if (page > m.PageCount && m.PageCount > 0)
//        {
//            return RedirectToAction(null, new { type, sort, dir, page = m.PageCount });
//        }

//        if (Request.IsAjax())
//        {
//            return PartialView("_D", m);
//        }

//        return View(m);
//    }
//    }

//    // GET: Ticket/Detail
//    public IActionResult Detail(string? id)
//    {
//        if (string.IsNullOrEmpty(id))
//        {
//            return RedirectToAction("Index2");
//        }

//        var model = db.Tickets.Find(id);
//        if (model == null)
//        {
//            return RedirectToAction("Index2");
//        }
//        return View(model);
//    }

//    // GET: Ticket/CheckId 
//    public bool CheckId(string id)
//    {
//        if (string.IsNullOrEmpty(id))
//            return false;

//        return !db.Tickets.Any(s => s.TicketID == id);
//    }

//    // GET: Ticket/Insert
//    public IActionResult Insert()
//    {
//        return View();
//    }

//    // POST: Ticket/Insert
//    [HttpPost]
//    public IActionResult Insert(TicketVM vm)
//    {
//        // 检查 TicketID 重复
//        if (!string.IsNullOrEmpty(vm.TicketID) &&
//            db.Tickets.Any(s => s.TicketID == vm.TicketID.Trim().ToUpper()))
//        {
//            ModelState.AddModelError("TicketID", "Duplicated Ticket ID.");
//        }

//        if (ModelState.IsValid)
//        {
//            var ticket = new Ticket() // 假设您的实体类叫 Ticket
//            {
//                TicketID = vm.TicketID.Trim().ToUpper(),
//                Type = vm.Type.Trim().ToUpper(),
//                UnitPrice = vm.UnitPrice,
//                Stock = vm.Stock,
//                DepartureTime = vm.DepartureTime,
//                Source = vm.Source,
//                Destination = vm.Destination,
//            };

//            db.Tickets.Add(ticket);
//            db.SaveChanges();
//            TempData["Info"] = "Record inserted.";
//            return RedirectToAction("Index2");
//        }

//        return View(vm);
//    }

//    // GET: Ticket/Update
//    public IActionResult Update(string? id)
//    {
//        if (string.IsNullOrEmpty(id))
//        {
//            return RedirectToAction("Index2");
//        }

//        var s = db.Tickets.Find(id);
//        if (s == null)
//        {
//            return RedirectToAction("Index2");
//        }

//        var vm = new TicketVM
//        {
//            TicketID = s.TicketID,
//            Type = s.Type,
//            UnitPrice = s.UnitPrice,
//            Stock = s.Stock,
//            DepartureTime = s.DepartureTime,
//            Source = s.Source,
//            Destination = s.Destination
//        };

//        // 删除了不必要的 ViewBag.ProgramList
//        return View(vm);
//    }

//    // POST: Ticket/Update
//    [HttpPost]
//    public IActionResult Update(TicketVM vm)
//    {
//        if (string.IsNullOrEmpty(vm.TicketID))
//        {
//            return RedirectToAction("Index2");
//        }

//        var s = db.Tickets.Find(vm.TicketID);
//        if (s == null)
//        {
//            return RedirectToAction("Index2");
//        }

//        if (ModelState.IsValid)
//        {
//            s.Type = vm.Type.Trim().ToUpper();
//            s.UnitPrice = vm.UnitPrice;
//            s.Stock = vm.Stock;
//            s.DepartureTime = vm.DepartureTime;
//            s.Source = vm.Source;
//            s.Destination = vm.Destination;

//            db.SaveChanges();
//            TempData["Info"] = "Record updated.";
//            return RedirectToAction("Index2");
//        }

//        return View(vm);
//    }

//    // POST: Ticket/Delete
//    [HttpPost]
//    public IActionResult Delete(string? id)
//    {
//        if (!string.IsNullOrEmpty(id))
//        {
//            var s = db.Tickets.Find(id);
//            if (s != null)
//            {
//                db.Tickets.Remove(s);
//                db.SaveChanges();
//                TempData["Info"] = "Record deleted.";
//            }
//        }

//        // 改进：如果没有 Referer，回到 Index2
//        var referer = Request.Headers.Referer.ToString();
//        return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction("Index2");
//    }
//}