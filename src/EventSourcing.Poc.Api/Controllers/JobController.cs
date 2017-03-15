using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace EventSourcing.Poc.Api.Controllers {
    [Route("api/[controller]")]
    public class JobController : Controller {
        private readonly IJobHandler _jobHandler;

        public JobController(IJobHandler jobHandler) {
            _jobHandler = jobHandler;
        }

        [HttpGet("{id}")]
        public async Task<IJob> GetAsync(string id) {
            return await _jobHandler.GetInformation(id);
        }
    }
}