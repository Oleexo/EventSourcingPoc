using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.CommandProcessing.Options;
using EventSourcing.Poc.EventSourcing;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.EventSourcing.Wrapper;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.File;

namespace EventSourcing.Poc.CommandProcessing {
    public class CommandStore : CommandFileStore, ICommandStore {
        public CommandStore(IOptions<CommandStoreOptions> options, IJsonConverter jsonConverter)
            : base(options.Value.ConnectionString, options.Value.Name, jsonConverter) {
        }

        public async Task Save(ICommandWrapper commandWrapped) {
            await SaveAsync(commandWrapped);
        }

        public async Task Save(IReadOnlyCollection<ICommandWrapper> commandWrappeds) {
            await SaveAsync(commandWrappeds);
        }

        public override async Task<CloudFileDirectory> GetDestinationFolder(CloudFileShare fileShare) {
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
    }
}