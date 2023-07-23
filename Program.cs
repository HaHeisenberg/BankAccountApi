using BankAccountApi.DbContexts;
using BankAccountApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var audience = "https://localhost:7000";
    //"54OaOP4Ec6FkjntJPzjzYv01ntyZVSo8";
    //"https://localhost:7114";
    //builder.Environment.IsDevelopment()
    //    ? "http://"
    //        + builder.Configuration.GetSection("Services").GetSection("Accounts").GetValue(typeof(string), "Host")
    //        + ":"
    //        + builder.Configuration.GetSection("Services").GetSection("Accounts").GetValue(typeof(string), "Port")
    //    : Environment.GetEnvironmentVariable("ACCOUNTS_API_ADDRESS");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme ,options =>
{
    options.Authority = "https://bankproject.eu.auth0.com/";
    options.Audience = audience;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidAudience = "https://localhost:7000",
        ValidIssuer = "https://bankproject.eu.auth0.com/",
    };
    options.MapInboundClaims = true;
});

//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(builder =>
//    {
//        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials();
//        builder.WithHeaders("Access-Control-Allow-Origin");
//        var strings = new string[] { "Get", "Options" };
//        builder.WithMethods(strings);
//    });
//});

SecretClientOptions options = new SecretClientOptions()
{
    Retry =
        {
            Delay= TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(16),
            MaxRetries = 5,
            Mode = RetryMode.Exponential
         }
};
var client = new SecretClient(new Uri("https://bankprojectkeyvault.vault.azure.net/"), new DefaultAzureCredential(), options);

KeyVaultSecret secret = client.GetSecret("BankProjectSQLConnectionString");

string dbConnectionString = secret.Value;

builder.Services.AddSqlServer<AccountDbContext>(dbConnectionString, options => options.EnableRetryOnFailure());

builder.Services.AddScoped<IAccountRepository, AccountRepository>();

var app = builder.Build();
app.UseCors(builder => {
    builder.AllowAnyOrigin();
    builder.AllowAnyMethod();
    builder.AllowAnyHeader();
});

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
