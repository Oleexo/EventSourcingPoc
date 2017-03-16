using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.StorageAccount {
    public class StorageTableClient {
        private readonly CloudTableClient _cloudTableClient;

        public StorageTableClient(string connectionString) {
            _cloudTableClient = CloudStorageAccount.Parse(connectionString)
                .CreateCloudTableClient();
        }

        public TableClient GetTable(string name) {
            return new TableClient(_cloudTableClient.GetTableReference(name));
        }
    }
}