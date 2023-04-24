using CorePlugin.Plugin.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorePlugin.Plugin.Controllers;

#warning This is a sample controller, should be deleted in your plugin.
[ApiController]
[Route("[controller]/[action]")]
public class SampleController : ControllerBase
{
    [HttpGet]
    public ActionResult<SampleDto> Test() => Ok(new SampleDto { Description = "Hello World!" });

    [HttpGet]
    [Authorize]
    public ActionResult<SampleDto> AuthorizeTest() => Ok(new SampleDto { Description = "Hello World!" });
}
