using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.ChangesCollectorModule.Core;
using VirtoCommerce.ChangesCollectorModule.Web.Model;

namespace VirtoCommerce.ChangesCollectorModule.Web.Controllers.Api
{
    [Authorize]
    [Route("api/last-modified-dates")]
    public class ChangesCollectorController : Controller
    {
        private readonly ILastChangesService _lastChangesService;

        public ChangesCollectorController(ILastChangesService lastChangesService)
        {
            _lastChangesService = lastChangesService;
        }

        [HttpGet]
        [Route("{scope}")]
        [AllowAnonymous]
        public ActionResult<LastModifiedResponse> GetLastModifiedDate([FromRoute] string scope = null)
        {
            var result = new LastModifiedResponse
            {
                Scope = scope,
                LastModifiedDate = _lastChangesService.GetLastModified(scope).UtcDateTime
            };

            return Ok(result);
        }

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public ActionResult<IList<LastModifiedResponse>> GetLastModifiedDateForAllScopes()
        {
            var allScopes = _lastChangesService.GetAllScopes();
            var result = new List<LastModifiedResponse>();

            foreach (var scope in allScopes)
            {
                result.Add(new LastModifiedResponse
                {
                    Scope = scope,
                    LastModifiedDate = _lastChangesService.GetLastModified(scope).UtcDateTime
                });
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("{scope}/reset")]
        [Authorize(ModuleConstants.Security.Permissions.Scopes.Reset)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public ActionResult ResetLastModifiedDate([FromRoute] string scope)
        {
            _lastChangesService.ResetScope(scope);

            return NoContent();
        }

        [HttpPost]
        [Route("reset")]
        [Authorize(ModuleConstants.Security.Permissions.Scopes.Reset)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public ActionResult ResetLastModifiedDates()
        {
            _lastChangesService.ResetAllScopes();

            return NoContent();
        }
    }
}
