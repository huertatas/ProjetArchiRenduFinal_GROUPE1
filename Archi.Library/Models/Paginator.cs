using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Archi.API.Model
{
    public class Paginator
    {
        public int range { get; set; }
        public int current_page { get; set; }

        public Paginator()
        {
            this.range = 2;
            this.current_page = 1;
        }

        public Paginator(int range, int current_page)
        {
            this.range = range > 5 ? 5 : range;
            this.current_page = current_page < 1 ? 1 : current_page;
        }
    }
}