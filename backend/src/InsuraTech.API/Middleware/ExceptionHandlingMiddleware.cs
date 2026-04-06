using InsuraTech.Application.Common.Exceptions;
using InsuraTech.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InsuraTech.API.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, code, title) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "NOT_FOUND", "Resource not found"),
                InvalidPolicyStateException => (StatusCodes.Status409Conflict, "INVALID_STATE", "Invalid state transition"),
                InvalidClaimStateException => (StatusCodes.Status409Conflict, "INVALID_STATE", "Invalid state transition"),
                ClaimLimitExceededException => (StatusCodes.Status422UnprocessableEntity, "CLAIM_LIMIT", "Claim limit exceeded"),
                ClaimOnExpiredPolicyException => (StatusCodes.Status422UnprocessableEntity, "EXPIRED_POLICY", "Policy is expired"),
                UnderageInsuredException e => (StatusCodes.Status422UnprocessableEntity, e.Code, "Underage insured"),
                OverageInsuredException e => (StatusCodes.Status422UnprocessableEntity, e.Code, "Overage insured — requires preexisting conditions form"),
                InvalidHealthPlanException e => (StatusCodes.Status400BadRequest, e.Code, "Invalid health plan"),
                BusinessRuleException e => (StatusCodes.Status422UnprocessableEntity, e.Code, "Business rule violation"),
                DomainException e => (StatusCodes.Status400BadRequest, e.Code, "Domain error"),
                ArgumentException => (StatusCodes.Status400BadRequest, "INVALID_ARGUMENT", "Invalid argument"),
                _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred")
            };

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Extensions = { ["code"] = code }
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var json = JsonSerializer.Serialize(problem);
            await context.Response.WriteAsync(json);
        }

    }
}
