using ALedgerApi.Model;
using Elasticsearch.Net;
using Newtonsoft.Json;
using RestDWH.Base.Model;
using RestDWH.Elastic.Model;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace ALedgerApi.Events
{
    public class PersonEvents : RestDWHEventsElastic<Person>
    {
        //public override async Task<DBListBase<Invoice, DBBase<Invoice>>> AfterQueryAsync(DBListBase<Invoice, DBBase<Invoice>> result, string query = "", ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        //{

        //    return base.AfterQueryAsync(result, query, user, serviceProvider);
        //}
    }
}
