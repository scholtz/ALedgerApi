
using ALedgerApi.Repository;
using Elasticsearch.Net;
using Nest;

namespace ALedgerApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers().AddNewtonsoftJson();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddProblemDetails();



            var settings =
                new ConnectionSettings(new Uri("https://elastic01.s4.a-wallet.net"))
                .ApiKeyAuthentication(new ApiKeyAuthenticationCredentials("TmVGX3dZWUJTbzM4elNmeGEtYUw6alhITDk1R3pTd1dCbGJsaVpXTDFSdw=="))
                .DefaultMappingFor<Model.Address.DB.DBAddress>(r => r.IndexName("Address-main"))
                .DefaultMappingFor<Model.Address.DB.DBAddressLog>(r => r.IndexName("Address-log"))
                .DefaultMappingFor<Model.Invoice.DB.DBInvoice>(r => r.IndexName("InvoiceCore-main"))
                .DefaultMappingFor<Model.Invoice.DB.DBInvoiceLog>(r => r.IndexName("InvoiceCore-log"))
                .DefaultMappingFor<Model.InvoiceItem.DB.DBInvoiceItem>(r => r.IndexName("InvoiceItem-main"))
                .DefaultMappingFor<Model.InvoiceItem.DB.DBInvoiceItemLog>(r => r.IndexName("InvoiceItem-log"))
                .DefaultMappingFor<Model.Person.DB.DBPerson>(r => r.IndexName("Person2-main"))
                .DefaultMappingFor<Model.Person.DB.DBPersonLog>(r => r.IndexName("Person2-log"));

            var client = new ElasticClient(settings);
            builder.Services.AddSingleton<IElasticClient>(client);

            builder.Services.AddSingleton<PersonRepository>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            _ = app.Services.GetService<PersonRepository>(); // init singleton
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}