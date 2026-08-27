using AMYL.Api.Features.Authentication;
using AMYL.Api.Features.Memories;
using AMYL.Api.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddPersistence(builder.Configuration)
    .AddCaching(builder.Configuration)
    .AddApiAuthentication(builder.Configuration)
    .AddApiAuthorization()
    .AddApiRateLimiting()
    .AddApiOpenApi()
    .AddApiCors()
    .AddObservability()
    .AddAuthenticationFeature()
    .AddMemoriesFeature();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHealthChecks("/health");
app.MapAuthentication();
app.MapMemories();

app.Run();
