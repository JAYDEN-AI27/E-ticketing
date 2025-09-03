using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Demo.Models.TicketVM;

namespace Demo.Controllers;

public class ProductController : Controller
{
    private readonly DB db;
    private readonly Helper hp;

    public ProductController(DB db, Helper hp)
    {
        this.db = db;
        this.hp = hp;
    }
    public IActionResult Index()
        {
            var m = db.Tickets;
            return View(m);
        }

    public IActionResult ProductDetail(string id)
    {

        ViewBag.Cart = hp.GetCart();
        var m = db.Tickets
                .FirstOrDefault(p => p.TicketID == id);

        //if (m == null) return RedirectToAction("Order");

        return View(m);
    }

    [HttpPost]
    [Authorize(Roles = "Member")]
    public IActionResult UpdateCart(string TicketId, int quantity)
    {
        
        var cart = hp.GetCart();

        if (quantity >= 1 && quantity <= 10)
        {
            
            cart[TicketId] = quantity;
        }
        else
        {
            
            cart.Remove(TicketId);
        }

        hp.SetCart(cart);

        return Redirect(Request.Headers.Referer.ToString());
    }

    public IActionResult ShoppingCart()
    {
        
        var cart = hp.GetCart();
        var m = db.Tickets
                .Where(p => cart.Keys.Contains(p.TicketID))
                .Select(p => new CartItem
                {
                    Ticket = p,
                    Quantity = cart[p.TicketID],
                    Subtotal = p.UnitPrice * cart[p.TicketID],
                });

        //if (Request.IsAjax()) return PartialView("_ShoppingCart", m);

        return View(m);
    }

    // POST: Product/Checkout
    [Authorize(Roles = "Member")]
    [HttpPost]
    public IActionResult Checkout()
    {
        // 1. Checking (shoping cart NOT empty)
        var cart = hp.GetCart();
        if (cart.Count == 0) return RedirectToAction("ShoppingCart");

        // 2. Create [Order] (parent record)

        var order = new Order
        {
            OrderDate = DateTime.Today.ToDateOnly(),
            Paid = false,
            UserEmail = User.Identity!.Name!,
        };
        db.Orders.Add(order);

        // 3. Create [OrderLine] (child record)
        
        foreach (var (TicketId, quantity) in cart)
        {
            var p = db.Tickets.Find(TicketId);
            if (p == null) continue;

            if (p.Stock >= quantity)
            {
                p.Stock -= quantity;
            }

            order.OrderLines.Add(new()
            {
                Price = p.UnitPrice,
                Quantity = quantity,
                TicketID = TicketId,
            });
        }

        // 4. Save changes + clear shopping cart
        
        db.SaveChanges();
        hp.SetCart();

        // Continue with other processing
        // For example: payment, etc.

        
        return RedirectToAction("OrderComplete", new { id =order.OrderID });
    }

    public IActionResult OrderComplete(int id)
    {
        ViewBag.Id = id;
        return View();
    }

    // GET: Product/Order
    [Authorize(Roles = "Member")]
    public IActionResult Order()
    {
        
        var m = db.Orders
                .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Ticket)
                .Where(o => o.UserEmail == User.Identity!.Name)
                .OrderByDescending(o => o.OrderID);

        return View(m);
    }

    // GET: Product/OrderDetail
    [Authorize(Roles = "Member")]
    public IActionResult OrderDetail(int id)
    {
        // TODO
        var m = db.Orders
                .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Ticket)
                .FirstOrDefault(o => o.OrderID == id &&
                o.UserEmail == User.Identity!.Name);

        if (m == null) return RedirectToAction("Order");

        return View(m);
    }
}

 