
using RestDWH.Base.Model;
using RestDWH.Base.Repository;
using RestDWH.Base.Extensions;
using AlgorandAuthentication;
using Elasticsearch.Net;
using Microsoft.OpenApi.Models;
using Nest;
using Microsoft.Extensions.DependencyInjection;
using RestDWH.Elastic.Extensions;
using RestDWH.Elastic.Repository;
using RestDWH.Base.Extensios;
using RestDWH.Elastic.Model;
using ALedgerApi.Events;
using ALedgerApi.Model;

namespace ALedgerApi
{
    /// <summary>
    /// Main entry point
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main app entry point
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
        /// <exception cref="Exception"></exception>
        public static WebApplication CreateWebApplication(string configFile = "appsettings.json")
        {
            var builder = WebApplication.CreateBuilder();

            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFile, true, true);


            builder.Services.AddControllers().AddNewtonsoftJson();
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

            var elasticConfig = new ALedgerApi.Model.Config.Elastic();
            builder.Configuration.GetSection("Elastic").Bind(elasticConfig);
            var config2 = new RestDWH.Elastic.Model.Config.Elastic()
            {
                ApiKey = elasticConfig.Token,
                Host = elasticConfig.Server
            };
            var settings =
                new ConnectionSettings(new Uri(elasticConfig.Server))
                .ApiKeyAuthentication(new ApiKeyAuthenticationCredentials(elasticConfig.Token))
                .ExtendElasticConnectionSettings(config2);

            var client = new ElasticClient(settings);
            builder.Services.AddSingleton<IElasticClient>(client);
            //Address
            builder.Services.AddSingleton<IDWHRepository<Model.Address>, RestDWHElasticSearchRepository<Model.Address>>();
            builder.Services.AddSingleton<RestDWHEventsElastic<Model.Address>, AddressEvents>();
            builder.Services.AddSingleton<IElasticDWHRepository<Model.Address>, RestDWHElasticSearchRepositoryExtended<Model.Address>>();
            builder.Services.AddSingleton<RestDWHElasticSearchRepositoryExtended<Model.Address>>();
            //Person
            builder.Services.AddSingleton<IDWHRepository<Model.Person>, RestDWHElasticSearchRepository<Model.Person>>();
            builder.Services.AddSingleton<RestDWHEventsElastic<Model.Person>, PersonEvents>();
            builder.Services.AddSingleton<IElasticDWHRepository<Model.Person>, RestDWHElasticSearchRepositoryExtended<Model.Person>>();
            builder.Services.AddSingleton<RestDWHElasticSearchRepositoryExtended<Model.Person>>();
            //Invoice
            builder.Services.AddSingleton<IDWHRepository<Model.Invoice>, RestDWHElasticSearchRepository<Model.Invoice>>();
            builder.Services.AddSingleton<RestDWHEventsElastic<Model.Invoice>, InvoiceEvents>();
            builder.Services.AddSingleton<IElasticDWHRepository<Model.Invoice>, RestDWHElasticSearchRepositoryExtended<Model.Invoice>>();
            builder.Services.AddSingleton<RestDWHElasticSearchRepositoryExtended<Model.Invoice>>();
            //InvoiceItem
            builder.Services.AddSingleton<IDWHRepository<Model.InvoiceItem>, RestDWHElasticSearchRepository<Model.InvoiceItem>>();
            builder.Services.AddSingleton<RestDWHEventsElastic<Model.InvoiceItem>>();
            builder.Services.AddSingleton<IElasticDWHRepository<Model.InvoiceItem>, RestDWHElasticSearchRepositoryExtended<Model.InvoiceItem>>();
            builder.Services.AddSingleton<RestDWHElasticSearchRepositoryExtended<Model.InvoiceItem>>();
            //PaymentMethod
            builder.Services.AddSingleton<IDWHRepository<Model.PaymentMethod>, RestDWHElasticSearchRepository<Model.PaymentMethod>>();
            builder.Services.AddSingleton<RestDWHEventsElastic<Model.PaymentMethod>>();
            builder.Services.AddSingleton<IElasticDWHRepository<Model.PaymentMethod>, RestDWHElasticSearchRepositoryExtended<Model.PaymentMethod>>();
            builder.Services.AddSingleton<RestDWHElasticSearchRepositoryExtended<Model.PaymentMethod>>();
            //User2Person
            builder.Services.AddSingleton<IDWHRepository<Model.User2Person>, RestDWHElasticSearchRepository<Model.User2Person>>();
            builder.Services.AddSingleton<RestDWHEventsElastic<Model.User2Person>>();
            builder.Services.AddSingleton<IElasticDWHRepository<Model.User2Person>, RestDWHElasticSearchRepositoryExtended<Model.User2Person>>();
            builder.Services.AddSingleton<RestDWHElasticSearchRepositoryExtended<Model.User2Person>>();

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

            app.UseAuthentication();
            app.UseAuthorization();
            //}
            var serviceAddress = app.Services.GetService<IDWHRepository<Model.Address>>();
            app.MapEndpoints<Model.Address>(serviceAddress);
            var serviceAddressExtended = app.Services.GetService<IElasticDWHRepository<Model.Address>>();
            app.MapElasticEndpoints<Model.Address>(serviceAddressExtended);

            var servicePerson = app.Services.GetService<IDWHRepository<Model.Person>>();
            app.MapEndpoints<Model.Person>(servicePerson);
            var servicePersonExtended = app.Services.GetService<IElasticDWHRepository<Model.Person>>();
            app.MapElasticEndpoints<Model.Person>(servicePersonExtended);

            var serviceInvoice = app.Services.GetService<IDWHRepository<Model.Invoice>>();
            app.MapEndpoints<Model.Invoice>(serviceInvoice);
            var serviceInvoiceExtended = app.Services.GetService<IElasticDWHRepository<Model.Invoice>>();
            app.MapElasticEndpoints<Model.Invoice>(serviceInvoiceExtended);

            var serviceInvoiceItem = app.Services.GetService<IDWHRepository<Model.InvoiceItem>>();
            app.MapEndpoints<Model.InvoiceItem>(serviceInvoiceItem);
            var serviceInvoiceItemExtended = app.Services.GetService<IElasticDWHRepository<Model.InvoiceItem>>();
            app.MapElasticEndpoints<Model.InvoiceItem>(serviceInvoiceItemExtended);

            var servicePaymentMethod = app.Services.GetService<IDWHRepository<Model.PaymentMethod>>();
            app.MapEndpoints<Model.PaymentMethod>(servicePaymentMethod);
            var servicePaymentMethodExtended = app.Services.GetService<IElasticDWHRepository<Model.PaymentMethod>>();
            app.MapElasticEndpoints<Model.PaymentMethod>(servicePaymentMethodExtended);

            var serviceUser2Person = app.Services.GetService<IDWHRepository<Model.User2Person>>();
            app.MapEndpoints<Model.User2Person>(serviceUser2Person);
            var serviceUser2PersonExtended = app.Services.GetService<IElasticDWHRepository<Model.User2Person>>();
            app.MapElasticEndpoints<Model.User2Person>(serviceUser2PersonExtended);

            app.MapControllers();

            return app;
        }
    }
}