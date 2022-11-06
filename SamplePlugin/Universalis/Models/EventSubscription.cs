using MongoDB.Bson.Serialization.Attributes;

namespace SamplePlugin.Universalis.Models;
internal class EventSubscription
{
    [BsonElement("event")]
    public string Event { get; set; }

    [BsonElement("channel")]
    public string Channel { get; set; }
}
