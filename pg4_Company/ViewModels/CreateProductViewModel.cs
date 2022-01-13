using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pg4_Company.ViewModels
{
    public class CreateProductViewModel
    {
        [Required]
        public string Name { set; get; }
        [Required]
        public int Price { set; get; }
        public bool IsSold { set; get; }
        [Required]
        public string Description_S { set; get; }
        public string Description_L { set; get; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public List<IFormFile> Pic { set; get; }
    }
}
