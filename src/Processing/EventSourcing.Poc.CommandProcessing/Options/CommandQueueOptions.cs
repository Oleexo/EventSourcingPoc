namespace EventSourcing.Poc.CommandProcessing.Options {
    public class CommandQueueOptions {
        public string QueueConnectionString { get; set; }
        public string FileShareConnectionString { get; set; }
        public string QueueName { get; set; }
        public string FileShareName { get; set; }
    }
}