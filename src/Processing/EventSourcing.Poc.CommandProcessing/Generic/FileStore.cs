using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Utils;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

namespace EventSourcing.Poc.Processing.Generic {
    public abstract class FileStore<TObject> where TObject : class {
        private readonly IJsonConverter _jsonConverter;
        private readonly CloudFileShare _fileShare;

        protected FileStore(string storageConnectionString, string storageName, IJsonConverter jsonConverter) {
            _jsonConverter = jsonConverter;
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var fileClient = storageAccount.CreateCloudFileClient();
            _fileShare = fileClient.GetShareReference(storageName);
            _fileShare.CreateIfNotExistsAsync();
        }

        protected async Task SaveAsync(TObject objectToStore) {
            await Upload(objectToStore);
        }

        protected async Task SaveAsync(IReadOnlyCollection<TObject> objectToStores) {
            var destinationFolder = await GetDestinationFolder(_fileShare);
            foreach (var objectToStore in objectToStores) {
                await Upload(destinationFolder, objectToStore);
            }
        }

        protected async Task<TObject> RetrieveAsync(string filename, bool deleteAfterRead = false)
        {
            var folder = await GetDestinationFolder(_fileShare);
            var file = folder.GetFileReference(filename);
            var content = await file.DownloadTextAsync();
            var fileContent = _jsonConverter.Deserialize<FileContent>(content);
            var outputType = Type.GetType(fileContent.Type);
            var @object = ConvertToObject(fileContent.Content.FromBase64(), outputType);
            if (deleteAfterRead)
            {
                await file.DeleteAsync();
            }
            return @object;
        }

        private async Task Upload(TObject objectToStore)
        {
            var folder = await GetDestinationFolder(_fileShare);
            await Upload(folder, objectToStore);
        }

        private async Task Upload(CloudFileDirectory destinationFolder, TObject objectToStore) {
            var fileName = CreateFileName(objectToStore);
            var file = destinationFolder.GetFileReference(fileName);
            var jsonContent = ConvertToJson(objectToStore);
            var fileContent = FileContent.Create(objectToStore, jsonContent);
            await file.UploadTextAsync(_jsonConverter.Serialize(fileContent));
        }

        protected virtual string ConvertToJson(TObject objectToStore) {
            return _jsonConverter.Serialize(objectToStore);
        }

        protected virtual TObject ConvertToObject(string objectInJson, Type outputType) {
            return _jsonConverter.Deserialize(objectInJson, outputType) as TObject;
        }

        protected abstract Task<CloudFileDirectory> GetDestinationFolder(CloudFileShare fileShare);
        protected abstract string CreateFileName(TObject @object);

        private class FileContent {
            public string Type { get; set; }
            public string Content { get; set; }

            public static FileContent Create(TObject objectToStore, string jsonContent) {
                var objectType = objectToStore.GetType().GetTypeInfo();
                return new FileContent {
                    Type = $"{objectType.FullName}, {objectType.Assembly.FullName}",
                    Content = jsonContent.ToBase64()
                };
            }

        }
    }
}