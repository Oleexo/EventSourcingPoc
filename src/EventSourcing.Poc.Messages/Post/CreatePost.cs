namespace EventSourcing.Poc.Messages.Post {
    public class CreatePost : ICommand {
        public CreatePost(string title, string content) {
            Title = title;
            Content = content;
        }

        public string Title { get; }
        public string Content { get; }
    }
}