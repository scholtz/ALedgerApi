using ALedgerApi.Extensions;
using ALedgerApi.Model.Person.DB;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Nest;

namespace ALedgerApi.Repository
{
    public class PersonRepository
    {
        private readonly IElasticClient _elasticClient;

        public PersonRepository(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<DBPersonList> Get(int from = 0, int size = 10, string query = "*")
        {

            var searchResponse = await _elasticClient.SearchAsync<DBPerson>(s => s
                //.Index("person-main")
                .From(from)
                .Size(size)
                .QueryOnQueryString(query)
            );

            var count = await _elasticClient.CountAsync<DBPerson>(s => s
                //.Index("person-main")
                .QueryOnQueryString(query)
                );

            var list = searchResponse.Hits.Select(s => { s.Source.Id = s.Id; return s.Source; }).ToArray();
            return new DBPersonList() { Results = list, From = from, Size = size, TotalCount = count.Count };
        }


        public async Task<DBPerson?> GetPerson(string id)
        {
            var searchResponse = await _elasticClient.GetAsync<DBPerson>(id);//
            if(searchResponse.Source == null) { throw new Exception("Not found"); }
            searchResponse.Source.Id = searchResponse.Id;
            return searchResponse.Source;
        }
        public async Task<DBPerson> Post(Model.Person.Comm.Person person)
        {
            var now = DateTimeOffset.Now;
            var db = new DBPerson()
            {
                Created = now,
                Updated = now,
                Person = person,
                UpdatedBy = "",
            };
            var indexResponse = await _elasticClient.IndexDocumentAsync(db);
            var searchResponse = await _elasticClient.GetAsync<DBPerson>(indexResponse.Id);
            searchResponse.Source.Id = indexResponse.Id;
            return searchResponse.Source;
        }
        public async Task<DBPerson> Put(string id, Model.Person.Comm.Person person)
        {
            var searchResponse = await _elasticClient.GetAsync<DBPerson>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            if (person == searchResponse.Source.Person)
            {
                return searchResponse.Source;// do not update if the docs are equal
            }
            var db = new DBPerson()
            {
                Created = searchResponse.Source.Created,
                Updated = DateTimeOffset.Now,
                Person = person,
                UpdatedBy = "",
            };
            var dbLog = new DBPersonLog()
            {
                Created = searchResponse.Source.Created,
                Updated = DateTimeOffset.Now,
                Person = searchResponse.Source.Person,
                UpdatedBy = searchResponse.Source.UpdatedBy,
                RefId = searchResponse.Id,
                Version = searchResponse.Version
            };

            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<DBPersonLog>(r => r.Document(dbLog)).
                Update<DBPerson>(r => r.Id(id).Doc(db)));


            var finalResponse = await _elasticClient.GetAsync<DBPerson>(id);
            finalResponse.Source.Id = finalResponse.Id;
            return finalResponse.Source;
        }


        public async Task<DBPerson> Patch(string id, JsonPatchDocument<Model.Person.Comm.Person> person)
        {
            var searchResponse = await _elasticClient.GetAsync<DBPerson>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            var orig = searchResponse.Source.Person.DeepCopy();

            person.ApplyTo(searchResponse.Source.Person);

            if (orig == searchResponse.Source.Person)
            {
                return searchResponse.Source;// do not update if the docs are equal
            }
            var db = new DBPerson()
            {
                Created = searchResponse.Source.Created,
                Updated = DateTimeOffset.Now,
                Person = searchResponse.Source.Person,
                UpdatedBy = "",
            };
            var dbLog = new DBPersonLog()
            {
                Created = searchResponse.Source.Created,
                Updated = DateTimeOffset.Now,
                Person = orig,
                UpdatedBy = searchResponse.Source.UpdatedBy,
                RefId = searchResponse.Id,
                Version = searchResponse.Version
            };

            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<DBPersonLog>(r => r.Document(dbLog)).
                Update<DBPerson>(r => r.Id(id).Doc(db)));


            //var updateResponse = await _elasticClient.UpdateAsync<DBPerson>(id, u => u.Doc(person));

            var finalResponse = await _elasticClient.GetAsync<DBPerson>(id);
            finalResponse.Source.Id = finalResponse.Id;
            return finalResponse.Source;
        }
        public async Task<bool> Delete(string id)
        {
            //var deleteResponse = await _elasticClient.DeleteAsync<DBPerson>(id);

            var searchResponse = await _elasticClient.GetAsync<DBPerson>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            var dbLog = new DBPersonLog()
            {
                Created = searchResponse.Source.Created,
                Updated = DateTimeOffset.Now,
                Person = searchResponse.Source.Person,
                UpdatedBy = searchResponse.Source.UpdatedBy,
                RefId = searchResponse.Id,
                Version = searchResponse.Version
            };

            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<DBPersonLog>(r => r.Document(dbLog)).
                Delete<DBPerson>(r => r.Id(id)));
            var errors = updateResponse.ItemsWithErrors.Select(e => e.Error?.Reason).Where(e => !string.IsNullOrEmpty(e));
            var hasErrors = errors?.Any() == true;
            if (hasErrors)
            {
                throw new Exception(string.Join(";", errors));
            }
            return !hasErrors;
        }
    }
}
