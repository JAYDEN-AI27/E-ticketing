using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using X.PagedList.Extensions;

namespace Demo.Controllers;

public class UserAdministrationController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;

    public UserAdministrationController(DB db, Helper helper, IWebHostEnvironment en)
    {
        this.db = db;
        this.hp = helper;
        this.en = en;
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

    //public IActionResult Detail(string email)
    //{
    //    var admin = db.Admins.FirstOrDefault(a => a.Email == email);
    //    if (admin != null)
    //        return View("Detail", admin);

    //    var member = db.Members.FirstOrDefault(m => m.Email == email);
    //    if (member != null)
    //        return View("Detail", member);

    //    return NotFound();
    //}
    //public IActionResult Update(string email)
    //{
    //    var admin = db.Admins.FirstOrDefault(a => a.Email == email);
    //    if (admin != null)
    //        return View("Update", admin);

    //    var member = db.Members.FirstOrDefault(m => m.Email == email);
    //    if (member != null)
    //        return View("Update", member);

    //    return NotFound();
    //}

    // GET: Member/Detail
    // GET: Home/Detail
    public IActionResult MemberDetail(string? email)
    {
        var model = db.Members.Find(email);

        if (model == null)
        {
            return RedirectToAction("Index");
        }

        return View(model);
    }

    // GET: Home/CheckId 
    public bool CheckEmail(string email)
    {
        // TODO
        return !db.Users.Any(u => u.Email == email);
    }

    //// GET: Home/CheckProgramId
    //public bool CheckProgramId(string programId)
    //{
    //    // TODO
    //    return db.Programs.Any(p => p.Id == programId);
    //}

    // GET: Home/Insert
    public IActionResult MemberInsert()
    {
        return View();
    }

    // POST: Home/Insert
    [HttpPost]
    public IActionResult MemberInsert(MembersVM vm)
    {

        if (db.Users.Any(u => u.Email == vm.Email))
        {
            ModelState.AddModelError("Email", "Duplicated Email.");
        }

        if (ModelState.IsValid)
        {
            var member = new Member()
            { 
               Email = vm.Email,
               Hash = hp.HashPassword(vm.Password),
               Name = vm.Name,
               PhotoURL = hp.SavePhoto(vm.ProfilePhoto, "photos"),
            };

            db.Members.Add(member);

            
            
            db.SaveChanges();
            TempData["Info"] = "Insert Recorded.";
            return RedirectToAction("Member");
        }

        return View(vm);
    }

    //// POST: Home/Update
    //[HttpPost]
    //public IActionResult Update(StudentVM vm)
    //{
    //    var s = db.Students.Find(vm.Id);

    //    if (s == null)
    //    {
    //        return RedirectToAction("Index");
    //    }

    //    if (ModelState.IsValid("ProgramId") &&
    //        !db.Programs.Any(p => p.Id == vm.ProgramId))
    //    {
    //        ModelState.AddModelError("ProgramId", "Invalid Program.");
    //    }

    //    if (ModelState.IsValid)
    //    {
    //        s.Name = vm.Name.Trim().ToUpper();
    //        s.Gender = vm.Gender;
    //        s.ProgramId = vm.ProgramId;
    //        db.SaveChanges();

    //        TempData["Info"] = "Record updated.";
    //        return RedirectToAction("Index");
    //    }

    //    ViewBag.ProgramList = new SelectList(db.Programs, "Id", "Name");
    //    return View(vm);
    //}

    //// POST: Home/Delete
    //[HttpPost]
    //public IActionResult Delete(string? id)
    //{
    //    var s = db.Students.Find(id);

    //    if (s != null)
    //    {
    //        // TODO
    //        db.Students.Remove(s);
    //        db.SaveChanges();

    //        TempData["Info"] = "Record deleted.";
    //    }

    //    return RedirectToAction(Request.Headers.Referer.ToString()); // TODO
    //}

    //// GET: Home/Demo
    //public IActionResult Demo()
    //{
    //    var model = db.Students;
    //    return View(model);
    //}

    //// POST: Home/DeleteMany
    //[HttpPost]
    //public IActionResult DeleteMany(string[] ids)
    //{
    //    // TODO
    //    int n = db.Students
    //        .Where(s => ids.Contains(s.Id))
    //        .ExecuteDelete();

    //    TempData["Info"] = $"{n} record(s) deleted.";
    //    return RedirectToAction("Demo");
    //}

    //// POST: Home/Restore
    //[HttpPost]
    //public IActionResult Restore()
    //{
    //    // (1) Delete all records
    //    db.Students.ExecuteDelete();

    //    // ------------------------------------------------

    //    // (2) Insert all records from "Students.txt"
    //    string path = Path.Combine(en.ContentRootPath, "Students.txt");

    //    foreach (string line in System.IO.File.ReadLines(path))
    //    {
    //        if (line.Trim() == "") continue;

    //        var data = line.Split("\t", StringSplitOptions.TrimEntries);

    //        db.Students.Add(new()
    //        {
    //            Id = data[0],
    //            Name = data[1],
    //            Gender = data[2],
    //            ProgramId = data[3],
    //        });
    //    }

    //    db.SaveChanges();

    //    TempData["Info"] = "Record(s) restored.";
    //    return RedirectToAction("Demo");
    //}



}
