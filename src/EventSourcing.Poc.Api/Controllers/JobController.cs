using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace EventSourcing.Poc.Api.Controllers {
    [Route("api/[controller]")]
    public class JobController : Controller {
        private readonly IJobFollower _jobFollower;

        public JobController(IJobFollower jobFollower) {
            _jobFollower = jobFollower;
        }

        [HttpGet("{id}")]
        public async Task<IJob> GetAsync(string id) {
            return await _jobFollower.GetInformation(id);
        }
    }
}