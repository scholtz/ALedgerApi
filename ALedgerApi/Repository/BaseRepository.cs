using ALedgerApi.Extensions;
using ALedgerApi.Model.DB;
using Microsoft.AspNetCore.JsonPatch;
using Nest;

namespace ALedgerApi.Repository
{
    public class BaseRepository<TEnt, TDBEnt, TDBEntList, TDBEntLog>
        where TEnt : class
        where TDBEnt : DBBase<TEnt>
        where TDBEntList : DBListBase<TEnt, TDBEnt>
        where TDBEntLog : DBBaseLog<TEnt>
    {
        private readonly IElasticClient _elasticClient;

        public BaseRepository(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<TDBEntList> Get(int from = 0, int size = 10, string query = "*")
        {

            var searchResponse = await _elasticClient.SearchAsync<TDBEnt>(s => s
                //.Index("person-main")
                .From(from)
                .Size(size)
                .QueryOnQueryString(query)
            );

            var count = await _elasticClient.CountAsync<TDBEnt>(s => s
                //.Index("person-main")
                .QueryOnQueryString(query)
                );

            var list = searchResponse.Hits.Select(s => { s.Source.Id = s.Id; return s.Source; }).ToArray();

            var instance = Activator.CreateInstance(typeof(TDBEntList)) as TDBEntList;
            if (instance == null) throw new Exception("Unable to inicialize TDBEntList");
            instance.Results = list;
            instance.From = from;
            instance.Size = size;
            instance.TotalCount = count.Count;
            return instance;
        }


        public async Task<TDBEnt?> GetById(string id)
        {
            var searchResponse = await _elasticClient.GetAsync<TDBEnt>(id);//
            if (searchResponse.Source == null) { throw new Exception("Not found"); }
            searchResponse.Source.Id = searchResponse.Id;
            return searchResponse.Source;
        }
        public async Task<TDBEnt> Post(TEnt person)
        {
            var now = DateTimeOffset.Now;
            var instance = Activator.CreateInstance(typeof(TDBEnt)) as TDBEnt;
            if (instance == null) throw new Exception("Unable to inicialize TDBEnt");
            instance.Created = now;
            instance.Updated = now;
            instance.Data = person;
            instance.UpdatedBy = "";

            var indexResponse = await _elasticClient.IndexDocumentAsync(instance);
            if (!indexResponse.IsValid) throw new Exception(indexResponse.DebugInformation);
            var searchResponse = await _elasticClient.GetAsync<TDBEnt>(indexResponse.Id);
            searchResponse.Source.Id = indexResponse.Id;
            return searchResponse.Source;
        }
        public async Task<TDBEnt> Put(string id, TEnt data)
        {
            var searchResponse = await _elasticClient.GetAsync<TDBEnt>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            if (data?.Equals(searchResponse.Source.Data) == true)
            {
                return searchResponse.Source;// do not update if the docs are equal
            }
            var instance = Activator.CreateInstance(typeof(TDBEnt)) as TDBEnt;
            if (instance == null) throw new Exception("Unable to inicialize TDBEnt");
            instance.Id = id;
            instance.Created = searchResponse.Source.Created;
            instance.Updated = DateTimeOffset.Now;
            instance.Data = data;
            instance.UpdatedBy = "";

            var instanceLog = Activator.CreateInstance(typeof(TDBEntLog)) as TDBEntLog;
            if (instanceLog == null) throw new Exception("Unable to inicialize TDBEntLog");
            instanceLog.Created = searchResponse.Source.Created;
            instanceLog.Updated = DateTimeOffset.Now;
            instanceLog.Data = searchResponse.Source.Data;
            instanceLog.UpdatedBy = searchResponse.Source.UpdatedBy;
            instanceLog.RefId = searchResponse.Id;
            instanceLog.Version = searchResponse.Version;

            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<TDBEntLog>(r => r.Document(instanceLog)).
                Update<TDBEnt>(r => r.Id(id).Doc(instance)));

            var finalResponse = await _elasticClient.GetAsync<TDBEnt>(id);
            if (finalResponse == null) throw new Exception($"FATAL Error occured. Failed to update {id} and instance is not available any more");
            finalResponse.Source.Id = finalResponse.Id;
            return finalResponse.Source;
        }

        public async Task<TDBEnt> Upsert(string id, TEnt data)
        {
            var searchResponse = await _elasticClient.GetAsync<TDBEnt>(id);
            if (searchResponse.Source != null && data?.Equals(searchResponse.Source.Data) == true)
            {
                return searchResponse.Source;// do not update if the docs are equal
            }
            var now = DateTimeOffset.Now;
            var instance = Activator.CreateInstance(typeof(TDBEnt)) as TDBEnt;
            if (instance == null) throw new Exception("Unable to inicialize TDBEnt");
            instance.Id = id;
            instance.Created = searchResponse.Source?.Created ?? now;
            instance.Updated = now;
            instance.Data = data;
            instance.UpdatedBy = "";
            if (searchResponse.Source == null)
            {
                // new record

                _ = await _elasticClient.IndexDocumentAsync(instance);
            }
            else
            {
                // update record
                var instanceLog = Activator.CreateInstance(typeof(TDBEntLog)) as TDBEntLog;
                if (instanceLog == null) throw new Exception("Unable to inicialize TDBEntLog");
                instanceLog.Created = searchResponse.Source?.Created ?? now;
                instanceLog.Updated = now;
                instanceLog.Data = searchResponse.Source?.Data;
                instanceLog.UpdatedBy = searchResponse.Source?.UpdatedBy;
                instanceLog.RefId = searchResponse.Id;
                instanceLog.Version = searchResponse.Version;

                _ = await _elasticClient.BulkAsync(r =>
                    r.
                    Index<TDBEntLog>(r => r.Document(instanceLog)).
                    Update<TDBEnt>(r => r.Id(id).Doc(instance)));
            }

            var finalResponse = await _elasticClient.GetAsync<TDBEnt>(id);
            if (finalResponse == null) throw new Exception($"FATAL Error occured. Failed to update {id} and instance is not available any more");
            finalResponse.Source.Id = finalResponse.Id;
            return finalResponse.Source;
        }


        public async Task<TDBEnt> Patch(string id, JsonPatchDocument<TEnt> data)
        {
            var searchResponse = await _elasticClient.GetAsync<TDBEnt>(id);
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
            var instance = Activator.CreateInstance(typeof(TDBEnt)) as TDBEnt;
            if (instance == null) throw new Exception("Unable to inicialize TDBEnt");
            instance.Created = searchResponse.Source.Created;
            instance.Updated = DateTimeOffset.Now;
            instance.Data = searchResponse.Source.Data;
            instance.UpdatedBy = "";

            var instanceLog = Activator.CreateInstance(typeof(TDBEntLog)) as TDBEntLog;
            if (instanceLog == null) throw new Exception("Unable to inicialize TDBEntLog");
            instanceLog.Created = searchResponse.Source.Created;
            instanceLog.Updated = DateTimeOffset.Now;
            instanceLog.Data = orig;
            instanceLog.UpdatedBy = searchResponse.Source.UpdatedBy;
            instanceLog.RefId = searchResponse.Id;
            instanceLog.Version = searchResponse.Version;

            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<TDBEntLog>(r => r.Document(instanceLog)).
                Update<TDBEnt>(r => r.Id(id).Doc(instance)));

            var finalResponse = await _elasticClient.GetAsync<TDBEnt>(id);
            if (finalResponse == null) throw new Exception($"FATAL Error occured. Failed to update {id} and instance is not available any more");
            finalResponse.Source.Id = finalResponse.Id;
            return finalResponse.Source;
        }
        public async Task<TDBEnt> Delete(string id)
        {
            //var deleteResponse = await _elasticClient.DeleteAsync<DBPerson>(id);

            var searchResponse = await _elasticClient.GetAsync<TDBEnt>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            var instanceLog = Activator.CreateInstance(typeof(TDBEntLog)) as TDBEntLog;

            if (instanceLog == null) throw new Exception("Unable to inicialize TDBEntLog");
            instanceLog.Created = searchResponse.Source.Created;
            instanceLog.Updated = DateTimeOffset.Now;
            instanceLog.Data = searchResponse.Source.Data;
            instanceLog.UpdatedBy = searchResponse.Source.UpdatedBy;
            instanceLog.RefId = searchResponse.Id;
            instanceLog.Version = searchResponse.Version;


            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<TDBEntLog>(r => r.Document(instanceLog)).
                Delete<TDBEnt>(r => r.Id(id)));

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
