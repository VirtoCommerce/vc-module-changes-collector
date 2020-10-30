using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.ChangesCollectorModule.Core;
using VirtoCommerce.ChangesCollectorModule.Web.Model;

namespace VirtoCommerce.ChangesCollectorModule.Web.Controllers.Api
{
    [Authorize]
    public class ChangesCollectorController : Controller
    {
        private readonly ILastChangesService _lastChangesService;

        public ChangesCollectorController(ILastChangesService lastChangesService)
        {
            _lastChangesService = lastChangesService;
        }

        [HttpGet]
        [Route("~/api/changescollector/lastmodifieddate")]
        [AllowAnonymous]
        public ActionResult<LastModifiedResponse> GetLastModifiedDate([FromQuery] string module = null)
        {
            var result = new LastModifiedResponse
            {
                Scope = module,
                LastModifiedDate = _lastChangesService.GetLastModified(module).UtcDateTime
            };

            return Ok(result);
        }

    }
}
