using System;
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
using EventSourcing.Poc.Processing.Commons.Security;
using EventSourcing.Poc.Processing.Jobs;
using EventSourcing.Poc.Processing.Mutex;
using EventSourcing.Poc.Processing.Options;
using Microsoft.Azure.ServiceBus;
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
                options.ArchiveConnectionString = _configurationRoot.GetSection("JobHandler")["ArchiveConnectionString"];
                options.ArchiveStorageName = _configurationRoot.GetSection("JobHandler")["ArchiveStorageName"];
                options.ArchiveTableName = _configurationRoot.GetSection("JobHandler")["ArchiveTableName"];
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
            serviceCollection.Configure<CommandStoreOptions>(options => {
                options.ConnectionString = _configurationRoot.GetSection("CommandStore")["ConnectionString"];
                options.Name = _configurationRoot.GetSection("CommandStore")["Name"];
            });
            serviceCollection.Configure<SecurityServiceOptions>(options => {
                options.Encryption = bool.Parse(_configurationRoot.GetSection("Security")["Encryption"]);
                options.Key = _configurationRoot.GetSection("Security")["Key"];
            });
            serviceCollection.AddScoped<ICommandQueue, CommandQueue>();
            serviceCollection.AddScoped<ICommandStore, CommandStore>();
            serviceCollection.AddScoped<IJsonConverter, NewtonsoftJsonConverter>();
            serviceCollection.AddScoped<IEventStore, EventStore>();
            serviceCollection.AddScoped<IJobHandler, JobHandler>();
            serviceCollection.AddScoped<IEventDispatcher, EventDispatcher>();
            serviceCollection.AddScoped<IEntityMutexFactory, EntityMutexFactory>();
            serviceCollection.AddScoped<IMutexCollector, MutexCollector>();
            serviceCollection.AddScoped<ICommandHandlerFactory, CommandHandlerFactory>();
            serviceCollection.AddScoped<CommandProcessor>();
            serviceCollection.AddScoped<PostHandler>();
            serviceCollection.AddScoped<ISecurityService, SecurityService>();
            serviceCollection.AddScoped<IEventProcessor, EventProcessor>();
            serviceCollection.AddScoped<IEventHandlerFactory, EventHandlerFactory>();
            serviceCollection.AddScoped<IActionDispatcher, ActionDispatcher>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            var commandQueue = serviceProvider.GetService<ICommandQueue>();
            Console.WriteLine("Start listening...");
            commandQueue.RegisterMessageHandler(async (wrapper, token) => {
                Console.WriteLine("Message receive :" + wrapper.Id);
                using (var scopedServiceProvider = serviceProvider.CreateScope()) {
                    var commandProcessor = scopedServiceProvider.ServiceProvider.GetService<CommandProcessor>();
                    var commandType = wrapper.GetType().GetTypeInfo().GetGenericArguments()[0];
                    await (Task) commandProcessor.GetType()
                        .GetMethod("Process")
                        .MakeGenericMethod(commandType)
                        .Invoke(commandProcessor, new object[] {wrapper});
                    scopedServiceProvider.ServiceProvider.GetService<IMutexCollector>().Collect();
                }
            }, new RegisterHandlerOptions {
                MaxConcurrentCalls = 10
            });
        }
    }
}