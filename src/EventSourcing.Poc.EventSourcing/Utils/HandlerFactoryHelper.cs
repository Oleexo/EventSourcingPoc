using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventSourcing.Poc.EventSourcing.Utils {
    public class HandlerFactoryHelper {
        public static IReadOnlyDictionary<Type, Type> CreateHandler<THandlerAttribute>(Assembly assembly, Type cht) {
            var attributeType = typeof(THandlerAttribute);
            var result = new Dictionary<Type, Type>();
            var commandHandlerTypes = from type in assembly.ExportedTypes
                let attributes = type.GetTypeInfo().GetCustomAttributes(attributeType, true)
                where attributes != null && attributes.Any()
                select type;
            foreach (var commandHandlerType in commandHandlerTypes) {
                var interfaces = commandHandlerType.GetTypeInfo()
                    .ImplementedInterfaces
                    .Where(i => i.GetTypeInfo().IsGenericType)
                    .Where(i => cht.GetTypeInfo().IsAssignableFrom(i.GetGenericTypeDefinition().GetTypeInfo()));
                foreach (var @interface in interfaces) {
                    var commandType = @interface.GenericTypeArguments[0];
                    result.Add(commandType, commandHandlerType);
                }
            }
            return result;
        }
    }
}