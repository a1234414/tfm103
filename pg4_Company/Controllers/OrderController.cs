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
                        psdate = r.psdate.ToString("yyyyMMdd"),
                        pedate = r.pedate.ToString("yyyyMMdd"),
                        totalPrice = r.price * r.quantity
                    });

                var jsonResult = JsonSerializer.Serialize(ordersGroups);
                return jsonResult;
            }
            return "";
        }
    }
}
