using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VirtoCommerce.ChangesCollectorModule.Core;
using VirtoCommerce.ChangesCollectorModule.Data;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.ChangesCollectorModule.Web
{
    public class Module : IModule
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.RemoveAll<IHandlerRegistrar>();
            serviceCollection.RemoveAll<IEventPublisher>();

            var collectingInProcessBus = new ChangeCollectingInProcessBus();
            serviceCollection.AddSingleton<IHandlerRegistrar>(collectingInProcessBus);
            serviceCollection.AddSingleton<IEventPublisher>(collectingInProcessBus);
            serviceCollection.AddSingleton<ILastChangesService>(collectingInProcessBus);
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            var collectingInProcessBus = appBuilder.ApplicationServices.GetService<IHandlerRegistrar>();
            var platformMemoryCache = appBuilder.ApplicationServices.GetService<IPlatformMemoryCache>();
            var moduleCatalog = appBuilder.ApplicationServices.GetService<IModuleCatalog>();
            ((ChangeCollectingInProcessBus)collectingInProcessBus).PlatformCache = platformMemoryCache;
            ((ChangeCollectingInProcessBus)collectingInProcessBus).ModuleCatalog = moduleCatalog;
        }

        public void Uninstall()
        {
            //Nothing special here
        }
    }
}
