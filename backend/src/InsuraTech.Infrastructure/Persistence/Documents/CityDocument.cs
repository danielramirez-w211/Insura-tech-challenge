using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InsuraTech.Infrastructure.Persistence.Documents
{
    public class CityDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string Name       { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Department { get; set; } = null!;
    }
}
