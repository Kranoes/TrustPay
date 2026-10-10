using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using TrustPay.Api.Logging;
using TrustPay.Application;
using TrustPay.Application.Common.Authentication.Commands.Login;
using TrustPay.Infrastructure;
using TrustPay.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
    configuration.WriteTo.Console();
    configuration.Destructure.With<SensitiveDestructuringPolicy>();
});
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Введите JWT токен"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = securityScheme;

        var securityRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        };

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(securityRequirement);

        return Task.CompletedTask;
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        await DbInitializer.SeedAsync(scope.ServiceProvider);
    }

    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();