using Microsoft.AspNetCore.Mvc;
using SsoServer.Infrastructures;

namespace SsoServer.Controllers;

[RequiredHeader(Constants.Constant.X_REFERER_HOST)]
[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    
}