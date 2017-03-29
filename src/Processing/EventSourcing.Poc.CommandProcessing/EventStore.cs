using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Processing.Commons.Security;
using EventSourcing.Poc.Processing.Generic;
using EventSourcing.Poc.Processing.Options;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.File;

namespace EventSourcing.Poc.Processing {
    public class EventStore : FileStore<IEventWrapper>, IEventStore {
        public EventStore(IOptions<EventStoreOptions> options,
            IJsonConverter jsonConverter,
            ISecurityService securityService)
            : base(options.Value.ConnectionString, options.Value.Name, jsonConverter, securityService) {
        }

        public async Task Save(IEventWrapper wrappedEvent) {
            await SaveAsync(wrappedEvent);
        }

        public async Task Save(IReadOnlyCollection<IEventWrapper> wrappedEvents) {
            await SaveAsync(wrappedEvents);
        }

        protected override async Task<CloudFileDirectory> GetDestinationFolder(CloudFileShare fileShare) {
            var currentDate = DateTimeOffset.UtcNow;
            var root = fileShare.GetRootDirectoryReference();
            var yearFolder = root.GetDirectoryReference(currentDate.Year.ToString("D4"));
            await yearFolder.CreateIfNotExistsAsync();
            var monthFolder = yearFolder.GetDirectoryReference(currentDate.Month.ToString("D2"));
            await monthFolder.CreateIfNotExistsAsync();
            var dayFolder = monthFolder.GetDirectoryReference(currentDate.Day.ToString("D2"));
            await dayFolder.CreateIfNotExistsAsync();
            return dayFolder;
        }

        protected override string CreateFileName(IEventWrapper @object) {
            return $"{@object.Id}.json";
        }
    }
}