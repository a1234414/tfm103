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
    //toB 端的權限 & layout設定
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

        public IActionResult Index()
        {
            return View();
        }

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
                        odate = o.Date,
                        productId = od.ProductId,
                        quantity = od.Quantity,
                    })
                    .Join(_dbContext.Product, od => od.productId, p => p.Id, (od, p) =>
                    new
                    {
                        oid = od.id,
                        odate = od.odate,
                        productName = p.Name,
                        price = p.Price,
                        quantity = od.quantity,
                        psdate = p.StartDate,
                        pedate = p.EndDate,
                        cid = p.CompanyUserId
                    })
                    .Where(o => o.cid == userId).Select(r =>
                    new {
                        oid = r.oid,
                        odate = r.odate,
                        productName = r.productName,
                        price = r.price,
                        quantity = r.quantity,
                        psdate = r.psdate,
                        pedate = r.pedate,
                        totalPrice = r.price * r.quantity
                    });

                //to do: datetime格式整理
                //foreach(var item in ordersGroups)
                //{
                //    DateTime dt = item.date;
                //    item.date = String.Format("0:yyyy/MM/dd/HH/mm", dt);
                //}

                //to do: 互動式日期篩選 x日內到來
                //.Where(o => (DateTime.Now - o.date).TotalDays <= 10);

                var jsonResult = JsonSerializer.Serialize(ordersGroups);
                return jsonResult;
            }
            return "";
        }

        //public void Init()
        //{
        //    ClaimsPrincipal thisUser = this.User;
        //    string userId = thisUser.FindFirst(ClaimTypes.NameIdentifier).Value;

        //    Order o1 = new Order
        //    {
        //        UserId = userId,
        //        Date = DateTime.Now
        //    };
        //    Order o2 = new Order
        //    {
        //        UserId = userId,
        //        Date = DateTime.Now
        //    };
        //    Order o3 = new Order
        //    {
        //        UserId = userId,
        //        Date = DateTime.Now
        //    };
        //    _dbContext.Order.AddRange(o1, o2, o3);
        //    _dbContext.SaveChanges();

        //    OrderDetail od1 = new OrderDetail
        //    {
        //        OrderId = o1.OrderId,
        //        ProductId = 1,
        //        Quantity = 4,
        //        IsPaid = true
        //    };
        //    OrderDetail od2 = new OrderDetail
        //    {
        //        OrderId = o2.OrderId,
        //        ProductId = 2,
        //        Quantity = 2,
        //        IsPaid = true
        //    };
        //    OrderDetail od3 = new OrderDetail
        //    {
        //        OrderId = o3.OrderId,
        //        ProductId = 3,
        //        Quantity = 2,
        //        IsPaid = true
        //    };

        //    _dbContext.OrderDetail.AddRange(od1, od2, od3);
        //    _dbContext.SaveChanges();
        //}

        //public void Dg2()
        //{
        //    Product p1 = new Product
        //    {
        //        Name = "金工初階班",
        //        Price = 1000,
        //        StartDate = DateTime.Now.AddDays(-2),
        //        EndDate = DateTime.Now.AddDays(-2),
        //        CompanyUserId = "fdf8443c-e0c9-4b7d-a6a1-b8222b7e0242"
        //    };
        //    Product p2 = new Product
        //    {
        //        Name = "金工進階班",
        //        Price = 2000,
        //        StartDate = DateTime.Now.AddDays(-6),
        //        EndDate = DateTime.Now.AddDays(-6),
        //        CompanyUserId = "fdf8443c-e0c9-4b7d-a6a1-b8222b7e0242"
        //    };

        //    Order o1 = new Order
        //    {
        //        UserId = "0f75dd28-10b0-48e7-b7da-e0b110f9ee41",
        //        Date = DateTime.Now
        //    };
        //    Order o2 = new Order
        //    {
        //        UserId = "0f75dd28-10b0-48e7-b7da-e0b110f9ee41",
        //        Date = DateTime.Now
        //    };

        //    _dbContext.Product.AddRange(p1, p2);
        //    _dbContext.Order.AddRange(o1, o2);
        //    _dbContext.SaveChanges();

        //    OrderDetail od1 = new OrderDetail
        //    {
        //        OrderId = o1.OrderId,
        //        ProductId = p1.Id,
        //        Quantity = 4,
        //        IsPaid = true
        //    };
        //    OrderDetail od2 = new OrderDetail
        //    {
        //        OrderId = o2.OrderId,
        //        ProductId = p2.Id,
        //        Quantity = 2,
        //        IsPaid = true
        //    };

        //    _dbContext.OrderDetail.AddRange(od1, od2);
        //    _dbContext.SaveChanges();
        //}
    }
}
