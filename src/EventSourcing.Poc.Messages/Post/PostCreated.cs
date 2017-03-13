namespace EventSourcing.Poc.Messages.Post {
    public class PostCreated : IEvent {
        public PostCreated(string title, string content)
        {
            Title = title;
            Content = content;
        }

        public string Title { get; }
        public string Content { get; }
    }
}