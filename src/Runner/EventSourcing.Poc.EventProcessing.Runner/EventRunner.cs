using System.Reflection;
using System.Threading.Tasks;
using EventSourcing.Poc.Domain;
using EventSourcing.Poc.EventSourcing.Event;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Mutex;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.Processing;
using EventSourcing.Poc.Processing.Jobs;
using EventSourcing.Poc.Processing.Mutex;
using EventSourcing.Poc.Processing.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Poc.EventProcessing.Runner {
    internal class EventRunner {
        private readonly IConfigurationRoot _configurationRoot;

        public EventRunner(IConfigurationRoot configurationRoot) {
            _configurationRoot = configurationRoot;
        }

        public void Run() {
            EventHandlerFactory.AddEventHandler(typeof(PostHandler).GetTypeInfo().Assembly);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.Configure<EntityMutexFactoryOptions>(options => {
                options.ConnectionString = _configurationRoot.GetSection("Lock")["ConnectionString"];
                options.Name = _configurationRoot.GetSection("Lock")["Name"];
            });
            serviceCollection.Configure<JobHandlerOptions>(options => {
                options.ConnectionString = _configurationRoot.GetSection("JobHandler")["ConnectionString"];
                options.JobTableName = _configurationRoot.GetSection("JobHandler")["JobTableName"];
                options.CommandTableName = _configurationRoot.GetSection("JobHandler")["CommandTableName"];
                options.EventTableName = _configurationRoot.GetSection("JobHandler")["EventTableName"];
                options.ActionTableName = _configurationRoot.GetSection("JobHandler")["ActionTableName"];
            });
            serviceCollection.Configure<EventQueueOptions>(options => {
                options.FileShareConnectionString =
                    _configurationRoot.GetSection("EventQueue")["FileShareConnectionString"];
                options.FileShareName = _configurationRoot.GetSection("EventQueue")["FileShareName"];
                options.QueueConnectionString = _configurationRoot.GetSection("EventQueue")["QueueConnectionString"];
                options.QueueName = _configurationRoot.GetSection("EventQueue")["QueueName"];
            });
            serviceCollection.AddTransient<IEntityMutexFactory, EntityMutexFactory>();
            serviceCollection.AddTransient<IJsonConverter, NewtonsoftJsonConverter>();
            serviceCollection.AddTransient<IJobHandler, JobHandler>();
            serviceCollection.AddTransient<IEventQueue, EventQueue>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var eventHandlerFactory = new EventHandlerFactory(serviceProvider);
            var eventProcessor = new EventProcessor(eventHandlerFactory, serviceProvider.GetService<IJobHandler>());
            var commandProcessorType = eventProcessor.GetType().GetTypeInfo();
            var eventQueue = serviceProvider.GetService<IEventQueue>();
            eventQueue.RegisterMessageHandler(async (wrapper, token) => {
                var eventType = wrapper.GetType().GetTypeInfo().GetGenericArguments()[0];
                await (Task)commandProcessorType.GetMethod("Process")
                    .MakeGenericMethod(eventType)
                    .Invoke(eventProcessor, new object[] { wrapper });
            });
        }
    }
}