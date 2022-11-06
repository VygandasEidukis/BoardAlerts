using MongoDB.Bson.Serialization.Attributes;

namespace SamplePlugin.Universalis.Models;
internal class SocketMessage
{
    [BsonElement("event")]
    public string Event { get; set; }
}
