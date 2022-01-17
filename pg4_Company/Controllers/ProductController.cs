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
    //toB 端的權限 & layout設定
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
        public IActionResult Index()
        {
            return View();
        }

        
        public IActionResult MyProducts()  //商品管理
        {
            return View("Index");
        }
        public IActionResult ProductSource()            //api傳回Product值: 已上架
        {
            ClaimsPrincipal thisUser = this.User;
            var userId = thisUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<Product> data = db.Product.ToList();
            var on = data.Where(p => p.IsSold == true && p.CompanyUserId == userId);
            return Json(on);



            var query = db.Product.Select(p => new
            { ProductPic = p.ProductPic.FirstOrDefault(), p.Price, p.Id, p.Name, p.Description_S, p.StartDate, p.EndDate });
        }

        public IActionResult ProductSourceOff()          //api傳回Product值: 未上架
        {
            List<Product> data = db.Product.ToList();
            var off = data.Where(p => p.IsSold == false);
            return Json(off);
        }
        public IActionResult Create()
        {
            return View();
        }

        //新增商品, 圖片上傳
        [HttpPost]
        public IActionResult Upload(CreateProductViewModel data)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return Ok($"發生錯誤: {errors}");
            }

            //抓目前登入的userId
            ClaimsPrincipal thisUser = this.User;
            var userId = thisUser.FindFirst(ClaimTypes.NameIdentifier).Value;

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

        public IActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SetPdSession([FromForm] int id)    //商品管理按下編輯 傳回該ProductId 並存到Session
        {
            HttpContext.Session.SetInt32("ProductId", id);      //(key , value)
            return Json(id);
            //  return View("Edit", id) ;
        }
        public IActionResult GetPdSession()                                   //API      Get Session的ProductId傳出去
        {
            List<Product> pd = db.Product.ToList();
            var id = HttpContext.Session.GetInt32("ProductId");
            var data = (pd.Where(x => x.Id == id));
            return Json(data);
        }

        public IActionResult GetPic()
        {
            List<ProductPic> pic = db.ProductPic.ToList();
            var id = HttpContext.Session.GetInt32("ProductId");

            var data = (pic.Where(pic => pic.ProductId == id));
            return Json(data);
        }

        [HttpPost]
        public IActionResult DeletePic([FromForm] int id)
        {
            List<ProductPic> pic = db.ProductPic.ToList();
            var pp = pic.FirstOrDefault(p => p.Id == id);
            db.ProductPic.Remove(pp);
            db.SaveChanges();
            return Json(pp);
        }

        public IActionResult EditUpdate(EditProductViewModel data)  //
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return Ok($"發生錯誤: {errors}");
            }

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

        public IActionResult Detail()
        {
            return View();
        }
        [HttpPost]
        public IActionResult DeletePd([FromForm] int id)
        {
            List<Product> pd = db.Product.ToList();
            var p = pd.FirstOrDefault(p => p.Id == id);

            p.IsSold = false;
            db.SaveChanges();

            return Ok();
        }
        public IActionResult OnPd(int id)
        {
            List<Product> pd = db.Product.ToList();
            var p = pd.FirstOrDefault(p => p.Id == id);

            p.IsSold = true;
            db.SaveChanges();

            return Ok();
        }
    }
}
