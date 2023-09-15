// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.Pool;

namespace VirtualBeings
{
    /// <summary>
    /// Factory for getting the correct inspector decorator for the requested type , also takes care of caching the pre-created decorators
    /// </summary>
    [InitializeOnLoad]
    public static class InspectorDecoratorFactory
    {
        private static Dictionary<Type, IInspectorDecorator> _typeToDecoratorMap = new Dictionary<Type, IInspectorDecorator>();

        static InspectorDecoratorFactory()
        {
            Type baseInterface = typeof(IInspectorDecorator);

            IEnumerable<Type> query = TypeCache.GetTypesDerivedFrom(baseInterface)
                .Where(t => t.IsClass)
                .Where(t => !t.IsAbstract)
                .Where(t => !t.IsGenericTypeDefinition);


            foreach (Type decoratorType in query)
            {
                IInspectorDecorator instance = (IInspectorDecorator)Activator.CreateInstance(decoratorType);
                DecoratorForAttribute decorator = decoratorType.GetCustomAttribute<DecoratorForAttribute>(true);

                if (decorator == null)
                    continue;

                _typeToDecoratorMap.Add(decorator.type, instance);
            }
        }


        public static IInspectorDecorator GetDecorator(Type type)
        {
            Type defaultDecorator = typeof(DefaultInspectorDecorator<>);


            if (!_typeToDecoratorMap.TryGetValue(type, out IInspectorDecorator decorator))
            {
                using (ListPool<Type>.Get(out List<Type> baseTypes))
                {
                    baseTypes.AddRange(TypeUtils.GetAllBaseTypes(type));

                    for (int i = 1; i < baseTypes.Count; i++)
                    {
                        Type t = baseTypes[i];
                        if (_typeToDecoratorMap.TryGetValue(t, out decorator) && decorator.AcceptSubclasses)
                        {
                            return decorator;
                        }
                    }
                }

                Type concreteDefault = defaultDecorator.MakeGenericType(type);
                decorator = (IInspectorDecorator)Activator.CreateInstance(concreteDefault, true);

                _typeToDecoratorMap.Add(type, decorator);
            }

            return decorator;
        }
        public static IInspectorDecorator GetDecorator<T>()
        {
            Type t = typeof(T);

            return GetDecorator(t);
        }
    }
}