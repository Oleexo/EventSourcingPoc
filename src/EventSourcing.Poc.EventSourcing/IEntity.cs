namespace EventSourcing.Poc.EventSourcing {
    public interface IEntity {
    }

    public interface IEntity<TKey> : IEntity {
        TKey Id { get; set; }
    }
}