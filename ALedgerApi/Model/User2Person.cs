using ALedgerApi.Events;
using RestDWH.Base.Attributes;
using RestDWH.Elastic.Attributes.Endpoints;
using RestDWHBase.Attributes.Endpoints;

namespace ALedgerApi.Model
{
    [RestDWHEndpointGet]
    [RestDWHEndpointGetById]
    [RestDWHEndpointUpsert]
    [RestDWHEndpointPatch]
    [RestDWHEndpointDelete]
    [RestDWHEndpointProperties]
    [RestDWHEndpointElasticQuery]
    [RestDWHEndpointPost]
    [RestDWHEndpointElasticPropertiesQuery]

    [RestDWHEntity("User2Person", typeof(User2PersonEvents), apiName: "user-2-person")]
    public class User2Person
    {
        public string UserId { get; set; }
        public string PersonId { get; set; }
        /// <summary>
        /// Admin, User
        /// </summary>
        public string Role { get; set; }
    }
}
