using GestaoRH.Middlewares;
using GestaoRH.Repositories;
using GestaoRH.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── CORS ────────────────────────────────────────────────────────────────────
const string MyCors = "_myCors";
builder.Services.AddCors(opts =>
{
    opts.AddPolicy(MyCors, p =>
        p.WithOrigins("http://localhost:5173")   // URL do React em dev
         .AllowAnyHeader()
         .AllowAnyMethod());
});

// ── Controllers & OpenAPI ───────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ── Injeção de dependência ──────────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<SetorService>();
builder.Services.AddScoped<FuncionarioService>();

// ── Build ───────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors(MyCors);

app.MapOpenApi();

if (app.Environment.IsDevelopment())
    app.MapScalarApiReference();   // UI de testes em /scalar

app.UseRouting();

app.UseMiddleware<Auth>();         // JWT manual (igual ao ThorInc)

app.UseAuthorization();

app.MapControllers();

app.Run();
