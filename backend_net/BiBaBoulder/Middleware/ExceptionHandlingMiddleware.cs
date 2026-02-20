using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;

namespace Thecell.Bibaboulder.BiBaBoulder.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _request;

    public ExceptionHandlingMiddleware(RequestDelegate request)
    {
        _request = request;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _request(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private async Task HandleException(HttpContext context, Exception ex)
    {
        switch (ex)
        {
            case AuthenticationException authenticationException:
                await HandleAuthenticationException(context, authenticationException);
                break;
            case AccessDeniedException accessDeniedException:
                await HandleAccessDeniedException(context, accessDeniedException);
                break;
            case DbUpdateConcurrencyException dbUpdateConcurrencyException:
                await HandleDbUpdateConcurrencyException(context, dbUpdateConcurrencyException);
                break;
            case NotFoundException notFoundException:
                await HandleNotFoundException(context, notFoundException);
                break;
            default:
                await HandleSystemException(context, ex);
                break;
        }
    }

    private Task HandleDbUpdateConcurrencyException(HttpContext context, DbUpdateConcurrencyException dbUpdateConcurrencyException)
    {
        context.Response.StatusCode = 409;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new { message = $"Concurrency Exception: {dbUpdateConcurrencyException.Entries}" });
    }

    private Task HandleAuthenticationException(HttpContext context, AuthenticationException authenticationException)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new { message = authenticationException.Message });
    }

    private Task HandleAccessDeniedException(HttpContext context, AccessDeniedException accessDeniedException)
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new { message = accessDeniedException.Message });
    }

    private Task HandleNotFoundException(HttpContext context, NotFoundException notFoundException)
    {
        context.Response.StatusCode = 404;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new { message = notFoundException.Message });
    }

    private Task HandleSystemException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new { message = ex.Message });
    }

}
