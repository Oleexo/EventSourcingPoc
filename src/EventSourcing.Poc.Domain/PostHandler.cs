using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Poc.EventSourcing.Command;
using EventSourcing.Poc.EventSourcing.Event;
using EventSourcing.Poc.EventSourcing.Mutex;
using EventSourcing.Poc.Messages;
using EventSourcing.Poc.Messages.Post;

namespace EventSourcing.Poc.Domain {
    [CommandHandler]
    [EventHandler]
    public class PostHandler : ICommandHandler<CreatePost>,
        IEventHandler<PostCreated> {
        private readonly IEntityMutexFactory _entityMutexFactory;

        public PostHandler(IEntityMutexFactory entityMutexFactory) {
            _entityMutexFactory = entityMutexFactory;
        }

        public async Task<IReadOnlyCollection<IEvent>> Handle(CreatePost command) {
            var author = new Author {
                Id = Guid.NewGuid(),
                Firstname = "Anonymous",
                Lastname = "Anonymous"
            };
            using (var authorMutex = _entityMutexFactory.Create(author)) {
                await authorMutex.LockAsync();
                Console.WriteLine("handle create post command.");
                return new List<IEvent> {
                    new PostCreated(command.Title, command.Content)
                };
            }
        }

        public Task<IReadOnlyCollection<IAction>> Handle(PostCreated @event) {
            throw new NotImplementedException();
        }
    }
}