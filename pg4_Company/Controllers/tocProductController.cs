﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_TFM10304.Data;
using Project_TFM10304.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace pg4_Company.Controllers
{
    public class tocProductController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        public tocProductController(ApplicationDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public IActionResult Index()
        {
            return View();
        }

        //to do: bug修復, 放圖片
        public string Generate()
        {
            ClaimsPrincipal thisUser = this.User;
            var userId = thisUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            if(userId != null)
            {
                Product p1 = new() { CompanyUserId = userId, Name = "商品名稱", Price = 1000, Description_S = "簡介", Description_L = "詳介", StartDate=DateTime.Now, EndDate=DateTime.Now, TotalStock=10, StockForSale=10 };
                Product p2 = new() { CompanyUserId = userId, Name = "商品名稱", Price = 1000, Description_S = "簡介", Description_L = "詳介", StartDate = DateTime.Now, EndDate = DateTime.Now, TotalStock = 10, StockForSale = 10 };
                Product p3 = new() { CompanyUserId = userId, Name = "商品名稱", Price = 1000, Description_S = "簡介", Description_L = "詳介", StartDate = DateTime.Now, EndDate = DateTime.Now, TotalStock = 10, StockForSale = 10 };

                _dbContext.Product.AddRange(p1, p2, p3);
                _dbContext.SaveChanges();
            }

            var query = _dbContext.Product.Select(p => p);
            return JsonSerializer.Serialize(query);
        }

        public string GetProducts()
        {
            var query = _dbContext.Product.Select(p => new 
            { ProductPic = p.ProductPic.FirstOrDefault(), p.Price, p.Id, p.Name, p.Description_S, p.StartDate, p.EndDate });

            return JsonSerializer.Serialize(query);
        }

        public List<Product> GetCart()
        {
            var cartList = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartList))
            {
                return new List<Product>();
            }
            
            var data = JsonSerializer.Deserialize<List<int>>(cartList);

            return _dbContext.Product.Where(p => data.Contains(p.Id)).ToList();
        }

        //加入購物車
        [HttpPost]
        public string AddProductToCart([FromForm] int id)
        {
            var cartList = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartList))
            {
                var data = new List<int>();
                data.Add(id);
                var j = JsonSerializer.Serialize(data);
                HttpContext.Session.SetString("Cart", j);
            }
            else
            {
                var data = JsonSerializer.Deserialize<List<int>>(cartList);
                data.Add(id);
                var j = JsonSerializer.Serialize(data);
                HttpContext.Session.SetString("Cart", j);
            }
            return "已加入購物車";
        }
        [HttpPost]
        public string RemoveItem([FromForm] int id)
        {
            var cartList = HttpContext.Session.GetString("Cart");
            var data = JsonSerializer.Deserialize<List<int>>(cartList);
            data.Remove(id);
            var tempdata = JsonSerializer.Serialize(data);
            HttpContext.Session.SetString("Cart", tempdata);

            return "商品已刪除";
        }
    }
}
