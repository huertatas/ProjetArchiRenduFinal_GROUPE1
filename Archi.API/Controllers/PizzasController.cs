using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Archi.API.Data;
using Archi.API.Models;
using Archi.Library.Controllers;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Archi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PizzasController : BaseController<ArchiDbContext, Pizza>
    {
        // private readonly ArchiDbContext _context;

        public PizzasController(ArchiDbContext c) : base(c)
        {

        }
                       
        
    }
}
