﻿using ALedgerApi.Model;
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
        public override async Task<string> BeforeQueryAsync(string query = "", ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            if (user == null)
            {
                var queryJson = new
                {
                    query = new
                    {
                        match_none = new { }
                    }
                };
                query = JsonConvert.SerializeObject(queryJson);
                return await base.BeforeQueryAsync(query, user, serviceProvider);
            }
            else
            {
                var userId = user?.Identity?.Name ?? "";
                var queryModel = JsonConvert.DeserializeObject<ElasticQuery>(query);
                //all records with filter
                var queryJson = new
                {
                    query = new
                    {
                        @bool = new
                        {
                            must = new
                            {
                                match_all = new
                                {
                                }
                            },
                            filter = new
                            {
                                match = new
                                {
                                    createdBy = new
                                    {
                                        query = userId
                                    }
                                }
                            }
                        }
                    }
                };
                query = JsonConvert.SerializeObject(queryJson);
                return await base.BeforeQueryAsync(query, user, serviceProvider);
            }
        }
    }
}
