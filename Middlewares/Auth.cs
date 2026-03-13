using GestaoRH.Utils;

namespace GestaoRH.Middlewares;

public class Auth
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    // Rotas públicas — não exigem token
    private static readonly HashSet<string> RotasPublicas = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/empresa/cadastrar",
        "/api/empresa/login"
    };

    public Auth(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = (context.Request.Path.Value ?? string.Empty)
            .TrimEnd('/')
            .ToLowerInvariant();

        if (RotasPublicas.Contains(path))
        {
            await _next(context);
            return;
        }

        // Protege qualquer rota /api/*
        if (path.StartsWith("/api/"))
        {
            var header = context.Request.Headers["Authorization"].FirstOrDefault();
            var token = (header?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ?? false)
                ? header["Bearer ".Length..].Trim()
                : null;

            if (string.IsNullOrWhiteSpace(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token não informado.");
                return;
            }

            var principal = Jwt.ValidateToken(token, _config);
            if (principal is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token inválido ou expirado.");
                return;
            }

            context.Items["Claims"] = principal;
        }

        await _next(context);
    }
}
