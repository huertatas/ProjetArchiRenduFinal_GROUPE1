using Archi.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Archi.API.Models
{
    public class Pizza : ModelBase
    {
        //public int ID { get; set; }
        [Required]        
        public string Topping  { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
