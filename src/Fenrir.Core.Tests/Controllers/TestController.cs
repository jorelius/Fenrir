using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Fenrir.Core.Tests.Controllers
{
    [Controller]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class TestController : Controller
    {
        [HttpGet("TestGet")]
        public IEnumerable<TestGetResponse> TestGet(int numberOfResponses = 1)
        {
            return Enumerable.Range(1, numberOfResponses).Select(index => new TestGetResponse
            {
               Index = index
            });
        }

        public class TestGetResponse
        {
            public int Index { get; set; }
        }

        static Dictionary<string, Thing> things = new Dictionary<string, Thing>(); 

        [HttpPut("things/{id}")]
        public IActionResult ThingPut([FromRoute]string id, Thing thing)
        {
            if (things.ContainsKey(id))
            {
                things[id] = thing; 
                return Ok(thing); 
            }

            things.Add(id, thing); 
            return Created("api/test/things/{id}", thing); 
        }

        [HttpGet("things/{id}")]
        public IActionResult ThingGet([FromRoute]string id)
        {
            if (!things.ContainsKey(id))
            {   
                return NotFound(); 
            }

            var thing = things[id]; 
            return Ok(thing); 
        }

        [HttpGet("echo/{id}")]
        public IActionResult Echo([FromRoute]string id)
        {
            return Ok(id);
        }

        public class Thing
        {
            string Id { get; set; }
            string What { get; set; }
        }
    }


}
