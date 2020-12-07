using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.ChangesCollectorModule.Core;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ChangesCollectorModule.Data
{
    public class LastChangesService : ILastChangesService
    {
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private IDictionary<string, IList<string>> _scopes = new Dictionary<string, IList<string>>();

        public LastChangesService(IPlatformMemoryCache platformMemoryCache)
        {
            _platformMemoryCache = platformMemoryCache;
        }

        public virtual void LoadScopes(IDictionary<string, IList<string>> scopes)
        {
            if (scopes?.Any() == true)
            {
                _scopes = scopes;
            }
        }

        public virtual IEnumerable<string> GetAllScopes()
        {
            return _scopes.Keys;
        }

        public virtual DateTimeOffset GetLastModified(string scopeName)
        {
            DateTimeOffset result;

            if (!scopeName.IsNullOrEmpty() && _scopes.ContainsKey(scopeName))
            {
                var cacheKey = CacheKey.With(GetType(), "LastModifiedDateTime", scopeName);
                result = _platformMemoryCache.GetOrCreateExclusive(cacheKey, options =>
                {
                    options.AddExpirationToken(ChangesCollectorCacheRegion.CreateChangeTokenForKey(scopeName));

                    return DateTimeOffset.UtcNow;
                });
            }
            else
            {
                // If the requested scope is not known to this module, do not use cached date - otherwise, it will never change, and data from that scope will get stuck in cache.
                // Instead, return the current date, as if target data had been modified just now.
                result = DateTimeOffset.UtcNow;
            }

            return result;
        }

        public virtual void SetLastModified(IEntity entry)
        {
            var scopeNames = _scopes
                .Where(x => x.Value.Contains(entry.GetType().FullName))
                .Select(x => x.Key)
                .ToList();

            ExpireLastModifiedDateForScopes(scopeNames);
        }

        public virtual void ResetScope(string scopeName)
        {
            if (!scopeName.IsNullOrEmpty())
            {
                ExpireLastModifiedDateForScopes(new[] { scopeName });
            }
        }

        public virtual void ResetAllScopes()
        {
            var scopeNames = GetAllScopes();

            ExpireLastModifiedDateForScopes(scopeNames);
        }


        protected virtual void ExpireLastModifiedDateForScopes(IEnumerable<string> scopeNames)
        {
            foreach (var scopeName in scopeNames)
            {
                ChangesCollectorCacheRegion.ExpireTokenForKey(scopeName);
            }
        }
    }
}
