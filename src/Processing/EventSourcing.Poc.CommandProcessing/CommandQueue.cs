using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Poc.CommandProcessing.Options;
using EventSourcing.Poc.EventSourcing.Command;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.EventSourcing.Wrapper;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.File;

namespace EventSourcing.Poc.CommandProcessing {
    public class CommandQueue : CommandFileStore, ICommandQueue {
        private readonly IQueueClient _commandQueueClient;
        private readonly IJsonConverter _jsonConverter;

        public CommandQueue(IOptions<CommandQueueOptions> options,
            IJsonConverter jsonConverter)
            : base(options.Value.FileShareConnectionString, options.Value.FileShareName, jsonConverter) {
            _commandQueueClient = new QueueClient(options.Value.QueueConnectionString, options.Value.QueueName,
                ReceiveMode.PeekLock);
            _jsonConverter = jsonConverter;
        }

        public CommandQueue(CloudFileShare fileShare, IQueueClient queueClient, IJsonConverter jsonConverter) : base(
            fileShare, jsonConverter) {
            _jsonConverter = jsonConverter;
            _commandQueueClient = queueClient;
        }

        public async Task Send(ICommandWrapper commandWrapper) {
            await SaveAsync(commandWrapper);
            var cm = new CommandMessage(commandWrapper);
            await _commandQueueClient.SendAsync(new Message(_jsonConverter.Serialize(cm)));
        }

        public void RegisterMessageHandler(Func<ICommandWrapper, CancellationToken, Task> handler) {
            _commandQueueClient.RegisterMessageHandler(async (message, token) => {
                var body = message.GetBody<string>();
                var cm = _jsonConverter.Deserialize<CommandMessage>(body);
                var commandWrapped = await RetrieveAsync(cm.CommandId, true);
                await handler.Invoke(commandWrapped, token);
            });
        }

        public override Task<CloudFileDirectory> GetDestinationFolder(CloudFileShare fileShare) {
            return Task.FromResult(fileShare.GetRootDirectoryReference());
        }

        private class CommandMessage {
            public CommandMessage(ICommandWrapper commandWrapper) {
                CommandId = commandWrapper.Id;
            }

            public CommandMessage() {
            }

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public Guid CommandId { get; set; }
        }
    }
}