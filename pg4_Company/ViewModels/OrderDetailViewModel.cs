using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pg4_Company.ViewModels
{
    //訂單詳情頁面用
    public class OrderDetailViewModel
    {
        public string OrderId { get; set; }
        public List<string> ProductNames { get; set; }
        public string Amount { get; set; }
        
        //收件人姓名
        [Required]
        public string fReceiver { get; set; }

        //收件人電話
        [Required]
        public string fPhone { get; set; }

        //收件人Email
        [Required]
        public string fEmail { get; set; }

        //收件人地址
        [Required]
        public string fAddress { get; set; }
    }
}
