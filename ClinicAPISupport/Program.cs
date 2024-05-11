using PatientSupportManager;
using Serilog;
using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(builder.Configuration["Logging:File:Path"])
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PatientSupportService>(serviceProvider => new PatientSupportService());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            var exception = contextFeature.Error;
            bool isDevelopment = app.Environment.IsDevelopment();
            string message = exception switch
            {
                NotFoundException _ => exception.Message,
                ValidationException _ => exception.Message,
                _ => "An internal server error occurred."
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception switch
            {
                NotFoundException _ => StatusCodes.Status404NotFound,
                ValidationException _ => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = message
            };

            Log.Error($"Unhandled exception: {exception.Message} | StackTrace: {exception.StackTrace}");

            await context.Response.WriteAsJsonAsync(response);
        }
    });
});

app.MapPost("/patients", (PatientSupportService service, Patient patient) => {
    try {
    return Results.Ok(service.GeneratePatientCode(patient));
    } catch (ValidationException ex) {
        Log.Information(ex.Message);
        return Results.BadRequest(ex.Message);
    }catch (Exception ex) {
        Log.Error(ex, "Failed to create patient code.");
        return Results.Problem(ex.Message);
    }
}).WithName("GeneratePatientCode");

app.Run();