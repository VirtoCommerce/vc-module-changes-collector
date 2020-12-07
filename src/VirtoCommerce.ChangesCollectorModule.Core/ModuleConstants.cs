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

        public static class Security
        {
            public static class Permissions
            {
                public static class Scopes
                {
                    public const string Reset = "changes-collector:scopes:reset";
                }

                public static IEnumerable<string> AllPermissions
                {
                    get
                    {
                        yield return Scopes.Reset;
                    }
                }
            }
        }
    }
}
