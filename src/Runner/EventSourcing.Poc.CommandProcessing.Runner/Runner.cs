using System.Reflection;
using System.Threading.Tasks;
using EventSourcing.Poc.CommandProcessing.Options;
using EventSourcing.Poc.Commons;
using EventSourcing.Poc.Commons.Mutex;
using EventSourcing.Poc.Domain;
using EventSourcing.Poc.EventSourcing.Mutex;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;

namespace EventSourcing.Poc.CommandProcessing.Runner {
    internal class Runner {
        private readonly IConfigurationRoot _configurationRoot;

        public Runner(IConfigurationRoot configurationRoot) {
            _configurationRoot = configurationRoot;
        }

        public void Run() {
            ;
            var commandQueue = new CommandQueue(
                CloudStorageAccount.Parse(_configurationRoot.GetSection("CommandQueue")["FileShareConnectionString"])
                    .CreateCloudFileClient()
                    .GetShareReference(_configurationRoot.GetSection("CommandQueue")["FileShareName"]),
                new QueueClient(_configurationRoot.GetSection("CommandQueue")["QueueConnectionString"], _configurationRoot.GetSection("CommandQueue")["QueueName"], ReceiveMode.ReceiveAndDelete),
                new NewtonsoftJsonConverter());
            CommandHandlerFactory.AddCommandHandler(typeof(PostHandler).GetTypeInfo().Assembly);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.Configure<EntityMutexFactoryOptions>(options => {
                options.ConnectionString = _configurationRoot.GetSection("Lock")["ConnectionString"];
                options.Name = _configurationRoot.GetSection("Lock")["Name"];
            });
            serviceCollection.AddTransient<IEntityMutexFactory, EntityMutexFactory>();
            serviceCollection.AddTransient<PostHandler>();
            var commandHandlerFactory = new CommandHandlerFactory(serviceCollection.BuildServiceProvider());
            var commandProcessor = new CommandProcessor(commandHandlerFactory);
            var commandProcessorType = commandProcessor.GetType().GetTypeInfo();
            commandQueue.RegisterMessageHandler(async (wrapper, token) => {
                var commandType = wrapper.GetType().GetTypeInfo().GetGenericArguments()[0];
                await (Task) commandProcessorType.GetMethod("Process")
                    .MakeGenericMethod(commandType)
                    .Invoke(commandProcessor, new object[] {wrapper});
            });
        }
    }
}