using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace EventSourcing.Poc.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddOptions();

            services.Configure<CommandStoreOptions>(Configuration.GetSection("CommandStore"));
            services.Configure<CommandQueueOptions>(Configuration.GetSection("CommandQueue"));
            services.Configure<JobHandlerOptions>(Configuration.GetSection("JobHandler"));

            services.AddTransient<IJobHandler, JobHandler>();
            services.AddTransient<ICommandDispatcher, CommandDispatcher>();
            services.AddTransient<ICommandQueue, CommandQueue>();
            services.AddTransient<ICommandStore, CommandStore>();
            services.AddTransient<IJobFactory, JobFactory>();
            services.AddTransient<IJsonConverter, NewtonsoftJsonConverter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}
