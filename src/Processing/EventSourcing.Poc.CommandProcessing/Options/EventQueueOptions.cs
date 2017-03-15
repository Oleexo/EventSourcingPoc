namespace EventSourcing.Poc.Processing.Options {
    public class EventQueueOptions {
        public string QueueConnectionString { get; set; }
        public string FileShareConnectionString { get; set; }
        public string QueueName { get; set; }
        public string FileShareName { get; set; }
    }
}