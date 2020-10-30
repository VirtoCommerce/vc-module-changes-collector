using System;

namespace VirtoCommerce.ChangesCollectorModule.Core
{
    public interface ILastChangesService
    {
        public DateTimeOffset GetLastModified(string module);
    }
}
