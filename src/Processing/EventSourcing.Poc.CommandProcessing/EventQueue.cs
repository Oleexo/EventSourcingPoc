using EventSourcing.Poc.EventSourcing.Event;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Processing.Generic;
using EventSourcing.Poc.Processing.Options;
using Microsoft.Extensions.Options;

namespace EventSourcing.Poc.Processing {
    public class EventQueue : Queue<IEventWrapper>, IEventQueue {
        public EventQueue(IOptions<EventQueueOptions> options, IJsonConverter jsonConverter)
            : base(options.Value.QueueConnectionString, options.Value.QueueName,
                options.Value.FileShareConnectionString, options.Value.FileShareName, jsonConverter) {
        }
    }
}