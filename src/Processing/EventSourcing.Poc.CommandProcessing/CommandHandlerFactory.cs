﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EventSourcing.Poc.EventSourcing.Command;
using EventSourcing.Poc.EventSourcing.Exceptions;
using EventSourcing.Poc.EventSourcing.Utils;
using EventSourcing.Poc.Messages;

namespace EventSourcing.Poc.CommandProcessing {
    public class CommandHandlerFactory {
        private static IReadOnlyDictionary<Type, Type> _commandToCommandHandlers;
        private readonly IServiceProvider _serviceProvider;

        static CommandHandlerFactory()
        {
            _commandToCommandHandlers = new Dictionary<Type, Type>();
        }

        public CommandHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static void AddCommandHandler(Assembly assembly)
        {
            var newMapping =
                HandlerFactoryHelper.CreateHandler<CommandHandlerAttribute>(assembly, typeof(ICommandHandler<>));
            _commandToCommandHandlers = _commandToCommandHandlers.Concat(newMapping)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public ICommandHandler<TCommand> Resolve<TCommand>() where TCommand : ICommand
        {
            if (_commandToCommandHandlers.ContainsKey(typeof(TCommand)))
            {
                var commandHandler = _commandToCommandHandlers[typeof(TCommand)];
                return _serviceProvider.GetService(commandHandler) as ICommandHandler<TCommand>;
            }
            throw new NoCommandHandlerRegisteredException();
        }
    }
}