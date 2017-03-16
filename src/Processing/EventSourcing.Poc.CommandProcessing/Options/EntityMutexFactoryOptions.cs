namespace EventSourcing.Poc.Processing.Options {
    public class EntityMutexFactoryOptions {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
    }
}