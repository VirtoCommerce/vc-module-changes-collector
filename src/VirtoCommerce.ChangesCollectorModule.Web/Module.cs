using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.ChangesCollectorModule.Core;
using VirtoCommerce.ChangesCollectorModule.Data;
using VirtoCommerce.ChangesCollectorModule.Web.Extensions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ChangesCollectorModule.Web
{
    public class Module : IModule
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ILastChangesService, LastChangesService>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            // Register settings and permissions
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            var permissionsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsRegistrar.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x => new Permission
            {
                Name = x,
                GroupName = "ChangesCollector",
                ModuleId = ModuleInfo.Id,
            }).ToArray());

            appBuilder.UseDbTriggers();

            LoadScopes(appBuilder.ApplicationServices).GetAwaiter().GetResult();
        }

        public void Uninstall()
        {
            //Nothing special here
        }


        private static async Task LoadScopes(IServiceProvider serviceProvider)
        {
            var hostingEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            var scopesFilePath = hostingEnvironment.MapPath("~/changes-collector-scopes.json");

            string serializedScopes;

            if (File.Exists(scopesFilePath))
            {
                serializedScopes = await File.ReadAllTextAsync(scopesFilePath);
            }
            else
            {
                var settingsManager = serviceProvider.GetRequiredService<ISettingsManager>();
                serializedScopes = await settingsManager.GetValueAsync(ModuleConstants.Settings.General.Scopes.Name, string.Empty);
            }

            IDictionary<string, IList<string>> scopes;

            await using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(serializedScopes)))
            {
                scopes = stream.DeserializeJson<Dictionary<string, IList<string>>>() ?? new Dictionary<string, IList<string>>();
            }

            var lastChangesService = serviceProvider.GetRequiredService<ILastChangesService>();
            lastChangesService.LoadScopes(scopes);
        }
    }
}
