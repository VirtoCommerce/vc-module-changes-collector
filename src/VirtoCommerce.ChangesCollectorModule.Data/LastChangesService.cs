using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VirtoCommerce.ChangesCollectorModule.Core;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ChangesCollectorModule.Data
{
    public class LastChangesService : ILastChangesService
    {
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly Dictionary<string, List<string>> _scopes;

        public LastChangesService(IPlatformMemoryCache platformMemoryCache, ISettingsManager settingsManager)
        {
            _platformMemoryCache = platformMemoryCache;

            var scopesJson = settingsManager.GetValue(ModuleConstants.Settings.General.Scopes.Name, "");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(scopesJson));
            _scopes = stream.DeserializeJson<Dictionary<string, List<string>>>();
            _scopes ??= new Dictionary<string, List<string>>();
        }

        public IEnumerable<string> GetAllScopes()
        {
            return _scopes.Keys;
        }

        public virtual DateTimeOffset GetLastModified(string scopeName)
        {
            if (!scopeName.IsNullOrEmpty())
            {
                var cacheKey = CacheKey.With(GetType(), "LastModifiedDateTime", scopeName);
                return _platformMemoryCache.GetOrCreateExclusive(cacheKey, (cacheEntry) =>
                {
                    cacheEntry.AddExpirationToken(ChangesCollectorCacheRegion.CreateChangeTokenForKey(scopeName));
                    return DateTimeOffset.UtcNow;
                });
            }
            else
            {
                return DateTimeOffset.MinValue;
            }
        }

        public virtual void SetLastModified(IEntity entry)
        {
            var scopeName = _scopes.FirstOrDefault(x => x.Value.Contains(entry.GetType().FullName)).Key;
            if (!scopeName.IsNullOrEmpty())
            {
                ChangesCollectorCacheRegion.ExpireTokenForKey(scopeName);
            }
        }
    }
}
