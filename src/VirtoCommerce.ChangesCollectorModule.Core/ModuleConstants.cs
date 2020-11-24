using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ChangesCollectorModule.Core
{
    public static class ModuleConstants
    {
        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor Scopes { get; } = new SettingDescriptor
                {
                    Name = "VirtoCommerce.ChangesCollectorModule.Scopes",
                    GroupName = "Scope-based changes collector|Scopes",
                    ValueType = SettingValueType.Json
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return Scopes;
                    }
                }
            }
        }
    }
}
