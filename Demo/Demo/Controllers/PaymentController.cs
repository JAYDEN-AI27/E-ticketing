////using Demo.Models;
////using Microsoft.AspNetCore.Authorization;
////using Microsoft.AspNetCore.Mvc;
////using Microsoft.EntityFrameworkCore;

////namespace Demo.Controllers;

////public class PaymentController : Controller
////{
////    private readonly DB db;
////    private readonly Helper hp;
////    private readonly IWebHostEnvironment en;
////    public PaymentController(DB db, IWebHostEnvironment en, Helper hp)
////    {
////        this.db = db;
////        this.hp = hp;
////        this.en = en;
////    }
////    public IActionResult ChooseCard(int orderId)
////    {
////        var email = User.Identity?.Name;
////        var m = db.Payments.Where(p => p.MemberEmail == email).ToList();

////        if (m == null) return RedirectToAction("Insert", "Payment");

////        var vm = new ChooseCardVM
////        {
////            Payments = m
////        };

////        ViewBag.OrderID = orderId;
////        return View(vm);
////    }

////    public IActionResult Insert()
////    {
////        return View();
////    }
////    [HttpPost]
////    public IActionResult Insert(PaymentVM vm)
////    {
////        var email = User.Identity?.Name;

////        if (ModelState.IsValid)
////        {
////            db.Payments.Add(new()
////            {
////                CardNum = vm.cardNum.Trim(),
////                Ccv = vm.ccv.Trim(),
////                Expired_month = vm.expire_month,
////                Expired_year = vm.expire_year,
////                MemberEmail = email

////            });
////            db.SaveChanges();

////            TempData["Info"] = "Record inserted.";
////            return RedirectToAction("Index", "Home");
////        }
////        return View("Insert", vm);
////    }

////    [Authorize(Roles = "Member")]
////    [HttpPost]
////    public IActionResult SelectCard(int orderId, ChooseCardVM vm)
////    {
////        var email = User.Identity?.Name;

////        var chosenCard = db.Payments.FirstOrDefault(p =>
////            p.Id == vm.SelectedPaymentId &&
////            p.MemberEmail == email);

////        var order = db.Orders.Find(orderId);
////        if (order == null) return RedirectToAction("Index", "Home");

////        order.Paid = true;
////        db.SaveChanges();

////        return RedirectToAction("OrderComplete", "Product", new { id = order.OrderID });
////    }

////    public IActionResult ShowCard()
////    {
////        var email = User.Identity?.Name;

////        var m = db.Payments
////                  .Where(p => p.MemberEmail == email)
////                  .ToList();

////        return View(m);
////    }

////    public IActionResult Update(int? id)
////    {
////        if (id == 0)
////        {
////            return RedirectToAction("Ticket");
////        }

////        var s = db.Payments.Find(id);
////        if (s == null)
////        {
////            return RedirectToAction("Index");
////        }

////        var vm = new PaymentVM
////        {
////            PaymentID = s.Id,
////            cardNum = s.CardNum,
////            ccv = s.Ccv,
////            expire_month = s.Expired_month,
////            expire_year = s.Expired_year,
////        };

////        return View(vm);
////    }

////    [HttpPost]
////    public IActionResult Update(PaymentVM vm)
////    {
////        if (vm.PaymentID == 0)
////        {
////            return RedirectToAction("Index");
////        }

////        var s = db.Payments.Find(vm.PaymentID);
////        if (s == null)
////        {
////            return RedirectToAction("Index", "Home");
////        }

////        if (ModelState.IsValid)
////        {
////            s.CardNum = vm.cardNum.Trim();
////            s.Ccv = vm.ccv.Trim();
////            s.Expired_month = vm.expire_month;
////            s.Expired_year = vm.expire_year;

////            db.SaveChanges();
////            TempData["Info"] = "Record updated.";
////            return RedirectToAction("ShowCard", "Payment");
////        }

////        return View(vm);
////    }

////    [HttpPost]
////    public IActionResult Delete(int? id)
////    {

////        var s = db.Payments.Find(id);
////        if (s != null)
////        {
////            db.Payments.Remove(s);
////            db.SaveChanges();
////            TempData["Info"] = "Record deleted.";
////        }

////        var referer = Request.Headers.Referer.ToString();
////        return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction("Index");
////    }

////}

////------------------------------------------------------------------------------------------------------//

//using Demo.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Text.Json.Nodes;
//using static System.Runtime.InteropServices.JavaScript.JSType;

//namespace Demo.Controllers;

//public class PaymentController : Controller
//{
//    private readonly DB db;
//    private readonly Helper hp;
//    private readonly IWebHostEnvironment en;
//    private string PaypalClientId { get; set; } = "";
//    private string PaypalSecret { get; set; } = "";
//    private string PaypalUrl { get; set; } = "";
//    public PaymentController(DB db, IWebHostEnvironment en, Helper hp, IConfiguration config)
//    {
//        this.db = db;
//        this.hp = hp;
//        this.en = en;

//        PaypalClientId = config["PaypalSettings:ClientId"]!;
//        PaypalSecret = config["PaypalSettings:Secret"]!;
//        PaypalUrl = config["PaypalSettings:Url"]!;
//    }

//    [Authorize(Roles = "Member")]
//    public IActionResult Index(int? orderId)
//    {
//        if (orderId == null)
//        {
//            return RedirectToAction("ShoppingCart", "Product");
//        }

//        // Get the order and calculate total
//        var order = db.Orders
//            .Include(o => o.OrderLines)
//            .ThenInclude(ol => ol.Ticket)
//            .FirstOrDefault(o => o.OrderID == orderId && o.UserEmail == User.Identity!.Name);

//        if (order == null || order.Paid)
//        {
//            return RedirectToAction("ShoppingCart", "Product");
//        }

//        // Calculate total amount from order lines
//        decimal totalAmount = order.OrderLines.Sum(ol => ol.Price * ol.Quantity);

//        ViewBag.PaypalClientId = PaypalClientId;
//        ViewBag.OrderId = orderId;
//        ViewBag.TotalAmount = totalAmount;
//        return View();
//    }

//    private async Task<string> GetPaypalAccessToken()
//    {
//        string accessToken = "";

//        string url = PaypalUrl + "/v1/oauth2/token";

//        using (var client = new HttpClient())
//        {

//            string credientials64 =
//                Convert.ToBase64String(Encoding.UTF8.GetBytes(PaypalClientId + ":" + PaypalSecret));

//            client.DefaultRequestHeaders.Add("Authorization", "Basic " + credientials64);

//            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
//            requestMessage.Content = new StringContent("grant_type=client_credentials", null
//                , "application/x-www-form-urlencoded");

//            var httpResponse = await client.SendAsync(requestMessage);

//            if (httpResponse.IsSuccessStatusCode)
//            {
//                var strResponse = await httpResponse.Content.ReadAsStringAsync();

//                var jsonResponse = JsonNode.Parse(strResponse);

//                if (jsonResponse != null)
//                {
//                    accessToken = jsonResponse["access_token"]?.ToString() ?? "";
//                }
//            }

//            if (!httpResponse.IsSuccessStatusCode)
//            {
//                var errorStr = await httpResponse.Content.ReadAsStringAsync();
//                Console.WriteLine("PayPal API error: " + errorStr);
//            }


//        }

//        return accessToken;
//    }
//    [HttpPost]
//    public async Task<JsonResult> CreatePayment([FromBody] JsonObject data)
//    {
//        var totalAmount = data?["amount"]?.ToString();
//        if (totalAmount == null)
//        {
//            return new JsonResult(new
//            {
//                id = ""
//            });
//        }

//        JsonObject createOrderRequest = new JsonObject();

//        createOrderRequest.Add("intent", "CAPTURE");

//        JsonObject amount = new JsonObject();

//        amount.Add("currency_code", "MYR");

//        amount.Add("value", totalAmount);

//        JsonObject purchaseUnit1 = new JsonObject();

//        purchaseUnit1.Add("amount", amount);

//        JsonArray purchaseUnits = new JsonArray();

//        purchaseUnits.Add(purchaseUnit1);

//        createOrderRequest.Add("purchase_units", purchaseUnits);

//        string accessToken = await GetPaypalAccessToken();

//        string url = PaypalUrl + "/v2/checkout/orders";


//        using (var client = new HttpClient())
//        {

//            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

//            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

//            requestMessage.Content = new StringContent(createOrderRequest.ToString(), null, "application/json");

//            var httpResponse = await client.SendAsync(requestMessage);

//            if (httpResponse.IsSuccessStatusCode)
//            {

//                var strResponse = await httpResponse.Content.ReadAsStringAsync();

//                var jsonResponse = JsonNode.Parse(strResponse);

//                if (jsonResponse != null)
//                {

//                    string paypalOrderId = jsonResponse["id"]?.ToString() ?? "";

//                    return new JsonResult(new { id = paypalOrderId });
//                }
//            }
//        }



//        return new JsonResult(new { id = "" });
//    }

//    //[HttpPost]
//    //[Authorize(Roles = "Member")]
//    //public async Task<JsonResult> CompletePaypalOrder([FromBody] JsonObject data)
//    //{
//    //    var paypalOrderId = data?["id"]?.ToString();
//    //    var email = data?["email"]?.ToString();
//    //    var orderId = data?["orderId"]?.ToString();

//    //    if (string.IsNullOrEmpty(paypalOrderId))
//    //        return new JsonResult(new { status = "error", message = "Missing PayPal orderID" });

//    //    if (string.IsNullOrEmpty(email))
//    //        return new JsonResult(new { status = "error", message = "Missing email" });

//    //    if (string.IsNullOrEmpty(orderId) || !int.TryParse(orderId, out int dbOrderId))
//    //        return new JsonResult(new { status = "error", message = "Missing or invalid order ID" });

//    //    string accessToken = await GetPaypalAccessToken();

//    //    string url = PaypalUrl + "/v2/checkout/orders/" + paypalOrderId + "/capture";

//    //    using (var client = new HttpClient())
//    //    {
//    //        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

//    //        var req = new HttpRequestMessage(HttpMethod.Post, url);
//    //        req.Content = new StringContent("", Encoding.UTF8, "application/json");

//    //        var httpResponse = await client.SendAsync(req);

//    //        var strResponse = await httpResponse.Content.ReadAsStringAsync();

//    //        if (httpResponse.IsSuccessStatusCode)
//    //        {
//    //            // Get the existing order
//    //            var order = db.Orders
//    //                .FirstOrDefault(o => o.OrderID == dbOrderId && o.UserEmail == email && !o.Paid);

//    //            var jsonResponse = JsonNode.Parse(strResponse);

//    //            // ✅ Get Capture ID
//    //            var captureId = jsonResponse?["purchase_units"]?[0]?["payments"]?["captures"]?[0]?["id"]?.ToString();

//    //            if (string.IsNullOrEmpty(captureId))
//    //            {
//    //                return new JsonResult(new { status = "error", message = "Payment capture failed" });
//    //            }


//    //            if (order == null)
//    //            {
//    //                return new JsonResult(new { status = "error", message = "Order not found or already paid" });
//    //            }

//    //            // Mark order as paid
//    //            order.Paid = true;
//    //            var payment = new Payment
//    //            {
//    //                OrderID = order.OrderID,
//    //                PaypalCaptureId = captureId
//    //            };

//    //            db.Payments.Add(payment);

//    //            // Save changes and clear cart
//    //            db.SaveChanges();

//    //            // Return success with redirect URL to Product/OrderComplete
//    //            return new JsonResult(new
//    //            {
//    //                status = "success",
//    //                redirectUrl = Url.Action("OrderComplete", "Product", new { e = email, id = order.OrderID })
//    //            });
//    //        }
//    //        else
//    //        {
//    //            return new JsonResult(new { status = "error", message = strResponse });
//    //        }
//    //    }
//    //}

//    [HttpPost]
//    [Authorize(Roles = "Member")]
//    // Remove [ValidateAntiForgeryToken] if you have it, as you're using API-style calls
//    public async Task<JsonResult> CompletePaypalOrder([FromBody] JsonObject data)
//    {
//        try
//        {
//            var paypalOrderId = data?["id"]?.ToString();
//            var email = data?["email"]?.ToString();
//            var orderId = data?["orderId"]?.ToString();

//            if (string.IsNullOrEmpty(paypalOrderId))
//                return new JsonResult(new { status = "error", message = "Missing PayPal orderID" });

//            if (string.IsNullOrEmpty(email))
//                return new JsonResult(new { status = "error", message = "Missing email" });

//            if (string.IsNullOrEmpty(orderId) || !int.TryParse(orderId, out int dbOrderId))
//                return new JsonResult(new { status = "error", message = "Missing or invalid order ID" });

//            // Verify the email matches the authenticated user
//            if (email != User.Identity?.Name)
//                return new JsonResult(new { status = "error", message = "Unauthorized" });

//            string accessToken = await GetPaypalAccessToken();

//            if (string.IsNullOrEmpty(accessToken))
//                return new JsonResult(new { status = "error", message = "Failed to get PayPal access token" });

//            string url = PaypalUrl + "/v2/checkout/orders/" + paypalOrderId + "/capture";

//            using (var client = new HttpClient())
//            {
//                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

//                var req = new HttpRequestMessage(HttpMethod.Post, url);
//                req.Content = new StringContent("", Encoding.UTF8, "application/json");

//                var httpResponse = await client.SendAsync(req);
//                var strResponse = await httpResponse.Content.ReadAsStringAsync();

//                if (httpResponse.IsSuccessStatusCode)
//                {
//                    var order = db.Orders
//                        .FirstOrDefault(o => o.OrderID == dbOrderId && o.UserEmail == email && !o.Paid);

//                    if (order == null)
//                    {
//                        return new JsonResult(new { status = "error", message = "Order not found or already paid" });
//                    }

//                    var jsonResponse = JsonNode.Parse(strResponse);
//                    var captureId = jsonResponse?["purchase_units"]?[0]?["payments"]?["captures"]?[0]?["id"]?.ToString();

//                    if (string.IsNullOrEmpty(captureId))
//                    {
//                        return new JsonResult(new { status = "error", message = "Payment capture failed" });
//                    }

//                    order.Paid = true;
//                    var payment = new Payment
//                    {
//                        OrderID = order.OrderID,
//                        PaypalCaptureId = captureId
//                    };

//                    db.Payments.Add(payment);
//                    db.SaveChanges();

//                    return new JsonResult(new
//                    {
//                        status = "success",
//                        redirectUrl = Url.Action("OrderComplete", "Product", new { e = email, id = order.OrderID })
//                    });
//                }
//                else
//                {
//                    return new JsonResult(new { status = "error", message = "PayPal capture failed: " + strResponse });
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            // Log the exception
//            Console.WriteLine($"Error in CompletePaypalOrder: {ex.Message}");
//            Console.WriteLine($"Stack trace: {ex.StackTrace}");

//            return new JsonResult(new { status = "error", message = "Internal server error: " + ex.Message });
//        }
//    }

//    [HttpPost]
//    public async Task<JsonResult> RefundPaypalOrder(int orderId)
//    {
//        var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId && o.Paid);
//        var payment = db.Payments
//                        .Include(p => p.Order)
//                        .FirstOrDefault(p => p.OrderID == orderId);


//        if (order == null || string.IsNullOrEmpty(payment.PaypalCaptureId))
//        {
//            return new JsonResult(new { status = "error", message = "Order not eligible for refund" });
//        }

//        string accessToken = await GetPaypalAccessToken();

//        string url = PaypalUrl + "/v2/payments/captures/" + payment.PaypalCaptureId + "/refund";

//        using (var client = new HttpClient())
//        {
//            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

//            var request = new HttpRequestMessage(HttpMethod.Post, url)
//            {
//                Content = new StringContent("{}", Encoding.UTF8, "application/json")
//            };

//            var response = await client.SendAsync(request);
//            var responseStr = await response.Content.ReadAsStringAsync();

//            if (response.IsSuccessStatusCode)
//            {
//                order.Paid = false;          // or add RefundStatus = true
//                db.SaveChanges();

//                return new JsonResult(new
//                {
//                    status = "success",
//                    message = "Refund completed successfully"
//                });
//            }
//            else
//            {
//                return new JsonResult(new
//                {
//                    status = "error",
//                    message = responseStr
//                });
//            }
//        }
//    }






//}




