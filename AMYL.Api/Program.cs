using AMYL.Api.Features;
using AMYL.Api.Features.Authentication;
using AMYL.Api.Features.Memories;
using AMYL.Api.Infrastructure;
using AMYL.Api.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFeatures()
    .AddInfrastructure(builder.Configuration)
    .AddWeb(builder.Configuration);

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
app.MapMemoryEndpoints();
app.MapAuthenticationEndpoints();
app.MapControllers();

app.Run();
