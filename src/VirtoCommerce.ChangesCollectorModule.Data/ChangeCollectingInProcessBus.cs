using System;
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

        private readonly Dictionary<Type, string> _moduleNameByTypeCache = new Dictionary<Type, string>();

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
            var module = GetModuleByType(@event.GetType());
            ChangesCollectorCacheRegion.ExpireTokenForKey(module);

            if (!EventSuppressor.EventsSuppressed && _routes.TryGetValue(@event.GetType(), out var handlers))
            {
                await Task.WhenAll(handlers.Select(handler => handler(@event, cancellationToken)));
            }
        }

        public DateTimeOffset GetLastModified(string module)
        {
            var cacheKey = CacheKey.With(GetType(), "LastModifiedDateTime", module);
            return PlatformCache.GetOrCreateExclusive(cacheKey, (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(ChangesCollectorCacheRegion.CreateChangeTokenForKey(module));
                return DateTimeOffset.Now;
            });
        }

        private string GetModuleByType(Type type)
        {
            if (!_moduleNameByTypeCache.ContainsKey(type))
            {

                var module = ModuleCatalog?.Modules.Where(module =>
                    {
                        return module.Assembly.GetReferencedAssemblies()
                            .Select(assemlyName => Assembly.Load(assemlyName))
                            .SelectMany(assembly => assembly.GetTypes())
                            .Contains(type);
                    }
                    ).FirstOrDefault(moduleInfo => type.Assembly.GetName().Name.Contains(moduleInfo.Assembly.GetName().Name.Replace(".Web", "")));

                if (module is null) return "Platform"; //No module found, seems it is the platform

                _moduleNameByTypeCache.Add(type, module.ModuleName);
            }
            return _moduleNameByTypeCache[type];
        }
    }

}
