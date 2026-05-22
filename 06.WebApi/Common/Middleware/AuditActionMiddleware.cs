/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Audit Action Tracking Middleware
*/

using System.Diagnostics;
using System.Security.Claims;
using Smlp.Domain.Entities;
using Smlp.Application.Services.Persistence;

namespace Smlp.WebApi.Common.Middleware;

/// <summary>
/// Middleware yang mencatat aksi HTTP (POST/PUT/DELETE) ke audit log secara otomatis.
/// </summary>
public class AuditActionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditActionMiddleware> _logger;

    private static readonly HashSet<string> _excludedPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health", "/swagger", "/favicon", "/_blazor", "/_framework", "/hubs"
    };

    private static readonly HashSet<string> _auditedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST", "PUT", "PATCH", "DELETE"
    };

    public AuditActionMiddleware(RequestDelegate next, ILogger<AuditActionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;

        if (_excludedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase))
            || !_auditedMethods.Contains(method))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        int statusCode;
        try
        {
            await _next(context);
            statusCode = context.Response.StatusCode;
        }
        catch
        {
            statusCode = 500;
            throw;
        }
        finally
        {
            sw.Stop();
        }

        // Simpan audit hanya untuk respon sukses
        if (statusCode is >= 200 and < 500)
        {
            try
            {
                await SaveAuditAsync(context, method, path, statusCode, sw.ElapsedMilliseconds, serviceProvider);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gagal menyimpan audit HTTP log: {Method} {Path}", method, path);
            }
        }
    }

    private static async Task SaveAuditAsync(
        HttpContext context, string method, string path,
        int statusCode, long ms, IServiceProvider sp)
    {
        var user = context.User;
        var username = user.FindFirstValue(ClaimTypes.Name)
                    ?? user.FindFirstValue("name")
                    ?? "anonymous";
        var role = user.FindFirstValue(ClaimTypes.Role) ?? "unknown";
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        var actionType = method switch
        {
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "PartialUpdate",
            "DELETE" => "Delete",
            _ => method
        };

        var entityName = ExtractEntityName(path);
        var entityId = ExtractEntityId(path);

        var audit = new Audit
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.Now,
            CreatedBy = username,
            TableName = entityName,
            EntityName = entityName,
            EntityId = entityId,
            ActionType = actionType,
            ActionName = $"HTTP {method} {path} → {statusCode} ({ms}ms)",
            OldValues = null,
            NewValues = $"{{\"statusCode\":{statusCode},\"durationMs\":{ms},\"role\":\"{role}\"}}",
            ClientApplicationId = "WebAPI-Middleware",
            FromIpAddress = ip,
        };

        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetService<ISmlpDbContext>();
        if (db is null)
            return;

        db.Audits.Add(audit);
        await db.SaveChangesAsync("AuditActionMiddleware", cancellationToken: default);
    }

    private static string ExtractEntityName(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var entity = segments.FirstOrDefault(s =>
            !s.Equals("api", StringComparison.OrdinalIgnoreCase) &&
            !s.StartsWith("v", StringComparison.OrdinalIgnoreCase) &&
            !Guid.TryParse(s, out _)) ?? "Unknown";
        return char.ToUpper(entity[0]) + entity[1..];
    }

    private static Guid ExtractEntityId(string path)
    {
        foreach (var seg in path.Split('/').Reverse())
            if (Guid.TryParse(seg, out var id))
                return id;
        return Guid.Empty;
    }
}

public static class AuditActionMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditAction(this IApplicationBuilder app)
        => app.UseMiddleware<AuditActionMiddleware>();
}
