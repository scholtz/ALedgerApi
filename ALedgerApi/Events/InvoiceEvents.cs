using ALedgerApi.Model;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using RestDWH.Base.Model;
using RestDWH.Elastic.Model;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace ALedgerApi.Events
{
    /// <summary>
    /// Invoice events manages the security of the invoices
    /// </summary>
    public class InvoiceEvents : RestDWHEventsElastic<Invoice>
    {
        public override Task<DBListBase<Invoice, DBBase<Invoice>>> AfterGetAsync(DBListBase<Invoice, DBBase<Invoice>> result, int from = 0, int size = 10, string query = "*", string sort = "", ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            var userId = user?.Identity?.Name ?? "";
            var ret = result.Results.Where(u => u.CreatedBy == userId);
            result.TotalCount = ret.Count();
            result.Offset = from;
            result.Limit = size;
            result.Results = ret.Skip(from).Take(size).ToArray();
            return base.AfterGetAsync(result, from, size, query, sort, user, serviceProvider);
        }
        public override Task<DBBase<Invoice>> AfterGetByIdAsync(DBBase<Invoice> result, string id, ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            var userId = user?.Identity?.Name ?? "";
            if(result.CreatedBy != userId) { throw new UnauthorizedAccessException("Resource was not created by authorized user"); }
            return base.AfterGetByIdAsync(result, id, user, serviceProvider);
        }
        public override Task<Dictionary<string, object>> AfterGetByIdWithFieldsAsync(Dictionary<string, object> result, string fields, string id, ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            throw new UnauthorizedAccessException("Access denied");
        }
        public override Task<FieldsListBase> AfterGetWithFieldsAsync(FieldsListBase result, string fields, int from = 0, int size = 10, string query = "*", string sort = "", ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            throw new UnauthorizedAccessException("Access denied");
        }
        public override Task<DBListBase<Invoice, DBBase<Invoice>>> AfterQueryAsync(DBListBase<Invoice, DBBase<Invoice>> result, string query = "", ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            var userId = user?.Identity?.Name ?? "";
            var queryModel = JsonConvert.DeserializeObject<ElasticQuery>(query);
            var ret = result.Results.Where(u => u.CreatedBy == userId);
            result.TotalCount = ret.Count();
            result.Offset = queryModel?.Offset ?? 0;
            result.Limit = queryModel?.Limit ?? 10;

            result.Results = ret.Skip(queryModel?.Offset ?? 0).Take(queryModel?.Limit ?? 10).ToArray();
            return base.AfterQueryAsync(result, query, user, serviceProvider);
        }
    }
}
