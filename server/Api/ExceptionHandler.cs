using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api;

public class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var problemDetails = new ProblemDetails()
        {
            Title = exception.Message
        };
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails);
        
        return true;
    }
}