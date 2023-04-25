/*
using ALedgerApi.Model.DB;
using Nest;

namespace ALedgerApi.Repository
{
    public class AddressRepository : BaseRepository<
            Model.Address.Comm.Address,
            Model.DB.DBBase<Model.Address.Comm.Address>,
            Model.DB.DBListBase<Model.Address.Comm.Address, Model.DB.DBBase<Model.Address.Comm.Address>>,
            DBBaseLog<Model.Address.Comm.Address>
        >
    {
        public AddressRepository(IElasticClient elasticClient) : base(elasticClient)
        {
        }
    }
}
/*
using ALedgerApi.Extensions;
using ALedgerApi.Model.Address.DB;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Nest;

namespace ALedgerApi.Repository
{
    public class AddressRepository
    {
        private readonly IElasticClient _elasticClient;

        public AddressRepository(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<DBAddressList> Get(int from = 0, int size = 10, string query = "*")
        {

            var searchResponse = await _elasticClient.SearchAsync<DBAddress>(s => s
                //.Index("address-main")
                .From(from)
                .Size(size)
                .QueryOnQueryString(query)
            );

            var count = await _elasticClient.CountAsync<DBAddress>(s => s
                //.Index("address-main")
                .QueryOnQueryString(query)
                );

            var list = searchResponse.Hits.Select(s => { s.Source.Id = s.Id; return s.Source; }).ToArray();
            return new DBAddressList() { Results = list, From = from, Size = size, TotalCount = count.Count };
        }


        public async Task<DBAddress?> GetAddress(string id)
        {
            var searchResponse = await _elasticClient.GetAsync<DBAddress>(id);//
            if (searchResponse.Source == null) { throw new Exception("Not found"); }
            searchResponse.Source.Id = searchResponse.Id;
            return searchResponse.Source;
        }
        public async Task<DBAddress> Post(Model.Address.Comm.Address address)
        {
            var now = DateTimeOffset.Now;
            var db = new DBAddress()
            {
                Created = now,
                Updated = now,
                Address = address,
                UpdatedBy = "",
            };
            var indexResponse = await _elasticClient.IndexDocumentAsync(db);
            var searchResponse = await _elasticClient.GetAsync<DBAddress>(indexResponse.Id);
            searchResponse.Source.Id = indexResponse.Id;
            return searchResponse.Source;
        }
        public async Task<DBAddress> Put(string id, Model.Address.Comm.Address address)
        {
            var searchResponse = await _elasticClient.GetAsync<DBAddress>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            if (address == searchResponse.Source.Address)
            {
                return searchResponse.Source;// do not update if the docs are equal
            }
            var db = new DBAddress()
            {
                Created = searchResponse.Source.Created,
                Updated = DateTimeOffset.Now,
                Address = address,
                UpdatedBy = "",
            };
            var dbLog = new DBAddressLog()
            {
                Created = searchResponse.Source.Created,
                Updated = DateTimeOffset.Now,
                Address = searchResponse.Source.Address,
                UpdatedBy = searchResponse.Source.UpdatedBy,
                RefId = searchResponse.Id,
                Version = searchResponse.Version
            };

            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<DBAddressLog>(r => r.Document(dbLog)).
                Update<DBAddress>(r => r.Id(id).Doc(db)));


            var finalResponse = await _elasticClient.GetAsync<DBAddress>(id);
            finalResponse.Source.Id = finalResponse.Id;
            return finalResponse.Source;
        }


        public async Task<DBAddress> Patch(string id, JsonPatchDocument<Model.Address.Comm.Address> address)
        {
            var searchResponse = await _elasticClient.GetAsync<DBAddress>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            var orig = searchResponse.Source.Address.DeepCopy();

            address.ApplyTo(searchResponse.Source.Address);

            if (orig == searchResponse.Source.Address)
            {
                return searchResponse.Source;// do not update if the docs are equal
            }
            var db = new DBAddress()
            {
                Created = searchResponse.Source.Created,
                Updated = DateTimeOffset.Now,
                Address = searchResponse.Source.Address,
                UpdatedBy = "",
            };
            var dbLog = new DBAddressLog()
            {
                Created = searchResponse.Source.Created,
                Updated = DateTimeOffset.Now,
                Address = orig,
                UpdatedBy = searchResponse.Source.UpdatedBy,
                RefId = searchResponse.Id,
                Version = searchResponse.Version
            };

            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<DBAddressLog>(r => r.Document(dbLog)).
                Update<DBAddress>(r => r.Id(id).Doc(db)));


            //var updateResponse = await _elasticClient.UpdateAsync<DBAddress>(id, u => u.Doc(address));

            var finalResponse = await _elasticClient.GetAsync<DBAddress>(id);
            finalResponse.Source.Id = finalResponse.Id;
            return finalResponse.Source;
        }
        public async Task<bool> Delete(string id)
        {
            //var deleteResponse = await _elasticClient.DeleteAsync<DBAddress>(id);

            var searchResponse = await _elasticClient.GetAsync<DBAddress>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            var dbLog = new DBAddressLog()
            {
                Created = searchResponse.Source.Created,
                Updated = DateTimeOffset.Now,
                Address = searchResponse.Source.Address,
                UpdatedBy = searchResponse.Source.UpdatedBy,
                RefId = searchResponse.Id,
                Version = searchResponse.Version
            };

            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<DBAddressLog>(r => r.Document(dbLog)).
                Delete<DBAddress>(r => r.Id(id)));
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
*/