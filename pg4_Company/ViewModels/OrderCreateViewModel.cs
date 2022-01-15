using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pg4_Company.ViewModels
{
    public class OrderCreateViewModel
    {
        public List<int> pIds { get; set; }
        public List<int> qtys { get; set; }
        public string fReceiver { get; set; }
        public string fPhone { get; set; }
        public string fEmail { get; set; }
        public string fAddress { get; set; }
    }
}
