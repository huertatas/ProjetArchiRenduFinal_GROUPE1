using Archi.API.Model;
using Archi.Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Archi.Library.Controllers
{    

    public abstract class BaseController<TContext, TModel> : ControllerBase where TContext : DbContext where TModel : ModelBase
    {
        protected readonly TContext _context;

        public BaseController(TContext context)
        {
            _context = context;
        }

        // GET: api/{model}
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TModel>>> GetAll([FromQuery] Paginator filter)
        {
            var paginator = new Paginator(filter.range, filter.current_page);
            var source = _context.Set<TModel>();
            var result1 = source.Skip((paginator.current_page - 1) * paginator.range)
            .Take(paginator.range);

            return await result1.Where(x => x.Active == true).ToListAsync();
            //return await _context.Pizzas.Where(x => x.Active == true).ToListAsync();
        }

        // GET: api/Pizzas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TModel>> GetModel(int id)
        {
            var model = await _context.Set<TModel>().FindAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            return model;
        }


        //PUT: api/{model}/5
        [HttpPut("{id}")]
        public async Task<ActionResult<TModel>> PutPizza(int id, TModel model)
        {


            if (!_context.Set<TModel>().Where(x => x.ID == id).Any())
            {
                return BadRequest();
            }

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Set<TModel>().Any(e => e.ID == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/{model}
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TModel>> PostPizza(TModel model)
        {
            _context.Set<TModel>().Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPizza", new { id = model.ID }, model);
        }

        // DELETE: api/Pizzas/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TModel>> DeletePizza(int id)
        {
            var pizza = await _context.Set<TModel>().FindAsync(id);
            if (pizza == null)
            {
                return NotFound();
            }

            _context.Set<TModel>().Remove(pizza);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // FILTER: api/filter/{model}
        [HttpGet("/filter")]
        public async Task<ActionResult<IEnumerable<TModel>>> GetFilter([FromQuery(Name = "typeGeneric")] string typeGeneric, [FromQuery(Name = "type")] string type, [FromQuery(Name = "numGeneric")] string numGeneric, [FromQuery(Name = "price")] string price, [FromQuery(Name = "dateGeneric")] string dateGeneric, [FromQuery(Name = "createdat")] string createdat)
        {
            // init
            var vide = _context.Set<TModel>().Where(x => x.ID == -51);
            var query = _context.Set<TModel>().Where(x => x.Active == true);

            // partie filtrage
            if (!String.IsNullOrEmpty(type))
            {              
                string queryType = type;
                string[] words = queryType.Split(',');

                foreach (var word in words)
                {
                    // binary expressions begin type
                    string propname = typeGeneric;

                    var valueToChange = word;

                    var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                    var binaryExpression = Expression.Equal(
                        Expression.Property(parameterExpresion, propname),
                        Expression.Constant(valueToChange));
                    var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);                
                    // binary expressions end

                    // recupère le packet a rajouter                    
                    var toCombine = query.Where(lambda);
                    // remplis le packet
                    vide = vide.Concat(toCombine);
                    
                }

                if (!vide.Any())
                {
                    return NotFound("rien");
                }
            }

            if (!String.IsNullOrEmpty(price))
            {
                // culture pour autoriser . in decimal conversion
                CultureInfo provider = new CultureInfo("en-US");
                // vide2
                var videTwo = _context.Set<TModel>().Where(x => x.ID == -51);
                // check if query is empty 
                if (!vide.Any())
                {
                    vide = query;
                }
                // entre [] case
                if (price[0].Equals('['))
                {
                    //filterOne filterTwo init
                    var filterOne = "";
                    var filterTwo = "";
                    Decimal fillOne = 0;
                    Decimal fillTwo = 0;
                    // couper en deux à partie du ","
                    string[] numsToFilter = price.Split(',');
                    //check if contain number
                    filterOne = string.Join("", new Regex("[.0-9]").Matches(numsToFilter[0]));
                    filterTwo = string.Join("", new Regex("[.0-9]").Matches(numsToFilter[1]));
                    if (!String.IsNullOrEmpty(filterOne))
                    {
                        fillOne = Decimal.Parse(filterOne, provider);
                    }
                    if (!String.IsNullOrEmpty(filterTwo))
                    {
                        fillTwo = Decimal.Parse(filterTwo, provider);
                    }
                    //commencer le filtrage
                    if (!String.IsNullOrEmpty(filterTwo) && !String.IsNullOrEmpty(filterOne))
                    {

                        // binary expressions begin type
                        string propname = numGeneric;

                        var valueToChange = fillOne;
                        var valueToChangeTwo = fillTwo;

                        var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                        var binaryExpression = Expression.And(Expression.GreaterThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChange)), Expression.LessThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChangeTwo)));
                        var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                        // binary expressions end

                        vide = vide.Where(lambda);

                        if (!vide.Any())
                        {
                            return NotFound();
                        }
                    }
                    else
                    {
                        if (price[1].Equals(','))
                        {

                            // binary expressions begin type
                            string propname = numGeneric;

                            var valueToChange = fillTwo;                            

                            var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                            var binaryExpression = Expression.LessThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChange));
                            var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                            // binary expressions end

                            vide = vide.Where(lambda);

                            if (!vide.Any())
                            {
                                return NotFound();
                            }
                        }
                        else
                        {
                            // binary expressions begin type
                            string propname = numGeneric;

                            var valueToChange = fillOne;

                            var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                            var binaryExpression = Expression.GreaterThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChange));
                            var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                            // binary expressions end

                            vide = vide.Where(lambda);

                            if (!vide.Any())
                            {
                                return NotFound();
                            }
                        }
                    }

                }

                string queryType = price;
                string[] words = queryType.Split(',');

                // , coma case
                if (words.Length > 1 && !price[0].Equals('['))
                {
                    foreach (var word in words)
                    {                  
                        //convert to int
                        var newNumFilter = Decimal.Parse(word, provider);


                        // binary expressions begin type
                        string propname = numGeneric;

                        var valueToChange = newNumFilter;

                        var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                        var binaryExpression = Expression.Equal(
                        Expression.Property(parameterExpresion, propname),
                        Expression.Constant(valueToChange));
                        var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                        // binary expressions end


                        // recupère le packet a rajouter
                        var toCombineTwo = vide.Where(lambda);
                        // remplis le packet
                        videTwo = videTwo.Concat(toCombineTwo);
                    }
                    //attrib to vide originale
                    vide = videTwo;

                    if (!vide.Any())
                    {
                        return NotFound();
                    }
                }

                // normal case
                if (words.Length == 1 && !price[0].Equals('['))
                {
                    var priceInt = Decimal.Parse(price, provider);

                    // binary expressions begin type
                    string propname = numGeneric;

                    var valueToChange = priceInt;

                    var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                    var binaryExpression = Expression.Equal(
                    Expression.Property(parameterExpresion, propname),
                    Expression.Constant(valueToChange));
                    var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                    // binary expressions end

                    vide = vide.Where(lambda);

                    if (!vide.Any())
                    {
                        return NotFound();
                    }
                }
            }

            if (!String.IsNullOrEmpty(createdat))
            {
                // vide2
                var videTwo = _context.Set<TModel>().Where(x => x.ID == -51);
                // check if query is empty 
                if (!vide.Any())
                {
                    vide = query;
                }

                // entre [] case
                if (createdat[0].Equals('['))
                {
                    //filterOne filterTwo init
                    var filterOne = "";
                    var filterTwo = "";
                    DateTime fillOne = new DateTime(2015, 12, 25);
                    DateTime fillTwo = new DateTime(2015, 12, 25);
                    // couper en deux à partie du ","
                    string[] numsToFilter = createdat.Split(',');
                    //check if contain number
                    filterOne = numsToFilter[0].Replace('['.ToString(), String.Empty);
                    filterTwo = numsToFilter[1].Replace(']'.ToString(), String.Empty);
                    if (!String.IsNullOrEmpty(filterOne))
                    {
                        fillOne = Convert.ToDateTime(filterOne);
                    }
                    if (!String.IsNullOrEmpty(filterTwo))
                    {
                        fillTwo = Convert.ToDateTime(filterTwo);
                    }
                    //commencer le filtrage
                    if (!String.IsNullOrEmpty(filterTwo) && !String.IsNullOrEmpty(filterOne))
                    {

                        // binary expressions begin type
                        string propname = dateGeneric;

                        var valueToChange = fillOne.Date;
                        var valueToChangeTwo = fillTwo.Date;

                        var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                        var binaryExpression = Expression.And(Expression.GreaterThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChange)), Expression.LessThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChangeTwo)));
                        var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                        // binary expressions end

                        vide = vide.Where(lambda);
                    }
                    else
                    {
                        if (createdat[1].Equals(','))
                        {

                            // binary expressions begin type
                            string propname = dateGeneric;

                            var valueToChange = fillTwo.Date;

                            var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                            var binaryExpression = Expression.LessThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChange));
                            var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                            // binary expressions end

                            vide = vide.Where(lambda);
                        }
                        else
                        {

                            // binary expressions begin type
                            string propname = dateGeneric;

                            var valueToChange = fillOne.Date;

                            var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                            var binaryExpression = Expression.GreaterThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChange));
                            var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                            // binary expressions end

                            vide = vide.Where(lambda);
                        }
                    }

                }

                string queryType = createdat;
                string[] words = queryType.Split(',');

                // , coma case
                if (words.Length > 1 && !createdat[0].Equals('['))
                {
                    foreach (var word in words)
                    {
                        //convert to date                       
                        var ol = Convert.ToDateTime(word);

                        // binary expressions begin type
                        string propname = dateGeneric;

                        var valueToChange = ol.Date;

                        var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                        var binaryExpression = Expression.Equal(
                        Expression.Property(parameterExpresion, propname),
                        Expression.Constant(valueToChange));
                        var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                        // binary expressions end

                        // recupère le packet a rajouter
                        var toCombineTwo = vide.Where(lambda);
                        // remplis le packet
                        videTwo = videTwo.Concat(toCombineTwo);
                    }
                    //attrib to vide originale
                    vide = videTwo;
                }

                // normal case
                if (words.Length == 1 && !createdat[0].Equals('['))
                {
                    var ol = Convert.ToDateTime(createdat);

                    // binary expressions begin type
                    string propname = dateGeneric;

                    var valueToChange = ol.Date;                    

                    var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                    var binaryExpression = Expression.Equal(
                    Expression.Property(parameterExpresion, propname),
                    Expression.Constant(valueToChange));
                    var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                    // binary expressions end

                    vide = vide.Where(lambda);
                }
            }

            return await vide.ToListAsync();

        }

        // GET: api/sort/{model}
        [HttpGet("/sort")]
        public async Task<ActionResult<IEnumerable<TModel>>> GetSort([FromQuery(Name = "asc")] string asc, [FromQuery(Name = "desc")] string desc)
        {
            var source = _context.Set<TModel>();

            // source.OrderBy(x => x.GetType().GetProperty("Lastname")); KO
            Console.WriteLine(asc);
            if (asc == null)
            {
                var parameter = Expression.Parameter(typeof(TModel), "x");
                Expression property = Expression.Property(parameter, desc);
                var lambda = Expression.Lambda(property, parameter);

                var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == "OrderByDescending" && x.GetParameters().Length == 2);
                var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(TModel), property.Type);
                var result = orderByGeneric.Invoke(null, new object[] { source, lambda });


                return await ((IOrderedQueryable<TModel>)result).ToListAsync();
            }

            else

            {

                // LAMBDA: x => x.[PropertyName]
                var parameter = Expression.Parameter(typeof(TModel), "x");
                Expression property = Expression.Property(parameter, asc);
                var lambda = Expression.Lambda(property, parameter);

                // REFLECTION: source.OrderBy(x => x.Property)
                var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == "OrderBy" && x.GetParameters().Length == 2);
                var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(TModel), property.Type);
                var result = orderByGeneric.Invoke(null, new object[] { source, lambda });


                return await ((IOrderedQueryable<TModel>)result).ToListAsync();

            }
            //return await query2.ToListAsync();
        }

        // FILTER: api/search/{model}
        [HttpGet("/search")]
        public async Task<ActionResult<IEnumerable<TModel>>> GetSearch([FromQuery(Name = "typeGeneric")] string typeGeneric, [FromQuery(Name = "type")] string type, [FromQuery(Name = "numGeneric")] string numGeneric, [FromQuery(Name = "price")] string price, [FromQuery(Name = "dateGeneric")] string dateGeneric, [FromQuery(Name = "createdat")] string createdat, [FromQuery] Paginator filter, [FromQuery(Name = "asc")] string asc, [FromQuery(Name = "desc")] string desc)
        {
            // init
            var vide = _context.Set<TModel>().Where(x => x.ID == -51);
            var query = _context.Set<TModel>().Where(x => x.Active == true);

            // partie filtrage
            if (!String.IsNullOrEmpty(type))
            {
                string queryType = type;
                string[] words = queryType.Split(',');

                foreach (var word in words)
                {
                    // binary expressions begin type
                    string propname = typeGeneric;

                    var valueToChange = word;

                    var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                    var binaryExpression = Expression.Equal(
                        Expression.Property(parameterExpresion, propname),
                        Expression.Constant(valueToChange));
                    var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                    // binary expressions end

                    // recupère le packet a rajouter                    
                    var toCombine = query.Where(lambda);
                    // remplis le packet
                    vide = vide.Concat(toCombine);

                    if (!vide.Any())
                    {
                        return NotFound();
                    }
                }
            }

            if (!String.IsNullOrEmpty(price))
            {
                // culture pour autoriser . in decimal conversion
                CultureInfo provider = new CultureInfo("en-US");
                // vide2
                var videTwo = _context.Set<TModel>().Where(x => x.ID == -51);
                // check if query is empty 
                if (!vide.Any())
                {
                    vide = query;
                }
                // entre [] case
                if (price[0].Equals('['))
                {
                    //filterOne filterTwo init
                    var filterOne = "";
                    var filterTwo = "";
                    Decimal fillOne = 0;
                    Decimal fillTwo = 0;
                    // couper en deux à partie du ","
                    string[] numsToFilter = price.Split(',');
                    //check if contain number
                    filterOne = string.Join("", new Regex("[.0-9]").Matches(numsToFilter[0]));
                    filterTwo = string.Join("", new Regex("[.0-9]").Matches(numsToFilter[1]));
                    if (!String.IsNullOrEmpty(filterOne))
                    {
                        fillOne = Decimal.Parse(filterOne, provider);
                    }
                    if (!String.IsNullOrEmpty(filterTwo))
                    {
                        fillTwo = Decimal.Parse(filterTwo, provider);
                    }
                    //commencer le filtrage
                    if (!String.IsNullOrEmpty(filterTwo) && !String.IsNullOrEmpty(filterOne))
                    {

                        // binary expressions begin type
                        string propname = numGeneric;

                        var valueToChange = fillOne;
                        var valueToChangeTwo = fillTwo;

                        var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                        var binaryExpression = Expression.And(Expression.GreaterThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChange)), Expression.LessThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChangeTwo)));
                        var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                        // binary expressions end

                        vide = vide.Where(lambda);

                        if (!vide.Any())
                        {
                            return NotFound();
                        }
                    }
                    else
                    {
                        if (price[1].Equals(','))
                        {

                            // binary expressions begin type
                            string propname = numGeneric;

                            var valueToChange = fillTwo;

                            var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                            var binaryExpression = Expression.LessThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChange));
                            var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                            // binary expressions end

                            vide = vide.Where(lambda);

                            if (!vide.Any())
                            {
                                return NotFound();
                            }
                        }
                        else
                        {
                            // binary expressions begin type
                            string propname = numGeneric;

                            var valueToChange = fillOne;

                            var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                            var binaryExpression = Expression.GreaterThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChange));
                            var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                            // binary expressions end

                            vide = vide.Where(lambda);

                            if (!vide.Any())
                            {
                                return NotFound();
                            }
                        }
                    }

                }

                string queryType = price;
                string[] words = queryType.Split(',');

                // , coma case
                if (words.Length > 1 && !price[0].Equals('['))
                {
                    foreach (var word in words)
                    {
                        //convert to int
                        var newNumFilter = Decimal.Parse(word, provider);


                        // binary expressions begin type
                        string propname = numGeneric;

                        var valueToChange = newNumFilter;

                        var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                        var binaryExpression = Expression.Equal(
                        Expression.Property(parameterExpresion, propname),
                        Expression.Constant(valueToChange));
                        var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                        // binary expressions end


                        // recupère le packet a rajouter
                        var toCombineTwo = vide.Where(lambda);
                        // remplis le packet
                        videTwo = videTwo.Concat(toCombineTwo);
                    }
                    //attrib to vide originale
                    vide = videTwo;

                    if (!vide.Any())
                    {
                        return NotFound();
                    }
                }

                // normal case
                if (words.Length == 1 && !price[0].Equals('['))
                {
                    var priceInt = Decimal.Parse(price, provider);

                    // binary expressions begin type
                    string propname = numGeneric;

                    var valueToChange = priceInt;

                    var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                    var binaryExpression = Expression.Equal(
                    Expression.Property(parameterExpresion, propname),
                    Expression.Constant(valueToChange));
                    var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                    // binary expressions end

                    vide = vide.Where(lambda);

                    if (!vide.Any())
                    {
                        return NotFound();
                    }
                }
            }

            if (!String.IsNullOrEmpty(createdat))
            {
                // vide2
                var videTwo = _context.Set<TModel>().Where(x => x.ID == -51);
                // check if query is empty 
                if (!vide.Any())
                {
                    vide = query;
                }

                // entre [] case
                if (createdat[0].Equals('['))
                {
                    //filterOne filterTwo init
                    var filterOne = "";
                    var filterTwo = "";
                    DateTime fillOne = new DateTime(2015, 12, 25);
                    DateTime fillTwo = new DateTime(2015, 12, 25);
                    // couper en deux à partie du ","
                    string[] numsToFilter = createdat.Split(',');
                    //check if contain number
                    filterOne = numsToFilter[0].Replace('['.ToString(), String.Empty);
                    filterTwo = numsToFilter[1].Replace(']'.ToString(), String.Empty);
                    if (!String.IsNullOrEmpty(filterOne))
                    {
                        fillOne = Convert.ToDateTime(filterOne);
                    }
                    if (!String.IsNullOrEmpty(filterTwo))
                    {
                        fillTwo = Convert.ToDateTime(filterTwo);
                    }
                    //commencer le filtrage
                    if (!String.IsNullOrEmpty(filterTwo) && !String.IsNullOrEmpty(filterOne))
                    {

                        // binary expressions begin type
                        string propname = dateGeneric;

                        var valueToChange = fillOne.Date;
                        var valueToChangeTwo = fillTwo.Date;

                        var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                        var binaryExpression = Expression.And(Expression.GreaterThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChange)), Expression.LessThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChangeTwo)));
                        var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                        // binary expressions end

                        vide = vide.Where(lambda);
                    }
                    else
                    {
                        if (createdat[1].Equals(','))
                        {

                            // binary expressions begin type
                            string propname = dateGeneric;

                            var valueToChange = fillTwo.Date;

                            var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                            var binaryExpression = Expression.LessThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChange));
                            var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                            // binary expressions end

                            vide = vide.Where(lambda);
                        }
                        else
                        {

                            // binary expressions begin type
                            string propname = dateGeneric;

                            var valueToChange = fillOne.Date;

                            var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                            var binaryExpression = Expression.GreaterThanOrEqual(
                            Expression.Property(parameterExpresion, propname),
                            Expression.Constant(valueToChange));
                            var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                            // binary expressions end

                            vide = vide.Where(lambda);
                        }
                    }

                }

                string queryType = createdat;
                string[] words = queryType.Split(',');

                // , coma case
                if (words.Length > 1 && !createdat[0].Equals('['))
                {
                    foreach (var word in words)
                    {
                        //convert to date                       
                        var ol = Convert.ToDateTime(word);

                        // binary expressions begin type
                        string propname = dateGeneric;

                        var valueToChange = ol.Date;

                        var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                        var binaryExpression = Expression.Equal(
                        Expression.Property(parameterExpresion, propname),
                        Expression.Constant(valueToChange));
                        var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                        // binary expressions end

                        // recupère le packet a rajouter
                        var toCombineTwo = vide.Where(lambda);
                        // remplis le packet
                        videTwo = videTwo.Concat(toCombineTwo);
                    }
                    //attrib to vide originale
                    vide = videTwo;
                }

                // normal case
                if (words.Length == 1 && !createdat[0].Equals('['))
                {
                    var ol = Convert.ToDateTime(createdat);

                    // binary expressions begin type
                    string propname = dateGeneric;

                    var valueToChange = ol.Date;

                    var parameterExpresion = Expression.Parameter(typeof(TModel), "d");
                    var binaryExpression = Expression.Equal(
                    Expression.Property(parameterExpresion, propname),
                    Expression.Constant(valueToChange));
                    var lambda = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameterExpresion);
                    // binary expressions end

                    vide = vide.Where(lambda);
                }
            }

            if (!vide.Any())
            {
                return await vide.ToListAsync();
            }

            // paginate

            var paginator = new Paginator(filter.range, filter.current_page);
            var source = vide;
            var result1 = source.Skip((paginator.current_page - 1) * paginator.range)
            .Take(paginator.range);

            //tri

            if (asc == null)
            {
                var parameter = Expression.Parameter(typeof(TModel), "x");
                Expression property = Expression.Property(parameter, desc);
                var lambda = Expression.Lambda(property, parameter);

                var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == "OrderByDescending" && x.GetParameters().Length == 2);
                var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(TModel), property.Type);
                var result = orderByGeneric.Invoke(null, new object[] { result1, lambda });


                return await ((IOrderedQueryable<TModel>)result).ToListAsync();
            }

            else

            {

                // LAMBDA: x => x.[PropertyName]
                var parameter = Expression.Parameter(typeof(TModel), "x");
                Expression property = Expression.Property(parameter, asc);
                var lambda = Expression.Lambda(property, parameter);

                // REFLECTION: source.OrderBy(x => x.Property)
                var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == "OrderBy" && x.GetParameters().Length == 2);
                var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(TModel), property.Type);
                var result = orderByGeneric.Invoke(null, new object[] { result1, lambda });


                return await ((IOrderedQueryable<TModel>)result).ToListAsync();

            }
        }
        
    }
}
