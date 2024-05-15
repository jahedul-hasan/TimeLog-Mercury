﻿using MercuryTimeLog.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace MercuryTimeLog.API.Controllers;

[Route("admin/api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected new IActionResult Ok()
    {
        return base.Ok(Envelope.Ok());
    }

    protected IActionResult Ok<T>(T result)
    {
        return base.Ok(Envelope.Ok(result));
    }

    protected IActionResult Error(string errorMessage)
    {
        return BadRequest(Envelope.Error(errorMessage));
    }
}
