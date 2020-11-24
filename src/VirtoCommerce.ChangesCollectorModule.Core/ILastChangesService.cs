using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ChangesCollectorModule.Core
{
    public interface ILastChangesService
    {
        public DateTimeOffset GetLastModified(string scopeName);
        public IEnumerable<string> GetAllScopes();
        public void SetLastModified(IEntity entry);
    }
}
