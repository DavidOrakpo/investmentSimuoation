using investmentSimuoation.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Register application domain services (singleton in-memory state for mock simulation)
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<IInvestmentService, InvestmentService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure OpenAPI & Scalar documentation
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Invest Now API Documentation")
               .WithTheme(ScalarTheme.Moon);
    });
}

// Note: Avoid enforcing HTTPS redirection in Development to allow seamless Flutter Android Emulator (http://10.0.2.2:5140)
// and iOS simulator testing without self-signed SSL certificate trust issues.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
