using Demo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.Controllers;

public class PaymentController : Controller
{
    private readonly DB db;
    private readonly Helper hp;
    private readonly IWebHostEnvironment en;
    public PaymentController(DB db, IWebHostEnvironment en, Helper hp)
    {
        this.db = db;
        this.hp = hp;
        this.en = en;
    }
    public IActionResult ChooseCard(int orderId)
    {
        var email = User.Identity?.Name;
        var m = db.Payments.Where(p => p.MemberEmail == email).ToList();

        if (m == null) return RedirectToAction("Insert", "Payment");

        var vm = new ChooseCardVM
        {
            Payments = m
        };

        ViewBag.OrderID = orderId;
        return View(vm);
    }

    public IActionResult Insert()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Insert(PaymentVM vm)
    {
        var email = User.Identity?.Name;

        if (ModelState.IsValid)
        {
            db.Payments.Add(new()
            {
                CardNum = vm.cardNum.Trim(),
                Ccv = vm.ccv.Trim(),
                Expired_month = vm.expire_month,
                Expired_year = vm.expire_year,
                MemberEmail = email

            });
            db.SaveChanges();

            TempData["Info"] = "Record inserted.";
            return RedirectToAction("Index", "Home");
        }
        return View("Insert", vm);
    }

    [Authorize(Roles = "Member")]
    [HttpPost]
    public IActionResult SelectCard(int orderId, ChooseCardVM vm)
    {
        var email = User.Identity?.Name;

        var chosenCard = db.Payments.FirstOrDefault(p =>
            p.Id == vm.SelectedPaymentId &&
            p.MemberEmail == email);

        var order = db.Orders.Find(orderId);
        if (order == null) return RedirectToAction("Index", "Home");

        order.Paid = true;
        db.SaveChanges();

        return RedirectToAction("OrderComplete", "Product", new { id = order.OrderID });
    }

    public IActionResult ShowCard()
    {
        var email = User.Identity?.Name;

        var m = db.Payments
                  .Where(p => p.MemberEmail == email)
                  .ToList();

        return View(m);
    }

    public IActionResult Update(int? id)
    {
        if (id == 0)
        {
            return RedirectToAction("Ticket");
        }

        var s = db.Payments.Find(id);
        if (s == null)
        {
            return RedirectToAction("Index");
        }

        var vm = new PaymentVM
        {
            PaymentID = s.Id,
            cardNum = s.CardNum,
            ccv = s.Ccv,
            expire_month = s.Expired_month,
            expire_year = s.Expired_year,
        };

        return View(vm);
    }

    [HttpPost]
    public IActionResult Update(PaymentVM vm)
    {
        if (vm.PaymentID == 0)
        {
            return RedirectToAction("Index");
        }

        var s = db.Payments.Find(vm.PaymentID);
        if (s == null)
        {
            return RedirectToAction("Index", "Home");
        }

        if (ModelState.IsValid)
        {
            s.CardNum = vm.cardNum.Trim();
            s.Ccv = vm.ccv.Trim();
            s.Expired_month = vm.expire_month;
            s.Expired_year = vm.expire_year;

            db.SaveChanges();
            TempData["Info"] = "Record updated.";
            return RedirectToAction("ShowCard", "Payment");
        }

        return View(vm);
    }

    [HttpPost]
    public IActionResult Delete(int? id)
    {

        var s = db.Payments.Find(id);
        if (s != null)
        {
            db.Payments.Remove(s);
            db.SaveChanges();
            TempData["Info"] = "Record deleted.";
        }

        var referer = Request.Headers.Referer.ToString();
        return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction("Index");
    }

}


