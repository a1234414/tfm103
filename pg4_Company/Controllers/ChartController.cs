using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_TFM10304.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_TFM10304.Controllers
{
    //toB 端的權限 & layout設定
    [ViewLayout("_CompanyLayout")]
    [Authorize(Roles = "Company")]
    public class ChartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
