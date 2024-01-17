using Microsoft.AspNetCore.Mvc;
using Services.InvestTrackingAPI.Models.Dto;

namespace Services.InvestTrackingAPI.Controllers;

public static class PingEndpoints
{
    public static void MapPingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/ping", Ping);
    }

    public static ResponseDto Ping()
    {
        return new ResponseDto{IsSuccess=true, Message="The server is running"};
    }
}