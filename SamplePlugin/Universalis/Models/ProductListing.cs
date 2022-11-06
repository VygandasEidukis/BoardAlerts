using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace SamplePlugin.Universalis.Models;
public class ProductListing
{
    [BsonElement("event")]
    public string Event { get; set; }

    [BsonElement("item")]
    public uint ItemId { get; init; }

    [BsonElement("world")]
    public uint WorldId { get; init; }

    [BsonElement("listings")]
    public IList<Listing> Listings { get; init; }
}
