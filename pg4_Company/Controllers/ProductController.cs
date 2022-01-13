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

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult ProductSource()            //api傳回Product值
        {
            List<Product> data = db.Product.ToList();
            return Json(data);
        }

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
        public IActionResult EditSource([FromForm] int id)    //商品管理按下編輯 傳回該ProductId 並存到Session
        {
            HttpContext.Session.SetInt32("ProductId", id);      //(key , value)
            return Json(id);
            //  return View("Edit", id) ;
        }
        public IActionResult EditSession()                                   //API      Get Session的ProductId傳出去
        {
            List<Product> pd = db.Product.ToList();
            var id = HttpContext.Session.GetInt32("ProductId");
            var data = (pd.Where(x => x.Id == id));
            return Json(data);
        }

        public IActionResult EditUpdate(EditProductViewModel data)  //
        {
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

                db.SaveChanges();
            }

            return RedirectToAction("MyProducts");
        }

        public IActionResult Detail()
        {
            return View();
        }


        /// ///////////////////////////////////////////////////////////////////////////////////////
        public IActionResult GetEdit()
        {
            var editList = HttpContext.Session.GetString("ProductId");
            List<Product> pd = db.Product.ToList();
            if (string.IsNullOrEmpty(editList))
            {
                return Json(editList);
            }
            var data = JsonSerializer.Deserialize<List<int>>(editList);
            return Json(pd.Where(x => data.Contains(x.Id)).ToList());                         //Contains (xx)  List包含xx的資料
        }





        [HttpPost]
        public bool EditProduct([FromForm] int id)    //[FromForm] 透過 HTTP POST 的 form 取值
        {
            var editList = HttpContext.Session.GetString("ProductId");
            if (string.IsNullOrEmpty(editList))
            {
                var data = new List<int>();
                data.Add(id);
                var json = JsonSerializer.Serialize(data);
                HttpContext.Session.SetString("ProductId", json);    //(key , value)
            }
            else
            {
                var data = JsonSerializer.Deserialize<List<int>>(editList);
                data.Add(id);
                var json = JsonSerializer.Serialize(data);
                HttpContext.Session.SetString("ProductId", json); //(key , value)
            }
            return true;
        }
    }
}
