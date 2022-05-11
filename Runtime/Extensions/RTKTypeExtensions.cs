﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using RealityToolkit.Interfaces;
using RealityToolkit.Interfaces.Events;
using RealityToolkit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace RealityToolkit.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Type"/> instances.
    /// </summary>
    public static class RTKTypeExtensions
    {
        internal static Type FindMixedRealityServiceInterfaceType(this Type serviceType, Type interfaceType)
        {
            if (serviceType == null)
            {
                return null;
            }

            var returnType = interfaceType;

            if (typeof(IMixedRealitySystem).IsAssignableFrom(serviceType))
            {
                if (!ServiceInterfaceCache.TryGetValue(serviceType, out returnType))
                {
                    var types = serviceType.GetInterfaces();

                    for (int i = 0; i < types.Length; i++)
                    {
                        if (!typeof(IMixedRealityService).IsAssignableFrom(types[i]))
                        {
                            continue;
                        }

                        if (types[i] != typeof(IMixedRealityService) &&
                            types[i] != typeof(IMixedRealityDataProvider) &&
                            types[i] != typeof(IMixedRealityEventSystem) &&
                            types[i] != typeof(IMixedRealitySystem))
                        {
                            returnType = types[i];
                            break;
                        }
                    }

                    ServiceInterfaceCache.Add(serviceType, returnType);
                }
            }

            return returnType;
        }

        private static readonly Dictionary<Type, Type> ServiceInterfaceCache = new Dictionary<Type, Type>();

        /// <summary>
        /// Checks if the <see cref="IMixedRealityService"/> has any valid implementations.
        /// </summary>
        /// <typeparam name="T">The specific <see cref="IMixedRealityService"/> interface to check.</typeparam>
        /// <returns>True, if the project contains valid implementations of <see cref="T"/>.</returns>
        public static bool HasValidImplementations<T>() where T : IMixedRealityService
        {
            var concreteTypes = TypeCache.Current
                .Select(pair => pair.Value)
                .Where(type => typeof(T).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

            var isValid = concreteTypes.Any();

            if (!isValid)
            {
                Debug.LogError($"Failed to find valid implementations of {typeof(T).Name}");
            }

            return isValid;
        }
    }
}
