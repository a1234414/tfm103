using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pg4_Company.ViewModels;
using Project_TFM10304.Attributes;
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
    //權限 & layout設定
    [ViewLayout("_CompanyLayout")]
    [Authorize(Roles = "Company")]
    public class ProductController : Controller
    {
        private readonly IWebHostEnvironment env;
        private readonly ApplicationDbContext db;
        private readonly string fileroot = @"\pic\product\";
        public ProductController(IWebHostEnvironment env, ApplicationDbContext db)
        {
            this.env = env;
            this.db = db;
        }

        //商品首頁, 顯示目前使用者的商品
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult MyProducts()  //商品管理
        {
            return View("Index");
        }
        public IActionResult ProductSource() //api傳回Product值: 已上架
        {
            ClaimsPrincipal thisUser = this.User;
            var userId = thisUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            var query = db.Product.Where(p=>p.IsSold == true && p.CompanyUserId == userId).Select(p => new
            { ProductPic = p.ProductPic.FirstOrDefault(), p.Price, p.Id, p.Name, p.StartDate, p.EndDate, p.IsSold });

            return Json(query);
        }

        public IActionResult ProductSourceOff() //api傳回Product值: 未上架
        {
            ClaimsPrincipal thisUser = this.User;
            var userId = thisUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            var query = db.Product.Where(p=>p.IsSold == false && p.CompanyUserId == userId).Select(p => new
            { ProductPic = p.ProductPic.FirstOrDefault(), p.Price, p.Id, p.Name, p.StartDate, p.EndDate, p.IsSold });

            return Json(query);
        }

        //於Index下架商品
        [HttpPost]
        public IActionResult DeletePd([FromForm] int id)
        {
            List<Product> pd = db.Product.ToList();
            var p = pd.FirstOrDefault(p => p.Id == id);

            p.IsSold = false;
            db.SaveChanges();

            return Ok();
        }

        //於Index上架商品
        public IActionResult OnPd(int id)
        {
            List<Product> pd = db.Product.ToList();
            var p = pd.FirstOrDefault(p => p.Id == id);

            p.IsSold = true;
            db.SaveChanges();

            return Ok();
        }

        //新增商品
        public IActionResult Create()
        {
            return View();
        }

        //接收新增商品表單, 處理圖片上傳
        [HttpPost]
        public IActionResult Upload(CreateProductViewModel data)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return Ok($"發生錯誤: {errors}");
            }

            ClaimsPrincipal thisUser = this.User;
            var userId = thisUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            //新增至Product Table
            Product p = new Product();
            p.CompanyUserId = userId;
            p.Name = data.Name;
            p.Price = data.Price;
            p.Description_S = data.Description_S;
            p.Description_L = data.Description_L;
            p.StartDate = data.StartDate;
            p.EndDate = data.EndDate;
            p.IsSold = data.IsSold;
            db.Product.Add(p);
            db.SaveChanges();

            var basePath = $@"{env.WebRootPath}";

            if (data.Pic != null)  //有傳圖片
            {
                //逐筆新增ProductPic
                foreach (var f in data.Pic)
                {
                    var combinFileName = $@"{fileroot}{p.Id}_{f.FileName}"; //宣告每次檔案路徑+名稱
                    using (var fileSteam = System.IO.File.Create($@"{basePath}{combinFileName}"))
                    {
                        f.CopyTo(fileSteam);
                    }

                    db.ProductPic.Add(new ProductPic
                    {
                        ProductId = p.Id,
                        PicPath = $"/pic/product/{p.Id}_{f.FileName}"
                    });
                    db.SaveChanges();
                }
            }
            return RedirectToAction("MyProducts");
        }//upload

        //編輯商品
        public IActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SetPdSession([FromForm] int id)    //於Index按下編輯 > 傳回該ProductId 並存到Session
        {
            HttpContext.Session.SetInt32("ProductId", id);      //(key , value)
            return Json(id);
        }

        //Edit 取得初始資料
        public IActionResult GetPdSession()                                   //API      Get Session的ProductId傳出去
        {
            List<Product> pd = db.Product.ToList();
            var id = HttpContext.Session.GetInt32("ProductId");
            var data = (pd.Where(x => x.Id == id));
            return Json(data);
        }

        //Edit 取得初始資料 商品圖片
        public IActionResult GetPic()
        {
            List<ProductPic> pic = db.ProductPic.ToList();
            var id = HttpContext.Session.GetInt32("ProductId");

            var data = (pic.Where(pic => pic.ProductId == id));
            return Json(data);
        }

        //刪除圖片
        [HttpPost]
        public IActionResult DeletePic([FromForm] int id)
        {
            List<ProductPic> pic = db.ProductPic.ToList();
            var pp = pic.FirstOrDefault(p => p.Id == id);
            db.ProductPic.Remove(pp);
            db.SaveChanges();
            return Json(pp);
        }

        //接收Edit送出的表單
        public IActionResult EditUpdate(EditProductViewModel data)  //
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return Ok($"發生錯誤: {errors}");
            }

            //鎖定欲編輯的product, 更新相關資料
            var id = HttpContext.Session.GetInt32("ProductId");
            var PdBd = db.Product.FirstOrDefault(x => x.Id == id);

            if (id != null)
            {
                PdBd.Name = data.Name;
                PdBd.Price = data.Price;
                PdBd.Description_S = data.Description_S;
                PdBd.Description_L = data.Description_L;
                PdBd.StartDate = data.StartDate;
                PdBd.EndDate = data.EndDate;
                PdBd.IsSold = data.IsSold;

                var basePath = $@"{env.WebRootPath}";

                if (data.Pic != null)  //有傳圖片
                {
                    foreach (var f in data.Pic)
                    {
                        var combinFileName = $@"{fileroot}{id}_{f.FileName}"; //宣告每次檔案路徑+名稱
                        using (var fileSteam = System.IO.File.Create($@"{basePath}{combinFileName}"))
                        {
                            f.CopyTo(fileSteam);
                        }

                        db.ProductPic.Add(new ProductPic
                        {
                            ProductId = PdBd.Id,
                            PicPath = $"/pic/product/{id}_{f.FileName}"
                        });
                        db.SaveChanges();
                    }
                }
                db.SaveChanges();
            }
            return RedirectToAction("MyProducts");
        }

        //商品詳情
        public IActionResult Detail()
        {
            return View();
        }
    }
}
