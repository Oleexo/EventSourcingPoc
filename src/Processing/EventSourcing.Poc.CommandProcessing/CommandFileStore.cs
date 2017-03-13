using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.EventSourcing.Wrapper;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

namespace EventSourcing.Poc.CommandProcessing {
    public abstract class CommandFileStore {
        private readonly CloudFileShare _fileShare;
        private readonly IJsonConverter _jsonConverter;

        protected CommandFileStore(string connectionString, string name, IJsonConverter jsonConverter) {
            _jsonConverter = jsonConverter;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var fileClient = storageAccount.CreateCloudFileClient();
            _fileShare = fileClient.GetShareReference(name);
            _fileShare.CreateIfNotExistsAsync();
        }

        protected CommandFileStore(CloudFileShare fileShare, IJsonConverter jsonConverter) {
            _fileShare = fileShare;
            _jsonConverter = jsonConverter;
            _fileShare.CreateIfNotExistsAsync();
        }

        protected async Task SaveAsync(ICommandWrapper commandWrapped) {
            var model = CommandStoredModel.Create(commandWrapped, _jsonConverter);
            await Upload(model);
        }

        protected async Task SaveAsync(IReadOnlyCollection<ICommandWrapper> commandWrappeds) {
            var storedModels = commandWrappeds.Select(cw => CommandStoredModel.Create(cw, _jsonConverter));
            var destinationFolder = await GetDestinationFolder(_fileShare);
            foreach (var commandStoredModel in storedModels) {
                await Upload(destinationFolder, commandStoredModel);
            }
        }

        protected async Task<ICommandWrapper> RetrieveAsync(Guid id, bool deleteAfterRead = false) {
            var fileName = $"{id}.json";
            var folder = await GetDestinationFolder(_fileShare);
            var file = folder.GetFileReference(fileName);
            var content = await file.DownloadTextAsync();
            var storedModel = _jsonConverter.Deserialize<CommandStoredModel>(content);
            var outputType = Type.GetType(storedModel.Type);
            var commandWrapper = _jsonConverter.Deserialize(storedModel.Command.FromBase64(), outputType) as ICommandWrapper;
            if (deleteAfterRead) {
                await file.DeleteAsync();
            }
            return commandWrapper;
        }

        private async Task Upload(CommandStoredModel storedModel) {
            var folder = await GetDestinationFolder(_fileShare);
            await Upload(folder, storedModel);
        }

        private async Task Upload(CloudFileDirectory destinationFolder, CommandStoredModel storedModel) {
            var fileName = $"{storedModel.Id}.json";
            var file = destinationFolder.GetFileReference(fileName);
            await file.UploadTextAsync(_jsonConverter.Serialize(storedModel));
        }

        public abstract Task<CloudFileDirectory> GetDestinationFolder(CloudFileShare fileShare);

        private class CommandStoredModel {
            public string Type { get; set; }
            public string SubType { get; set; }
            public string Command { get; set; }
            public Guid Id { get; set; }

            public static CommandStoredModel Create(ICommandWrapper commandWrapped, IJsonConverter jsonConverter) {
                var commandTypeInfo = commandWrapped.GetType().GetTypeInfo();
                var genericArgument = commandTypeInfo.GetGenericArguments()[0].GetTypeInfo();
                return new CommandStoredModel {
                    Id = commandWrapped.Id,
                    Type = $"{commandTypeInfo.FullName}, {commandTypeInfo.Assembly.FullName}",
                    SubType = $"{genericArgument.FullName}, {genericArgument.Assembly.FullName}",
                    Command = jsonConverter.Serialize(commandWrapped).ToBase64()
                };
            }
        }
    }
}