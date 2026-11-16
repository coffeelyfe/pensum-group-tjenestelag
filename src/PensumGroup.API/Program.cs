using Microsoft.AspNetCore.Authentication.JwtBearer;
using PensumGroup.Modules.Equities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var azureAdSection = builder.Configuration.GetSection("AzureAd");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{azureAdSection["Instance"]}{azureAdSection["TenantId"]}/v2.0";
        options.Audience = azureAdSection["Audience"];
    });

builder.Services.AddAuthorization();

builder.Services.AddEquitiesModule(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
