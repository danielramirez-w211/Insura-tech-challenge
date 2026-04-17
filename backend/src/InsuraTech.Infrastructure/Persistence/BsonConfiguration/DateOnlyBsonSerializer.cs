using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace InsuraTech.Infrastructure.Persistence.BsonConfiguration;

public sealed class DateOnlyBsonSerializer : SerializerBase<DateOnly>
{
    public override DateOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var str = context.Reader.ReadString();
        return DateOnly.ParseExact(str, "yyyy-MM-dd");
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateOnly value)
    {
        context.Writer.WriteString(value.ToString("yyyy-MM-dd"));
    }
}
