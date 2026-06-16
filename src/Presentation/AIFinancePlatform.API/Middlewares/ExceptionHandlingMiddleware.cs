using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AIFinancePlatform.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bir hata oluştu: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // 1. DÜZELTME: Bütün patlamalar (Exceptions) 500 İç Sunucu Hatasıdır.
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // 2. DÜZELTME: Siber güvenlik için exception.Message sokağa ASLA açılmaz.
        var result = JsonSerializer.Serialize(new 
        { 
            isSuccess = false,
            message = "Sistemde beklenmeyen teknik bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."
            // exception.Message'ı burada siliyoruz ki veritabanı yolları veya şifreler dışarı sızmasın!
        });
        
        return context.Response.WriteAsync(result);
    }
}
