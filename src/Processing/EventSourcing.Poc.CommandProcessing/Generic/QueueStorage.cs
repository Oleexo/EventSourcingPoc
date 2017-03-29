using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.EventSourcing.Wrapper;
using EventSourcing.Poc.Processing.Commons.Security;
using EventSourcing.Poc.Processing.Options;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.File;

namespace EventSourcing.Poc.Processing.Generic {
    public class QueueStorage<TWrapper> : FileStore<TWrapper> where TWrapper : class, IWrapper {
        public QueueStorage(IOptions<CommandQueueStorageReaderOptions> options,
            IJsonConverter jsonConverter,
            ISecurityService securityService)
            : base(options.Value.ConnectionString, options.Value.Name
                , jsonConverter, securityService) {
        }

        public QueueStorage(string storageConnectionString,
            string storageName,
            IJsonConverter jsonConverter,
            ISecurityService securityService)
            : base(storageConnectionString, storageName, jsonConverter, securityService) {
        }

        protected override string CreateFileName(TWrapper @object) {
            return $"{@object.Id}.json";
        }

        protected override Task<CloudFileDirectory> GetDestinationFolder(CloudFileShare fileShare) {
            return Task.FromResult(fileShare.GetRootDirectoryReference());
        }

        public Task Save(TWrapper wrapper) {
            return SaveAsync(wrapper);
        }

        public Task<TWrapper> Retrieve(string filename, bool deleteAfterRead) {
            return RetrieveAsync($"{filename}.json", deleteAfterRead);
        }

    }
}