using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace BoardAlertPlugin.Universalis.Models;
public class Materia
{
    /// <summary>
    /// The materia slot.
    /// </summary>
    [BsonElement("slotID")]
    [JsonPropertyName("slotID")]
    public uint SlotId { get; init; }

    /// <summary>
    /// The materia item ID.
    /// </summary>
    [BsonElement("materiaID")]
    [JsonPropertyName("materiaID")]
    public uint MateriaId { get; init; }
}
