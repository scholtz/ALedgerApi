
using RestDWH.Model;
using RestDWH.Repository;
using RestDWH.Extensions;
using AlgorandAuthentication;
using Elasticsearch.Net;
using Microsoft.OpenApi.Models;
using Nest;
using Microsoft.Extensions.DependencyInjection;

namespace ALedgerApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddNewtonsoftJson().CreateAPIControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ALedgerApi",
                    Version = "v1",
                    Description = File.ReadAllText("doc/readme.md")
                });
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "ARC-0014 Algorand authentication transaction",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                });
                c.OperationFilter<Swashbuckle.AspNetCore.Filters.SecurityRequirementsOperationFilter>();
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                var xmlFile = $"doc/documentation.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            });
            builder.Services.AddProblemDetails();

            builder.Services
             .AddAuthentication(AlgorandAuthenticationHandler.ID)
             .AddAlgorand(o =>
             {
                 o.CheckExpiration = false;
                 o.AlgodServer = builder.Configuration["algod:server"];
                 o.AlgodServerToken = builder.Configuration["algod:token"];
                 o.AlgodServerHeader = builder.Configuration["algod:header"];
                 o.Realm = builder.Configuration["algod:realm"];
                 o.NetworkGenesisHash = builder.Configuration["algod:networkGenesisHash"];
             });


            var settings =
                new ConnectionSettings(new Uri("https://elastic01.s4.a-wallet.net"))
                .ApiKeyAuthentication(new ApiKeyAuthenticationCredentials("TmVGX3dZWUJTbzM4elNmeGEtYUw6alhITDk1R3pTd1dCbGJsaVpXTDFSdw=="))
                .ExtendElasticConnectionSettings();

            var client = new ElasticClient(settings);
            builder.Services.AddSingleton<IElasticClient>(client);
            builder.Services.RegisterRestDWHRepositories();
            builder.Services.RegisterRestDWHEvents();

            var app = builder.Build();

            var resp = client.Ping();
            if (!resp.IsValid)
            {
                throw new Exception("Connection to elastic has not been established");
            }

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}
            app.PreloadRestDWHRepositories();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}