using EntityFrameworkCore.Triggers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.ChangesCollectorModule.Core;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ChangesCollectorModule.Data
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDbTriggers(this IApplicationBuilder appBuilder)
        {
            var lastChangesService = appBuilder.ApplicationServices.GetRequiredService<ILastChangesService>();

            Triggers<IEntity>.Inserting += entry =>
            {
                lastChangesService.SetLastModified(entry.Entity);
            };

            Triggers<IEntity>.Updating += entry =>
            {
                lastChangesService.SetLastModified(entry.Entity);
            };

            return appBuilder;
        }
    }
}
