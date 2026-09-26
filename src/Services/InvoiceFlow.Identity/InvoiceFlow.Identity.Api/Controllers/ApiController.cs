using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InvoiceFlow.Identity.Api;

public abstract class ApiController : ControllerBase
{
    protected string? _ipAddress;
    protected string? _deviceInfo;

    protected string? IpAddress
    {
        get
        {
            if (_ipAddress is string)
            {
                return _ipAddress;
            }

            var ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? HttpContext.Connection.RemoteIpAddress?.ToString();
            if (IPAddress.TryParse(ipAddress, out var ipa))
            {
                if (IPAddress.IsLoopback(ipa))
                {
                    return "localhost";
                }
            }
            return _ipAddress = ipAddress;
        }
    }
    protected string? DeviceInfo
    {
        get
        {
            if (_deviceInfo is string)
            {
                return _deviceInfo;
            }

            var deviceInfo = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(deviceInfo))
            {
                return "Unknown";
            }
            return _deviceInfo = deviceInfo;
        }
    }
}
