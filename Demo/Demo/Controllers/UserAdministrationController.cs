using Microsoft.AspNetCore.Mvc;
using System.Linq;
using X.PagedList.Extensions;

namespace Demo.Controllers;

public class UserAdministrationController : Controller
{
    private readonly DB db;

    public UserAdministrationController(DB db)
    {
        this.db = db;
    }

    //Ask option
    public IActionResult askAM()
    {
        return View();
    }

    // GET: Home/Mtable
    public IActionResult MTable(string? name, string? sort, string? dir, int page = 1)
    {
        // (1) Searching ------------------------
        ViewBag.Name = name = name?.Trim() ?? "";

        var searched = db.Members.Where(s => s.Name.Contains(name));

        // (2) Sorting --------------------------
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        Func<Member, object> fn = sort switch
        {
            "Photo" => m => m.PhotoURL ?? "",
            "Id" => m => m.UserId,
            "Name" => m => m.Name,
            "Email" => m => m.Email,
            _ => m => m.Email,
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
            return PartialView("_MT", m);
        }

        return View(m);

    }

    // GET: Home/Atable
    public IActionResult ATable(string? name, string? sort, string? dir, int page = 1)
    {
        // (1) Searching ------------------------
        ViewBag.Name = name = name?.Trim() ?? "";

        var searched = db.Admins.Where(s => s.Name.Contains(name));

        // (2) Sorting --------------------------
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        Func<Admin, object> fn = sort switch
        {
            "Photo" => m => m.PhotoURL ?? "",
            "Id" => m => m.UserId,
            "Name" => m => m.Name,
            "Email" => m => m.Email,
            _ => m => m.Email,
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
            return PartialView("_AT", m);
        }

        return View(m);
    }

}
