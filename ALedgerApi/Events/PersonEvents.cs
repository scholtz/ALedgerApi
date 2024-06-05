using ALedgerApi.Model;
using Algorand.KMD;
using Elasticsearch.Net;
using Newtonsoft.Json;
using RestDWH.Base.Model;
using RestDWH.Base.Repository;
using RestDWH.Elastic.Model;
using RestDWH.Elastic.Repository;
using System.CodeDom;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace ALedgerApi.Events
{
    public class PersonEvents : RestDWHEventsElastic<Person>
    {
        public override async Task<DBBase<Person>> AfterPostAsync(DBBase<Person> result, Person data, ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            if (user != null)
            {
                var userId = user?.Identity?.Name ?? "";
                var serviceUser2Person = serviceProvider?.GetService<IElasticDWHRepository<Model.User2Person>>() ?? throw new NullReferenceException("IElasticDWHRepository");                
                var queryJson = new
                {
                    query = new
                    {
                        @bool = new
                        {
                            must = new List<ElasticMust>
                            {
                                new ElasticMust
                                {
                                    Match = new ElasticMatch
                                    {
                                        QueryPropertyName = "data.userId",
                                        QueryProperty = new QueryProperty
                                        {
                                            Query = userId
                                        }
                                    }
                                },
                                new ElasticMust
                                {
                                    Match = new ElasticMatch
                                    {
                                        QueryPropertyName = "data.personId",
                                        QueryProperty = new QueryProperty
                                        {
                                            Query = result.Id
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
                var query = JsonConvert.SerializeObject(queryJson);
                var relation = await serviceUser2Person?.QueryAsync(query);
                if (relation == null || relation.Results.Count() == 0)
                {
                    var user2Person = new User2Person
                    {
                        UserId = userId,
                        PersonId = result.Id
                    };
                    var serviceUser2PersonRepo = (serviceProvider?.GetService<IDWHRepository<Model.User2Person>>() as RestDWHElasticSearchRepository<Model.User2Person>) ?? throw new NullReferenceException("RestDWHElasticSearchRepository");
                    _ = await serviceUser2PersonRepo.PostAsync(user2Person, user);

                }
            }
            return await base.AfterPostAsync(result, data, user, serviceProvider);
        }

        //public override async Task<string> BeforeQueryAsync(string query = "", ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        //{
        //    if (user == null)
        //    {
        //        var queryJson = new
        //        {
        //            query = new
        //            {
        //                match_none = new { }
        //            }
        //        };
        //        query = JsonConvert.SerializeObject(queryJson);
        //        return await base.BeforeQueryAsync(query, user, serviceProvider);
        //    }
        //    else
        //    {
        //        var userId = user?.Identity?.Name ?? "";
        //        var queryModel = JsonConvert.DeserializeObject<ElasticQuery>(query);
        //        //all records with filter
        //        var queryJson = new
        //        {
        //            query = new
        //            {
        //                @bool = new
        //                {
        //                    must = new
        //                    {
        //                        match_all = new
        //                        {
        //                        }
        //                    },
        //                    filter = new
        //                    {
        //                        match = new
        //                        {
        //                            createdBy = new
        //                            {
        //                                query = userId
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        };
        //        query = JsonConvert.SerializeObject(queryJson);
        //        return await base.BeforeQueryAsync(query, user, serviceProvider);
        //    }
        //}

        public override Task<DBListBase<Person, DBBase<Person>>> AfterGetAsync(DBListBase<Person, DBBase<Person>> result, int from = 0, int size = 10, string query = "*", string sort = "", ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            var userId = user?.Identity?.Name ?? "";
            var ret = result.Results.Where(u => u.CreatedBy == userId);
            result.TotalCount = ret.Count();
            result.Offset = from;
            result.Limit = size;
            result.Results = ret.Skip(from).Take(size).ToArray();
            return base.AfterGetAsync(result, from, size, query, sort, user, serviceProvider);
        }
        public override Task<DBBase<Person>> AfterGetByIdAsync(DBBase<Person> result, string id, ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            var userId = user?.Identity?.Name ?? "";
            if (result.CreatedBy != userId) { throw new UnauthorizedAccessException("Resource was not created by authorized user"); }
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
        public override Task<DBListBase<Person, DBBase<Person>>> AfterQueryAsync(DBListBase<Person, DBBase<Person>> result, string query = "", ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
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
