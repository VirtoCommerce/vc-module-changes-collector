using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.ChangesCollectorModule.Core;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Messages;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.ChangesCollectorModule.Data
{
    public class ChangeCollectingInProcessBus : IEventPublisher, IHandlerRegistrar, ILastChangesService
    {
        private readonly Dictionary<Type, List<Func<IMessage, CancellationToken, Task>>> _routes = new Dictionary<Type, List<Func<IMessage, CancellationToken, Task>>>();

        private readonly Dictionary<Type, MethodInfo> _gettersCacheChangedEntries = new Dictionary<Type, MethodInfo>();
        private readonly Dictionary<Type, MethodInfo> _gettersCacheNewEntry = new Dictionary<Type, MethodInfo>();

        public IPlatformMemoryCache PlatformCache { get; set; }
        public IModuleCatalog ModuleCatalog { get; set; }

        public void RegisterHandler<T>(Func<T, CancellationToken, Task> handler) where T : class, IMessage
        {
            if (!_routes.TryGetValue(typeof(T), out var handlers))
            {
                handlers = new List<Func<IMessage, CancellationToken, Task>>();
                _routes.Add(typeof(T), handlers);
            }
            handlers.Add((message, token) => handler((T)message, token));
        }

        public async Task Publish<T>(T @event, CancellationToken cancellationToken = default(CancellationToken)) where T : class, IEvent
        {
            var modelTypeNames = GetModelsNamesByEventType(@event);

            foreach (var modelTypeName in modelTypeNames)
            {
                ChangesCollectorCacheRegion.ExpireTokenForKey(modelTypeName);
            }

            if (!EventSuppressor.EventsSuppressed && _routes.TryGetValue(@event.GetType(), out var handlers))
            {
                await Task.WhenAll(handlers.Select(handler => handler(@event, cancellationToken)));
            }
        }

        public DateTimeOffset GetLastModified(string modelTypeName)
        {
            var cacheKey = CacheKey.With(GetType(), "LastModifiedDateTime", modelTypeName);
            return PlatformCache.GetOrCreateExclusive(cacheKey, (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(ChangesCollectorCacheRegion.CreateChangeTokenForKey(modelTypeName));
                return DateTimeOffset.UtcNow;
            });
        }


        private List<string> GetModelsNamesByEventType<T>(T @event) where T : class, IEvent
        {
            var result = new List<string>();

            if (@event.GetType().BaseType.IsSubclassOf(typeof(DomainEvent))) // Is it GenericChangedEntryEvent ?
            {
                var eventType = @event.GetType();
                if (!_gettersCacheChangedEntries.ContainsKey(eventType)) // Just remember getter to eliminate reflection slowness
                {
                    _gettersCacheChangedEntries.Add(eventType, eventType.GetProperty("ChangedEntries").GetGetMethod());
                }

                var changedEntries = (IEnumerable)_gettersCacheChangedEntries[eventType].Invoke(@event, null);

                foreach (var changedEntry in changedEntries)
                {
                    var changedEntryType = changedEntry.GetType();
                    if (!_gettersCacheNewEntry.ContainsKey(changedEntryType)) // Just remember getter to eliminate reflection slowness
                    {
                        _gettersCacheNewEntry.Add(changedEntryType, changedEntryType.GetProperty("NewEntry").GetGetMethod());
                    }
                    var changedEntryTypeName = _gettersCacheNewEntry[changedEntryType].Invoke(changedEntry, null).GetType().FullName;
                    result.Add(changedEntryTypeName);
                }

            }

            return result;
        }

        /*
        private string GetModuleNameByEventType(Type eventType)
        {
            if (!_moduleNameByTypeCache.ContainsKey(eventType))
            {

                var module = ModuleCatalog?.Modules.Where(module =>
                    {
                        return module.Assembly.GetReferencedAssemblies()
                            .Select(assemlyName => Assembly.Load(assemlyName))
                            .SelectMany(assembly => assembly.GetTypes())
                            .Contains(eventType);
                    }
                    ).FirstOrDefault(moduleInfo => eventType.Assembly.GetName().Name.Contains(moduleInfo.Assembly.GetName().Name.Replace(".Web", "")));

                if (module is null) return "Platform"; //No module found, seems it is the platform

                _moduleNameByTypeCache.Add(eventType, module.ModuleName);
            }
            return _moduleNameByTypeCache[eventType];
        }
        */
    }

}
