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
        IEventHandler<PostCreated>,
        ICommandHandler<AddToCategory>,
        IEventHandler<AddedToCategory> {
        private readonly IEntityMutexFactory _entityMutexFactory;

        public PostHandler(IEntityMutexFactory entityMutexFactory) {
            _entityMutexFactory = entityMutexFactory;
        }

        public async Task<IReadOnlyCollection<IEvent>> Handle(AddToCategory command) {
            return new List<IEvent> {
                new AddedToCategory {
                    Id = command.Id,
                    Category = command.Category
                }
            };
        }

        public async Task<IReadOnlyCollection<IEvent>> Handle(CreatePost command) {
            var author = new Author {
                Id = Guid.Parse("10b7497e-6dfb-4add-a7eb-14c3b69feaad"),
                Firstname = "Anonymous",
                Lastname = "Anonymous"
            };
            await _entityMutexFactory.CreateAndLock(author);
            return new List<IEvent> {
                new PostCreated(Guid.NewGuid(), command.Title, command.Content)
            };
        }

        public async Task<IReadOnlyCollection<IAction>> Handle(AddedToCategory @event) {
            return new List<IAction>();
        }

        public async Task<IReadOnlyCollection<IAction>> Handle(PostCreated @event) {
            return new List<IAction> {
                new AddToCategory {
                    Id = @event.Id,
                    Category = "My Category"
                }
            };
        }
    }
}