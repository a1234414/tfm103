using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Project_TFM10304.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public Users User { get; set; }
        public DateTime Date { get; set; }
        public ICollection<OrderDetail> OrderDetail { get; set; }
    }
}
