using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

        protected async Task<string> SaveAsync(TObject objectToStore) {
            var destinationFolder = await GetDestinationFolder(_fileShare);
            
            await Upload(destinationFolder, objectToStore);
            return UriToString(destinationFolder.Uri);
        }

        private string UriToString(Uri uri) {
            var segments = uri.Segments.Skip(2);
            var sb = new StringBuilder();
            foreach (var segment in segments) {
                sb.Append(segment);
            }
            return sb.ToString();
        }

        protected async Task<string> SaveAsync(IReadOnlyCollection<TObject> objectToStores) {
            var destinationFolder = await GetDestinationFolder(_fileShare);
            foreach (var objectToStore in objectToStores) {
                await Upload(destinationFolder, objectToStore);
            }
            return UriToString(destinationFolder.Uri);
        }

        protected async Task<TObject> RetrieveAsync(string filename, string path, bool deleteAfterRead = false) {
            var folders = path.Split('/');
            var currentFolder = _fileShare.GetRootDirectoryReference();
            foreach (var folderName in folders) {
                currentFolder = currentFolder.GetDirectoryReference(folderName);
            }
            return await RetrieveAsync(filename, currentFolder, deleteAfterRead);
        }

        protected async Task<TObject> RetrieveAsync(string filename, bool deleteAfterRead = false)
        {
            var folder = await GetDestinationFolder(_fileShare);
            return await RetrieveAsync(filename, folder, deleteAfterRead);
        }

        private async Task<TObject> RetrieveAsync(string filename, CloudFileDirectory directory, bool deleteAfterRead) {
            var file = directory.GetFileReference(filename);
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