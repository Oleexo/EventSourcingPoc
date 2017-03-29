using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Processing.Commons.Security;
using Microsoft.Azure.ServiceBus;

namespace EventSourcing.Poc.Processing.Generic {
    public abstract class Queue<TWrapper> where TWrapper : class, IWrapper {
        private readonly IJsonConverter _jsonConverter;
        private readonly Lazy<QueueClient> _queueClient;
        private readonly QueueStorage<TWrapper> _queueStorage;

        protected Queue(string queueConnectionString, string queueName,
            string storageConnectionString, string storageName,
            IJsonConverter jsonConverter,
            ISecurityService securityService) {
            _jsonConverter = jsonConverter;
            _queueClient = new Lazy<QueueClient>(
                () => new QueueClient(queueConnectionString, queueName, ReceiveMode.ReceiveAndDelete));
            _queueStorage = new QueueStorage<TWrapper>(storageConnectionString, storageName, jsonConverter,
                securityService);
        }

        private QueueClient QueueClient => _queueClient.Value;

        public virtual async Task Send(TWrapper wrapper) {
            await _queueStorage.Save(wrapper);
            var message = new QueueMessage {
                Id = wrapper.Id
            };
            var serializedMessage = _jsonConverter.Serialize(message);
            var queueMessage = new Message(serializedMessage);
            await QueueClient.SendAsync(queueMessage);
        }

        public virtual void RegisterMessageHandler(Func<TWrapper, CancellationToken, Task> handler,
            RegisterHandlerOptions handlerOptions) {
            QueueClient.RegisterMessageHandler((message, token) => Handler(message, token, handler), handlerOptions);
        }

        private async Task Handler(Message message, CancellationToken token,
            Func<TWrapper, CancellationToken, Task> handler) {
            var body = message.GetBody<string>();
            var wrapper = await ConvertBack(body, _jsonConverter, _queueStorage);
            await handler.Invoke(wrapper, token);
        }

        public static async Task<TWrapper> ConvertBack(string message, IJsonConverter jsonConverter, QueueStorage<TWrapper> queueStorage) {
            var queueMessage = jsonConverter.Deserialize<QueueMessage>(message);
            return await queueStorage.Retrieve(queueMessage.Id.ToString(), true);
        }

        public virtual void RegisterMessageHandler(Func<TWrapper, CancellationToken, Task> handler) {
            QueueClient.RegisterMessageHandler((message, token) => Handler(message, token, handler));
        }

        private class QueueMessage {
            public Guid Id { get; set; }
            public string Type { get; set; }
        }
    }
}