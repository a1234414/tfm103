using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using pg4_Company.Helpers;
using pg4_Company.Models;
using pg4_Company.ViewModels;
using Project_TFM10304.Data;
using Project_TFM10304.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace pg4_Company.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly BankInfoModel _bankInfoModel;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _bankInfoModel = new BankInfoModel
            {
                MerchantID = "MS129265803",
                HashKey = "1ZpJSRGcLkyAy5PGLpYXalsYKoNPcXYY",
                HashIV = "CBKeN7efuQV9Q6tP",
                ReturnURL = "http://yourWebsitUrl/Bank/SpgatewayReturn",
                NotifyURL = "http://yourWebsitUrl/Bank/SpgatewayNotify",
                CustomerURL = "http://yourWebsitUrl/Bank/SpgatewayCustomer",
                AuthUrl = "https://ccore.spgateway.com/MPG/mpg_gateway",
                CloseUrl = "https://core.newebpay.com/API/CreditCard/Close"
            };
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Orderlist()
        {
            //var amount = HttpContext.Session.GetString("Amount");
            //ViewBag.Amount = amount;
            //return View(ViewBag);
            return View();
        }

        //接cartlist
        [HttpPost]
        public void payAmount([FromForm] String amount)
        {
            HttpContext.Session.SetString("Amount", amount);
            Console.WriteLine(amount);
        }

        //接orderlist
        [HttpPost]
        //[Authorize(Roles = "Customer")]
        public IActionResult ThirdPartyPay([FromForm]OrderDetailViewModel input)
        {
            //訂單總額
            var amount = HttpContext.Session.GetString("Amount");
            var productIds = HttpContext.Session.GetString("");
            ViewBag.Amount = amount;

            OrderDetailViewModel orderDetail = new OrderDetailViewModel
            {
                Amount = HttpContext.Session.GetString("Amount"),
                OrderId = DateTime.Now.ToString("yyyyMMddhhmmss"),
                //ProductNames = 
            };

            //訂單寫進資料庫


            //建立viewmodel

            //view model丟給view()

            return View(orderDetail);
        }
        public IActionResult Cartlist()
        {
            return View();
        }

        public string CartlistContent()
        {
            //向 Session 取得商品列表
            List<Object> CartItems = SessionHelper.
                GetObjectFromJson<List<Object>>(HttpContext.Session, "Cart");

            if (CartItems == null)
            {
                List<Product> productList = new List<Product>();
                return JsonSerializer.Serialize(productList);
            }
            var result = _dbContext.Product.Where(x => CartItems.Contains(x.Id)).Select(p => new { p.Id, PicPath = p.ProductPic.FirstOrDefault().PicPath, p.Name, p.StartDate, p.Price });
            return JsonSerializer.Serialize(result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        /// <summary>
        /// [智付通支付]金流介接
        /// </summary>
        /// <param name="ordernumber">訂單單號</param>
        /// <param name="amount">訂單金額</param>
        /// <param name="payType">請款類型</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SpgatewayPayBillAsync(int amount, string payType)
        {
            string version = "1.5";

            // 目前時間轉換 +08:00, 防止傳入時間或Server時間時區不同造成錯誤
            DateTimeOffset taipeiStandardTimeOffset = DateTimeOffset.Now.ToOffset(new TimeSpan(8, 0, 0));

            TradeInfo tradeInfo = new TradeInfo()
            {
                // * 商店代號
                MerchantID = _bankInfoModel.MerchantID,
                // * 回傳格式
                RespondType = "String",
                // * TimeStamp
                TimeStamp = taipeiStandardTimeOffset.ToUnixTimeSeconds().ToString(),
                // * 串接程式版本
                Version = version,
                // * 商店訂單編號
                //MerchantOrderNo = $"T{DateTime.Now.ToString("yyyyMMddHHmm")}",
                MerchantOrderNo = $"T{DateTime.Now.ToString("yyyyMMddHHmm")}",
                // * 訂單金額
                Amt = amount,
                // * 商品資訊
                ItemDesc = "PAPAGO花藝冬令營",
                // 繳費有效期限(適用於非即時交易)
                ExpireDate = null,
                // 支付完成 返回商店網址
                ReturnURL = _bankInfoModel.ReturnURL,
                // 支付通知網址
                NotifyURL = _bankInfoModel.NotifyURL,
                // 商店取號網址
                CustomerURL = _bankInfoModel.CustomerURL,
                // 支付取消 返回商店網址
                ClientBackURL = null,
                // * 付款人電子信箱
                Email = string.Empty,
                // 付款人電子信箱 是否開放修改(1=可修改 0=不可修改)
                EmailModify = 0,
                // 商店備註
                OrderComment = null,
                // 信用卡 一次付清啟用(1=啟用、0或者未有此參數=不啟用)
                CREDIT = null,
                // WEBATM啟用(1=啟用、0或者未有此參數，即代表不開啟)
                WEBATM = null,
                // ATM 轉帳啟用(1=啟用、0或者未有此參數，即代表不開啟)
                VACC = null,
                // 超商代碼繳費啟用(1=啟用、0或者未有此參數，即代表不開啟)(當該筆訂單金額小於 30 元或超過 2 萬元時，即使此參數設定為啟用，MPG 付款頁面仍不會顯示此支付方式選項。)
                CVS = null,
                // 超商條碼繳費啟用(1=啟用、0或者未有此參數，即代表不開啟)(當該筆訂單金額小於 20 元或超過 4 萬元時，即使此參數設定為啟用，MPG 付款頁面仍不會顯示此支付方式選項。)
                BARCODE = null
            };

            if (string.Equals(payType, "CREDIT"))
            {
                tradeInfo.CREDIT = 1;
            }
            else if (string.Equals(payType, "WEBATM"))
            {
                tradeInfo.WEBATM = 1;
            }
            else if (string.Equals(payType, "VACC"))
            {
                // 設定繳費截止日期
                tradeInfo.ExpireDate = taipeiStandardTimeOffset.AddDays(1).ToString("yyyyMMdd");
                tradeInfo.VACC = 1;
            }
            else if (string.Equals(payType, "CVS"))
            {
                // 設定繳費截止日期
                tradeInfo.ExpireDate = taipeiStandardTimeOffset.AddDays(1).ToString("yyyyMMdd");
                tradeInfo.CVS = 1;
            }
            else if (string.Equals(payType, "BARCODE"))
            {
                // 設定繳費截止日期
                tradeInfo.ExpireDate = taipeiStandardTimeOffset.AddDays(1).ToString("yyyyMMdd");
                tradeInfo.BARCODE = 1;
            }

            //Atom<string> result = new Atom<string>()
            //{
            //    IsSuccess = true
            //};

            var inputModel = new SpgatewayInputModel
            {
                MerchantID = _bankInfoModel.MerchantID,
                Version = version
            };

            // 將model 轉換為List<KeyValuePair<string, string>>, null值不轉
            List<KeyValuePair<string, string>> tradeData = Pay.Util.LambdaUtil.ModelToKeyValuePairList<TradeInfo>(tradeInfo);
            // 將List<KeyValuePair<string, string>> 轉換為 key1=Value1&key2=Value2&key3=Value3...
            var tradeQueryPara = string.Join("&", tradeData.Select(x => $"{x.Key}={x.Value}"));
            // AES 加密
            inputModel.TradeInfo = Pay.Util.CryptoUtil.EncryptAESHex(tradeQueryPara, _bankInfoModel.HashKey, _bankInfoModel.HashIV);
            // SHA256 加密
            inputModel.TradeSha = Pay.Util.CryptoUtil.EncryptSHA256($"HashKey={_bankInfoModel.HashKey}&{inputModel.TradeInfo}&HashIV={_bankInfoModel.HashIV}");

            // 將model 轉換為List<KeyValuePair<string, string>>, null值不轉
            List<KeyValuePair<string, string>> postData = Pay.Util.LambdaUtil.ModelToKeyValuePairList<SpgatewayInputModel>(inputModel);
            httpContextAccessor.HttpContext.Response.Clear();

            System.Text.StringBuilder s = new StringBuilder();
            s.Append("<html>");
            s.AppendFormat("<body onload='document.forms[\"form\"].submit()'>");
            s.AppendFormat("<form name='form' action='{0}' method='post'>", _bankInfoModel.AuthUrl);
            foreach (KeyValuePair<string, string> item in postData)
            {
                s.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", item.Key, item.Value);
            }
            s.Append("</form></body></html>");
            await httpContextAccessor.HttpContext.Response.WriteAsync(s.ToString());
            await httpContextAccessor.HttpContext.Response.CompleteAsync();
            return Content(string.Empty);
        }
    }
}
