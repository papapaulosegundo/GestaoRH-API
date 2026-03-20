using GestaoRH.Middlewares;
using GestaoRH.Repositories;
using GestaoRH.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

const string MyCors = "_myCors";
builder.Services.AddCors(opts =>
{
    opts.AddPolicy(MyCors, p =>
        p.WithOrigins("http://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<SetorService>();
builder.Services.AddScoped<FuncionarioService>();
builder.Services.AddScoped<ModeloService>();   // novo

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors(MyCors);
app.MapOpenApi();

if (app.Environment.IsDevelopment())
    app.MapScalarApiReference();

app.UseRouting();
app.UseMiddleware<Auth>();
app.UseAuthorization();
app.MapControllers();
app.Run();
