
using ALedgerBFFApi.Model.Options;
using AlgorandAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

namespace ALedgerBFFApi
{
    public class Program
    {
        /// <summary>
        /// BFF main entry point
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var web = CreateWebApplication();
            web.Run();
        }
        /// <summary>
        /// Create web app for main entry point and unit tests
        /// </summary>
        /// <param name="configFile">Config file</param>
        /// <returns>WebApplication</returns>
        public static WebApplication CreateWebApplication(string configFile = "appsettings.json")
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFile, true, true);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ALedgerBFFApi",
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

            });
            builder.Services.AddProblemDetails();

            // Add CORS policy
            var corsConfig = builder.Configuration.GetSection("Cors").AsEnumerable().Select(k => k.Value).Where(k => !string.IsNullOrEmpty(k)).ToArray();
            if (!(corsConfig?.Length > 0)) throw new Exception("Cors not defined");

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                builder =>
                {
                    builder.WithOrigins(corsConfig)
                                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                                        .AllowAnyMethod()
                                        .AllowAnyHeader()
                                        .AllowCredentials()
                                        .WithExposedHeaders("rowcount", "rowstate");
                });
            });

            var algorandAuthenticationOptions = new AlgorandAuthenticationOptions();
            builder.Configuration.GetSection("AlgorandAuthentication").Bind(algorandAuthenticationOptions);

            builder.Services
             .AddAuthentication(AlgorandAuthenticationHandler.ID)
             .AddAlgorand(o =>
             {
                 o.CheckExpiration = algorandAuthenticationOptions.CheckExpiration;
                 o.Debug = algorandAuthenticationOptions.Debug;
                 o.AlgodServer = algorandAuthenticationOptions.AlgodServer;
                 o.AlgodServerToken = algorandAuthenticationOptions.AlgodServerToken;
                 o.AlgodServerHeader = algorandAuthenticationOptions.AlgodServerHeader;
                 o.Realm = algorandAuthenticationOptions.Realm;
                 o.NetworkGenesisHash = algorandAuthenticationOptions.NetworkGenesisHash;
                 o.MsPerBlock = algorandAuthenticationOptions.MsPerBlock;
                 o.EmptySuccessOnFailure = algorandAuthenticationOptions.EmptySuccessOnFailure;
                 o.EmptySuccessOnFailure = algorandAuthenticationOptions.EmptySuccessOnFailure;
             });

            builder.Services.Configure<ObjectStorage>(builder.Configuration.GetSection("ObjectStorage"));
            builder.Services.Configure<BFF>(builder.Configuration.GetSection("BFF"));
            builder.Services.AddTransient<Controllers.InvoiceController>();
            builder.Services.AddTransient<Controllers.PersonController>();

            var app = builder.Build();


            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            return app;
        }
    }
}