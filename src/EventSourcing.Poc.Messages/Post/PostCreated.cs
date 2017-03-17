using System;

namespace EventSourcing.Poc.Messages.Post {
    public class PostCreated : IEvent {
        public PostCreated(Guid id, string title, string content) {
            Id = id;
            Title = title;
            Content = content;
        }

        public Guid Id { get; set; }
        public string Title { get; }
        public string Content { get; }
    }

    public class AddToCategory : IAction {
        public Guid Id { get; set; }
        public string Category { get; set; }
    }

    public class AddedToCategory : IEvent {
        public Guid Id { get; set; }
        public string Category { get; set; }
    }
}