using System.Reflection;
using System.Threading.Tasks;
using EventSourcing.Poc.Domain;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Command;
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

namespace EventSourcing.Poc.CommandProcessing.Runner {
    internal class CommandRunner {
        private readonly IConfigurationRoot _configurationRoot;

        public CommandRunner(IConfigurationRoot configurationRoot) {
            _configurationRoot = configurationRoot;
        }

        public void Run() {
            CommandHandlerFactory.AddCommandHandler(typeof(PostHandler).GetTypeInfo().Assembly);
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
            serviceCollection.Configure<CommandQueueOptions>(options => {
                options.FileShareConnectionString =
                    _configurationRoot.GetSection("CommandQueue")["FileShareConnectionString"];
                options.FileShareName = _configurationRoot.GetSection("CommandQueue")["FileShareName"];
                options.QueueConnectionString = _configurationRoot.GetSection("CommandQueue")["QueueConnectionString"];
                options.QueueName = _configurationRoot.GetSection("CommandQueue")["QueueName"];
            });
            serviceCollection.Configure<EventQueueOptions>(options => {
                options.FileShareConnectionString =
                    _configurationRoot.GetSection("EventQueue")["FileShareConnectionString"];
                options.FileShareName = _configurationRoot.GetSection("EventQueue")["FileShareName"];
                options.QueueConnectionString = _configurationRoot.GetSection("EventQueue")["QueueConnectionString"];
                options.QueueName = _configurationRoot.GetSection("EventQueue")["QueueName"];
            });
            serviceCollection.Configure<EventStoreOptions>(options => {
                options.ConnectionString = _configurationRoot.GetSection("EventStore")["ConnectionString"];
                options.Name = _configurationRoot.GetSection("EventStore")["Name"];
            });
            serviceCollection.AddTransient<ICommandQueue, CommandQueue>();
            serviceCollection.AddTransient<IJsonConverter, NewtonsoftJsonConverter>();
            serviceCollection.AddTransient<IEventQueue, EventQueue>();
            serviceCollection.AddTransient<IEventStore, EventStore>();
            serviceCollection.AddTransient<IJobHandler, JobHandler>();
            serviceCollection.AddTransient<IEventDispatcher, EventDispatcher>();
            serviceCollection.AddTransient<IEntityMutexFactory, EntityMutexFactory>();
            serviceCollection.AddTransient<PostHandler>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var commandHandlerFactory = new CommandHandlerFactory(serviceProvider);
            var commandProcessor = new CommandProcessor(commandHandlerFactory,
                serviceProvider.GetService<IEventDispatcher>(),
                serviceProvider.GetService<IJobHandler>());
            var commandProcessorType = commandProcessor.GetType().GetTypeInfo();
            var commandQueue = serviceProvider.GetService<ICommandQueue>();
            commandQueue.RegisterMessageHandler(async (wrapper, token) => {
                var commandType = wrapper.GetType().GetTypeInfo().GetGenericArguments()[0];
                await (Task) commandProcessorType.GetMethod("Process")
                    .MakeGenericMethod(commandType)
                    .Invoke(commandProcessor, new object[] {wrapper});
            });
        }
    }
}