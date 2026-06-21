using System.Text.Json;
using EventGate.Api.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace EventGate.Api.Api.Middleware;

/// <summary>
/// Tratamento global de erros. Traduz AppException no status correto e devolve
/// um ProblemDetails. Para erros inesperados, responde 500 genérico — nunca
/// vaza stack trace nem detalhes internos ao cliente.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            await WriteProblemAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado em {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError,
                "Ocorreu um erro inesperado.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonFor(statusCode),
            Detail = detail
        };

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }

    private static string ReasonFor(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Requisição inválida",
        StatusCodes.Status401Unauthorized => "Não autorizado",
        StatusCodes.Status404NotFound => "Não encontrado",
        StatusCodes.Status409Conflict => "Conflito",
        _ => "Erro interno"
    };
}
