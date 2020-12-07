using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ChangesCollectorModule.Core
{
    public interface ILastChangesService
    {
        public void LoadScopes(IDictionary<string, IList<string>> scopes);
        public IEnumerable<string> GetAllScopes();
        public DateTimeOffset GetLastModified(string scopeName);
        public void SetLastModified(IEntity entry);
        public void ResetScope(string scopeName);
        public void ResetAllScopes();
    }
}
