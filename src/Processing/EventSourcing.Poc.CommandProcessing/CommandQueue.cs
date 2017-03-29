using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Command;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Processing.Commons.Security;
using EventSourcing.Poc.Processing.Generic;
using EventSourcing.Poc.Processing.Options;
using Microsoft.Extensions.Options;

namespace EventSourcing.Poc.Processing {
    public class CommandQueue : Queue<ICommandWrapper>, ICommandQueue {
        public CommandQueue(IOptions<CommandQueueOptions> options,
            IJsonConverter jsonConverter,
            ISecurityService securityService)
            : base(options.Value.QueueConnectionString, options.Value.QueueName,
                options.Value.FileShareConnectionString, options.Value.FileShareName,
                jsonConverter, securityService) {
        }
    }
}