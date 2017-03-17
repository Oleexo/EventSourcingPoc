using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.EventSourcing.Wrapper;
using Microsoft.Azure.ServiceBus;
using Microsoft.WindowsAzure.Storage.File;

namespace EventSourcing.Poc.Processing.Generic {
    public abstract class Queue<TWrapper> : FileStore<TWrapper> where TWrapper : class, IWrapper {
        private readonly IJsonConverter _jsonConverter;
        private readonly QueueClient _queueClient;

        public Queue(string queueConnectionString, string queueName,
            string storageConnectionString, string storageName,
            IJsonConverter jsonConverter)
            : base(storageConnectionString, storageName, jsonConverter) {
            _jsonConverter = jsonConverter;
            _queueClient = new QueueClient(queueConnectionString, queueName, ReceiveMode.ReceiveAndDelete);
        }

        public virtual async Task Send(TWrapper wrapper) {
            await SaveAsync(wrapper);
            var message = new QueueMessage {
                Id = wrapper.Id
            };
            await _queueClient.SendAsync(new Message(_jsonConverter.Serialize(message)));
        }

        public virtual void RegisterMessageHandler(Func<TWrapper, CancellationToken, Task> handler) {
            _queueClient.RegisterMessageHandler(async (message, token) => {
                var body = message.GetBody<string>();
                var queueMessage = _jsonConverter.Deserialize<QueueMessage>(body);
                var wrapper = await RetrieveAsync($"{queueMessage.Id}.json", true);
                await handler.Invoke(wrapper, token);
            });
        }

        protected override string CreateFileName(TWrapper @object) {
            return $"{@object.Id}.json";
        }

        protected override Task<CloudFileDirectory> GetDestinationFolder(CloudFileShare fileShare) {
            return Task.FromResult(fileShare.GetRootDirectoryReference());
        }

        private class QueueMessage {
            public Guid Id { get; set; }
            public string Type { get; set; }
        }
    }
}