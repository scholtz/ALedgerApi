
using ALedgerApi.Model.Comm;
using ALedgerApi.Model.DB;
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
                .DefaultMappingFor<DBBase<Address>>(r => r.IndexName("address-main"))
                .DefaultMappingFor<DBBaseLog<Address>>(r => r.IndexName("address-log"))
                .DefaultMappingFor<DBBase<Invoice>>(r => r.IndexName("invoiceCore-main"))
                .DefaultMappingFor<DBBaseLog<Invoice>>(r => r.IndexName("invoiceCore-log"))
                .DefaultMappingFor<DBBase<InvoiceItem>>(r => r.IndexName("invoiceItem-main"))
                .DefaultMappingFor<DBBaseLog<InvoiceItem>>(r => r.IndexName("invoiceItem-log"))
                .DefaultMappingFor<DBBase<Person>>(r => r.IndexName("person2-main"))
                .DefaultMappingFor<DBBaseLog<Person>>(r => r.IndexName("person2-log"))
                .DefaultMappingFor<DBBase<TestId>>(r => r.IndexName("testid-main"))
                .DefaultMappingFor<DBBaseLog<TestId>>(r => r.IndexName("testid-log"));

            var client = new ElasticClient(settings);
            builder.Services.AddSingleton<IElasticClient>(client);

            builder.Services.AddSingleton<BaseRepository<
                Address,
                Model.DB.DBBase<Address>,
                Model.DB.DBListBase<Address, Model.DB.DBBase<Address>>,
                DBBaseLog<Address>
            >>();
            builder.Services.AddSingleton<BaseRepository<
                Person,
                Model.DB.DBBase<Person>,
                Model.DB.DBListBase<Person, Model.DB.DBBase<Person>>,
                DBBaseLog<Person>
            >>();
            builder.Services.AddSingleton<BaseRepository<
                InvoiceItem,
                Model.DB.DBBase<InvoiceItem>,
                Model.DB.DBListBase<InvoiceItem, Model.DB.DBBase<InvoiceItem>>,
                DBBaseLog<InvoiceItem>
            >>();
            builder.Services.AddSingleton<BaseRepository<
                Invoice,
                Model.DB.DBBase<Invoice>,
                Model.DB.DBListBase<Invoice, Model.DB.DBBase<Invoice>>,
                DBBaseLog<Invoice>
            >>();
            builder.Services.AddSingleton<BaseRepository<
                TestId,
                Model.DB.DBBase<TestId>,
                Model.DB.DBListBase<TestId, Model.DB.DBBase<TestId>>,
                DBBaseLog<TestId>
            >>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            _ = app.Services.GetService<BaseRepository<
                Address,
                Model.DB.DBBase<Address>,
                Model.DB.DBListBase<Address, Model.DB.DBBase<Address>>,
                DBBaseLog<Address>
            >>();// init singleton
            _ = app.Services.GetService<BaseRepository<
                Person,
                Model.DB.DBBase<Person>,
                Model.DB.DBListBase<Person, Model.DB.DBBase<Person>>,
                DBBaseLog<Person>
            >>(); // init singleton
            _ = app.Services.GetService<BaseRepository<
                Invoice,
                Model.DB.DBBase<Invoice>,
                Model.DB.DBListBase<Invoice, Model.DB.DBBase<Invoice>>,
                DBBaseLog<Invoice>
            >>(); // init singleton
            _ = app.Services.GetService<BaseRepository<
                InvoiceItem,
                Model.DB.DBBase<InvoiceItem>,
                Model.DB.DBListBase<InvoiceItem, Model.DB.DBBase<InvoiceItem>>,
                DBBaseLog<InvoiceItem>
            >>(); // init singleton
            _ = app.Services.GetService<BaseRepository<
                TestId,
                Model.DB.DBBase<TestId>,
                Model.DB.DBListBase<TestId, Model.DB.DBBase<TestId>>,
                DBBaseLog<TestId>
            >>(); // init singleton
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}