using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Aerende.Service.API.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace Aerende.Service.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public AppSettings AppSettings { get; }

        public ValuesController(IOptions<AppSettings> options)
        {
            AppSettings = options.Value;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<Patients>> Get()
        {
            //Använd denna metod för att SeedaData i Auth och Aerende
            List<Patients> items = null;
            using (StreamReader reader = new StreamReader(AppSettings.SeedData))
            {
                string json = reader.ReadToEnd();
               items = JsonConvert.DeserializeObject<List<Patients>>(json);
            }

                return items;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
