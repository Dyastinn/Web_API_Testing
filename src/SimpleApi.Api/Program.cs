using SimpleApi.Api.Middleware;
using SimpleApi.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IProductService, ProductService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();

public partial class Program { }
