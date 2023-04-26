using ALedgerApi.Model.Comm;
using Elasticsearch.Net;
using Nest;
using RestDWH.Model;
using System.Security.Claims;

namespace ALedgerApi.Events
{
    public class AddressEvents : RestDWHEvents<Address>
    {
        public AddressEvents()
        {

        }
        public override async Task<(int from, int size, string query , string sort)> BeforeGetAsync(int from = 0, int size = 10, string query = "*", string sort = "", ClaimsPrincipal? user = null)
        {
            return (from, size, query, sort);
        }
    }
}
