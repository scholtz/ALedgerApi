using RestDWH.Extensions;
using RestDWH.Model;
using Microsoft.AspNetCore.JsonPatch;
using Nest;
using Elasticsearch.Net;
using System.Drawing;

namespace RestDWH.Repository
{
    public class BaseRepository<TEnt>
        where TEnt : class
    {
        private readonly IElasticClient _elasticClient;
        private readonly RestDWHEvents<TEnt> _events;
        public BaseRepository(IElasticClient elasticClient, RestDWHEvents<TEnt> events)
        {
            _elasticClient = elasticClient;
            _events = events;
        }

        public async Task<DBListBase<TEnt, DBBase<TEnt>>> Get(int from = 0, int size = 10, string query = "*", string sort = "", System.Security.Claims.ClaimsPrincipal? user = null)
        {

            (from, size, query, sort) = await _events.BeforeGetAsync(from, size, query, sort, user);
            var searchResponse = await _elasticClient.SearchAsync<DBBase<TEnt>>(s =>
            {
                s = s.From(from);
                s = s.Size(size);
                s = s.QueryOnQueryString(query);
                if (!string.IsNullOrEmpty(sort.Trim()))
                {
                    s = s.Sort(sortDescriptor =>
                    {

                        foreach (var sortItem in sort.Split(","))
                        {
                            var sortItemLower = sortItem.ToLower().Trim();
                            if (string.IsNullOrEmpty(sortItemLower.Trim())) continue;
                            var dir = sortItemLower.EndsWith(" desc") ? "desc" : sortItemLower.EndsWith(" asc") ? "asc" : "asc";
                            var strDir = $" {dir}";
                            var field = (sortItemLower.EndsWith(strDir) ? sortItem.Substring(0, sortItem.Length - strDir.Length) : sortItem).Trim();
                            if (dir == "asc")
                            {
                                sortDescriptor = sortDescriptor.Ascending(field);
                            }
                            else
                            {
                                sortDescriptor = sortDescriptor.Descending(field);
                            }
                        }

                        return sortDescriptor;
                    });
                }
                return s;
            });

            var count = await _elasticClient.CountAsync<DBBase<TEnt>>(s => s
                //.Index("person-main")
                .QueryOnQueryString(query)
                );

            var list = searchResponse.Hits.Select(s => { s.Source.Id = s.Id; return s.Source; }).ToArray();
            var instance = Activator.CreateInstance(typeof(DBListBase<TEnt, DBBase<TEnt>>)) as DBListBase<TEnt, DBBase<TEnt>>;
            if (instance == null) throw new Exception("Unable to inicialize DBListBase<TEnt, DBBase<TEnt>>");
            instance.Results = list;
            instance.From = from;
            instance.Size = size;
            instance.TotalCount = count.Count;
            var result = await _events.AfterGetAsync(instance, from, size, query, sort, user);
            return result;
        }


        public async Task<DBBase<TEnt>?> GetById(string id, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            id = await _events.BeforeGetByIdAsync(id, user);
            var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);//
            if (searchResponse.Source == null) { throw new Exception("Not found"); }
            searchResponse.Source.Id = searchResponse.Id;
            var result = await _events.AfterGetByIdAsync(searchResponse.Source, id, user);
            return searchResponse.Source;
        }
        public async Task<DBBase<TEnt>> Post(TEnt person, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            var now = DateTimeOffset.Now;
            var instance = Activator.CreateInstance(typeof(DBBase<TEnt>)) as DBBase<TEnt>;
            if (instance == null) throw new Exception("Unable to inicialize DBBase<TEnt>");
            instance.Created = now;
            instance.Updated = now;
            instance.Data = person;
            instance.CreatedBy = user?.Identity?.Name;
            instance.UpdatedBy = user?.Identity?.Name;

            var indexResponse = await _elasticClient.IndexDocumentAsync(instance);
            if (!indexResponse.IsValid) throw new Exception(indexResponse.DebugInformation);
            var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(indexResponse.Id);
            searchResponse.Source.Id = indexResponse.Id;
            return searchResponse.Source;
        }
        public async Task<DBBase<TEnt>> Put(string id, TEnt data, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            if (data?.Equals(searchResponse.Source.Data) == true)
            {
                return searchResponse.Source;// do not update if the docs are equal
            }
            var instance = Activator.CreateInstance(typeof(DBBase<TEnt>)) as DBBase<TEnt>;
            if (instance == null) throw new Exception("Unable to inicialize DBBase<TEnt>");
            instance.Id = id;
            instance.Created = searchResponse.Source.Created;
            instance.Updated = DateTimeOffset.Now;
            instance.Data = data;
            instance.CreatedBy = searchResponse.Source.CreatedBy ?? user?.Identity?.Name;
            instance.UpdatedBy = user?.Identity?.Name;

            var instanceLog = Activator.CreateInstance(typeof(DBBaseLog<TEnt>)) as DBBaseLog<TEnt>;
            if (instanceLog == null) throw new Exception("Unable to inicialize DBBaseLog<TEnt>");
            instanceLog.Created = searchResponse.Source.Created;
            instanceLog.Updated = DateTimeOffset.Now;
            instanceLog.Data = searchResponse.Source.Data;
            instanceLog.UpdatedBy = searchResponse.Source.UpdatedBy;
            instanceLog.RefId = searchResponse.Id;
            instanceLog.Version = searchResponse.Version;

            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<DBBaseLog<TEnt>>(r => r.Document(instanceLog)).
                Update<DBBase<TEnt>>(r => r.Id(id).Doc(instance)));

            var finalResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (finalResponse == null) throw new Exception($"FATAL Error occured. Failed to update {id} and instance is not available any more");
            finalResponse.Source.Id = finalResponse.Id;
            return finalResponse.Source;
        }

        public async Task<DBBase<TEnt>> Upsert(string id, TEnt data, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (searchResponse.Source != null && data?.Equals(searchResponse.Source.Data) == true)
            {
                return searchResponse.Source;// do not update if the docs are equal
            }
            var now = DateTimeOffset.Now;
            var instance = Activator.CreateInstance(typeof(DBBase<TEnt>)) as DBBase<TEnt>;
            if (instance == null) throw new Exception("Unable to inicialize DBBase<TEnt>");
            instance.Id = id;
            instance.Created = searchResponse.Source?.Created ?? now;
            instance.Updated = now;
            instance.Data = data;
            instance.CreatedBy = searchResponse.Source?.CreatedBy ?? user?.Identity?.Name;
            instance.UpdatedBy = user?.Identity?.Name;
            if (searchResponse.Source == null)
            {
                // new record

                _ = await _elasticClient.IndexDocumentAsync(instance);
            }
            else
            {
                // update record
                var instanceLog = Activator.CreateInstance(typeof(DBBaseLog<TEnt>)) as DBBaseLog<TEnt>;
                if (instanceLog == null) throw new Exception("Unable to inicialize DBBaseLog<TEnt>");
                instanceLog.Created = searchResponse.Source?.Created ?? now;
                instanceLog.Updated = now;
                instanceLog.Data = searchResponse.Source?.Data;
                instanceLog.UpdatedBy = searchResponse.Source?.UpdatedBy;
                instanceLog.RefId = searchResponse.Id;
                instanceLog.Version = searchResponse.Version;

                _ = await _elasticClient.BulkAsync(r =>
                    r.
                    Index<DBBaseLog<TEnt>>(r => r.Document(instanceLog)).
                    Update<DBBase<TEnt>>(r => r.Id(id).Doc(instance)));
            }

            var finalResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (finalResponse == null) throw new Exception($"FATAL Error occured. Failed to update {id} and instance is not available any more");
            finalResponse.Source.Id = finalResponse.Id;
            return finalResponse.Source;
        }


        public async Task<DBBase<TEnt>> Patch(string id, JsonPatchDocument<TEnt> data, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            var orig = searchResponse.Source.Data.DeepCopy();

            data.ApplyTo(searchResponse.Source.Data);

            if (orig.Equals(searchResponse.Source.Data))
            {
                return searchResponse.Source;// do not update if the docs are equal
            }
            var instance = Activator.CreateInstance(typeof(DBBase<TEnt>)) as DBBase<TEnt>;
            if (instance == null) throw new Exception("Unable to inicialize DBBase<TEnt>");
            instance.Created = searchResponse.Source.Created;
            instance.Updated = DateTimeOffset.Now;
            instance.Data = searchResponse.Source.Data;
            instance.CreatedBy = searchResponse.Source?.CreatedBy;
            instance.UpdatedBy = user?.Identity?.Name;

            var instanceLog = Activator.CreateInstance(typeof(DBBaseLog<TEnt>)) as DBBaseLog<TEnt>;
            if (instanceLog == null) throw new Exception("Unable to inicialize DBBaseLog<TEnt>");
            instanceLog.Created = searchResponse.Source.Created;
            instanceLog.Updated = DateTimeOffset.Now;
            instanceLog.Data = orig;
            instanceLog.CreatedBy = searchResponse.Source?.CreatedBy;
            instanceLog.UpdatedBy = searchResponse.Source?.UpdatedBy;
            instanceLog.RefId = searchResponse.Id;
            instanceLog.Version = searchResponse.Version;

            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<DBBaseLog<TEnt>>(r => r.Document(instanceLog)).
                Update<DBBase<TEnt>>(r => r.Id(id).Doc(instance)));

            var finalResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (finalResponse == null) throw new Exception($"FATAL Error occured. Failed to update {id} and instance is not available any more");
            finalResponse.Source.Id = finalResponse.Id;
            return finalResponse.Source;
        }
        public async Task<DBBase<TEnt>> Delete(string id, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            //var deleteResponse = await _elasticClient.DeleteAsync<DBPerson>(id);

            var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            var instanceLog = Activator.CreateInstance(typeof(DBBaseLog<TEnt>)) as DBBaseLog<TEnt>;

            if (instanceLog == null) throw new Exception("Unable to inicialize DBBaseLog<TEnt>");
            instanceLog.Created = searchResponse.Source.Created;
            instanceLog.Updated = searchResponse.Source.Updated;
            instanceLog.Deleted = DateTimeOffset.Now;
            instanceLog.Data = searchResponse.Source.Data;
            instanceLog.CreatedBy = searchResponse.Source.CreatedBy;
            instanceLog.UpdatedBy = searchResponse.Source.UpdatedBy;
            instanceLog.DeletedBy = user?.Identity?.Name;
            instanceLog.RefId = searchResponse.Id;
            instanceLog.Version = searchResponse.Version;


            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<DBBaseLog<TEnt>>(r => r.Document(instanceLog)).
                Delete<DBBase<TEnt>>(r => r.Id(id)));

            var errors = updateResponse.ItemsWithErrors.Select(e => e.Error?.Reason).Where(e => !string.IsNullOrEmpty(e));
            var hasErrors = errors?.Any() == true;
            if (hasErrors)
            {
                throw new Exception(string.Join(";", errors));
            }
            searchResponse.Source.Id = searchResponse.Id;
            return searchResponse.Source;
        }
    }

}
