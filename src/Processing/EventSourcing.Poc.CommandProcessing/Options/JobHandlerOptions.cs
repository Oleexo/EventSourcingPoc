namespace EventSourcing.Poc.Processing.Options {
    public class JobHandlerOptions {
        public string ConnectionString { get; set; }
        public string JobTableName { get; set; }
        public string CommandTableName { get; set; }
        public string EventTableName { get; set; }
        public string ActionTableName { get; set; }
        public string ArchiveStorageName { get; set; }
        public string ArchiveTableName { get; set; }
    }
}