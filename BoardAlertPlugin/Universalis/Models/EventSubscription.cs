using MongoDB.Bson.Serialization.Attributes;

namespace BoardAlertPlugin.Universalis.Models;
internal class EventSubscription
{
    [BsonElement("event")]
    public string Event { get; set; }

    [BsonElement("channel")]
    public string Channel { get; set; }
}
