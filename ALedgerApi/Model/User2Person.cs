using RestDWH.Base.Attributes;

namespace ALedgerApi.Model
{
    [RestDWHEntity("User2Person", endpointGet: "user2Person", endpointUpsert: "user2Person", endpointPatch: "user2Person", endpointPost: "user2Person", endpointGetById: "user2Person/{id}", endpointDelete: "user2Person")]
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
