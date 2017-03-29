namespace EventSourcing.Poc.Processing.Options {
    public class CommandQueueStorageReaderOptions {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
    }
}