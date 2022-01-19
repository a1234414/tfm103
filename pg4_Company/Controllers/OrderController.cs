using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project_TFM10304.Attributes;
using Project_TFM10304.Data;
using Project_TFM10304.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace Project_TFM10304.Controllers
{
    //權限 & layout設定
    [ViewLayout("_CompanyLayout")]
    [Authorize(Roles = "Company")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<Users> _userManager;
        
        public OrderController(ApplicationDbContext dbContext,
            UserManager<Users> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        //歷史訂單
        public IActionResult Index()
        {
            return View();
        }

        //歷史訂單 初始資料
        public string MyOrders()
        {
            ClaimsPrincipal thisUser = this.User;
            var IsCompany = thisUser.IsInRole("Company");

            if (IsCompany == true)
            {
                var userId = thisUser.FindFirst(ClaimTypes.NameIdentifier).Value;

                //join [order, orderDetail, Product], groupby(product.name), 將orderDetail.quantity加總
                var ordersGroups = _dbContext.Order
                    .Join(_dbContext.OrderDetail, o => o.OrderId, od => od.OrderId, (o, od) =>
                    new
                    {
                        id = o.OrderId,
                        productId = od.ProductId,
                        quantity = od.Quantity,
                    })
                    .Join(_dbContext.Product, od => od.productId, p => p.Id, (od, p) =>
                    new
                    {
                        oid = od.id,
                        productName = p.Name,
                        price = p.Price,
                        quantity = od.quantity,
                        psdate = p.StartDate,
                        pedate = p.EndDate,
                        cid = p.CompanyUserId
                    })
                    .Where(o => o.cid == userId && o.pedate <= DateTime.Now).Select(r =>
                    new {
                        oid = r.oid,
                        productName = r.productName,
                        price = r.price,
                        quantity = r.quantity,
                        psdate = r.psdate.ToString("yyyy/MM/dd"),
                        pedate = r.pedate.ToString("yyyy/MM/dd"),
                        totalPrice = r.price * r.quantity
                    });

                var jsonResult = JsonSerializer.Serialize(ordersGroups);
                return jsonResult;
            }
            return "";
        }

        public string GetOrders(string sdate, string edate)
        {
            ClaimsPrincipal thisUser = this.User;
            string userId = thisUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            DateTime dts = (sdate == null) ? DateTime.Parse("2000-01-01") : DateTime.Parse(sdate);
            DateTime dte = (edate == null) ? DateTime.Now : DateTime.Parse(edate);

            var query = _dbContext.Order.Join(_dbContext.OrderDetail, o => o.OrderId, od => od.OrderId, (o, od) => new { id = o.OrderId, pid = od.ProductId, qty = od.Quantity })
                .Join(_dbContext.Product, o => o.pid, p => p.Id, (o, p) => new { cid = p.CompanyUserId, oid = o.id, productName = p.Name, price = p.Price, quantity = o.qty, psdate = p.StartDate, pedate = p.EndDate })
                .Where(o => o.cid == userId && o.psdate >= dts && o.pedate <= dte)
                .Select(r => new { oid = r.oid, productName = r.productName, price = r.price, quantity = r.quantity, psdate = r.psdate.ToString("yyyy/MM/dd"), pedate = r.pedate.ToString("yyyy/MM/dd"), totalPrice = r.price * r.quantity });

            return JsonSerializer.Serialize(query);
        }

        //初始化Demo用的資料
        public void DemoInit()
        {
            ClaimsPrincipal thisUser = this.User;
            string userId = thisUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            Product p1 = new() { CompanyUserId = userId, Name = "第一期 | 金工體驗初階班", Price = 1000, StartDate = DateTime.Parse("2022-01-09").AddHours(13).AddMinutes(30), EndDate = DateTime.Parse("2022-01-09").AddHours(15).AddMinutes(30) };
            Product p2 = new() { CompanyUserId = userId, Name = "第二期 | 金工體驗初階班", Price = 1000, StartDate = DateTime.Parse("2022-01-16").AddHours(13).AddMinutes(30), EndDate = DateTime.Parse("2022-01-16").AddHours(15).AddMinutes(30) };
            Product p3 = new() { CompanyUserId = userId, Name = "第三期 | 金工體驗初階班", Price = 1000, StartDate = DateTime.Parse("2022-01-23").AddHours(13).AddMinutes(30), EndDate = DateTime.Parse("2022-01-23").AddHours(15).AddMinutes(30) };
            Product p4 = new() { CompanyUserId = userId, Name = "第一期 | 金工進階探索班", Price = 2999, StartDate = DateTime.Parse("2022-01-22").AddHours(13).AddMinutes(30), EndDate = DateTime.Parse("2022-01-22").AddHours(18).AddMinutes(30) };

            _dbContext.Product.AddRange(p1, p2, p3, p4);
            _dbContext.SaveChanges();

            Order o1 = new() { OrderId = "O20220105001", UserId = userId };
            Order o2 = new() { OrderId = "O20220105002", UserId = userId };

            Order o3 = new() { OrderId = "O20220113001", UserId = userId };
            Order o4 = new() { OrderId = "O20220114001", UserId = userId };

            Order o5 = new() { OrderId = "O20220120001", UserId = userId };
            Order o6 = new() { OrderId = "O20220121001", UserId = userId };

            _dbContext.Order.AddRange(o1, o2, o3, o4, o5, o6);
            _dbContext.SaveChanges();

            //p6: 9筆總售出
            OrderDetail od1 = new() { OrderId = "O20220105001", ProductId = p1.Id, Quantity = 4 };
            OrderDetail od2 = new() { OrderId = "O20220105002", ProductId = p1.Id, Quantity = 5 };

            OrderDetail od3 = new() { OrderId = "O20220113001", ProductId = p2.Id, Quantity = 6 };
            OrderDetail od4 = new() { OrderId = "O20220114001", ProductId = p2.Id, Quantity = 6 };

            OrderDetail od5 = new() { OrderId = "O20220120001", ProductId = p3.Id, Quantity = 6 };
            OrderDetail od6 = new() { OrderId = "O20220114001", ProductId = p4.Id, Quantity = 6 };

            _dbContext.AddRange(od1, od2, od3, od4, od5, od6);
            _dbContext.SaveChanges();
        }
    }
}
