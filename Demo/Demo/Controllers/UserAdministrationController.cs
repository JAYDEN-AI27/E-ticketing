using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using X.PagedList.Extensions;

namespace Demo.Controllers;

public class UserAdministrationController : Controller
{
    private readonly DB db;
    private readonly Helper hp;

    public UserAdministrationController(DB db, Helper hp)
    {
        this.db = db;
        this.hp = hp;
    }

    //Ask option
    [Authorize(Roles ="Admin")]
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

    public IActionResult Detail(string email)
    {
        var admin = db.Admins.FirstOrDefault(a => a.Email == email);
        if (admin != null)
            return View("Detail", admin);

        var member = db.Members.FirstOrDefault(m => m.Email == email);
        if (member != null)
            return View("Detail", member);

        return NotFound();
    }

    public bool CheckMemberEmail(string email)
    {
        // TODO
        return !db.Members.Any(m => m.Email == email);
    }

    // GET: Home/Insert Member
    public IActionResult InsertMember()
    {
        return View();
    }

    // POST: Home/Insert Member
    [HttpPost]
    public IActionResult InsertMember(RegisterVM vm)
    {
        // TODO
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (db.Members.Any(m => m.Email == vm.Email)) 
        {
            ModelState.AddModelError("Email", "Duplicated Email");
        }

        if (ModelState.IsValid)
        {
            db.Members.Add(new()
            {
                Email = vm.Email,
                Name = vm.Name.Trim(),
                Hash = hp.HashPassword(vm.Password),
                PhotoURL = hp.SavePhoto(vm.Photo, "photos"),
            });
            db.SaveChanges();

            TempData["Info"] = "Record inserted.";
            return RedirectToAction("MTable");
        }

        return View(vm);
    }
    // GET: Home/Insert Admin
    public IActionResult InsertAdmin()
    {
        return View();
    }

    // POST: Home/Insert Admin
    [HttpPost]
    public IActionResult InsertAdmin(RegisterVM vm)
    {
        // TODO
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (db.Admins.Any(a => a.Email == vm.Email))
        {
            ModelState.AddModelError("Email", "Duplicated Email");
        }

        if (ModelState.IsValid)
        {
            db.Admins.Add(new()
            {
                Email = vm.Email,
                Name = vm.Name.Trim(),
                Hash = hp.HashPassword(vm.Password),
                PhotoURL = hp.SavePhoto(vm.Photo, "photos"),
            });
            db.SaveChanges();

            TempData["Info"] = "Record inserted.";
            return RedirectToAction("ATable");
        }

        return View(vm);
    }
    // GET: Home/Update Member
    public IActionResult UpdateMember(string? email)
    {
        var m = db.Members.Find(email);

        if (m == null)
        {
            return RedirectToAction("MTable");
        }

        // TODO
        var vm = new UpdateProfileVM
        {
            Name = m.Name,
            PhotoURL = m.PhotoURL,
            Photo = null,
        };

        return View(vm); // TODO
    }

    // POST: Home/Update Member
    [HttpPost]
    public IActionResult UpdateMember(UpdateProfileVM vm)
    {
        var m = db.Members.Find(vm.Email);

        if (m == null)
        {
            return RedirectToAction("MTable");
        }

        if (ModelState.IsValid)
        {
            m.Name = vm.Name.Trim();
            if (vm.Photo != null)

            {
                var error = hp.ValidatePhoto(vm.Photo);
                if (!string.IsNullOrEmpty(error))
                {
                    ModelState.AddModelError("Photo", error);
                    return View(vm);
                }

                hp.DeletePhoto(m.PhotoURL, "photos");

                var fileName = hp.SavePhoto(vm.Photo, "photos");
                m.PhotoURL = fileName;
            }

            db.SaveChanges();

            TempData["Info"] = "Record updated.";
            return RedirectToAction("MTable");
        }

        return View(vm);
    }

}
