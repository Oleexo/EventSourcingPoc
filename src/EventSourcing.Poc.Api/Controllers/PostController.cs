using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Command;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.Messages.Post;
using Microsoft.AspNetCore.Mvc;

namespace EventSourcing.Poc.Api.Controllers
{
    [Route("api/[controller]")]
    public class PostController : Controller
    {
        private readonly ICommandDispatcher _commandDispatcher;

        // GET api/values
        public PostController(ICommandDispatcher commandDispatcher) {
            _commandDispatcher = commandDispatcher;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public async Task<IJob> Post([FromBody]PostPostModel value, [FromQuery]int timeout = 60)
        {
            var command = new CreatePost(value.Title, value.Content);
            return await _commandDispatcher.Send(command, TimeSpan.FromSeconds(timeout));
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        public class PostPostModel {
            public string Title { get; set; }
            public string Content { get; set; }
        }
    }
}
