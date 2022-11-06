using MongoDB.Bson.Serialization.Attributes;

namespace BoardAlertPlugin.Universalis.Models;
internal class SocketMessage
{
    [BsonElement("event")]
    public string Event { get; set; }
}
