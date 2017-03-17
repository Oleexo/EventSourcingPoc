using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Command;
using EventSourcing.Poc.EventSourcing.Jobs;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.Processing;
using EventSourcing.Poc.Processing.Jobs;
using EventSourcing.Poc.Processing.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventSourcing.Poc.Api {
    public class Startup {
        public Startup(IHostingEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // Add framework services.
            services.AddMvc();
            services.AddOptions();

            services.Configure<CommandStoreOptions>(Configuration.GetSection("CommandStore"));
            services.Configure<CommandQueueOptions>(Configuration.GetSection("CommandQueue"));
            services.Configure<JobHandlerOptions>(Configuration.GetSection("JobHandler"));

            services.AddScoped<IJobHandler, JobHandler>();
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<ICommandQueue, CommandQueue>();
            services.AddScoped<ICommandStore, CommandStore>();
            services.AddScoped<IJobFactory, JobFactory>();
            services.AddScoped<IJobFollower, JobHandler>();
            services.AddScoped<IJsonConverter, NewtonsoftJsonConverter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}