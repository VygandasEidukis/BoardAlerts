using Dalamud.Logging;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using SamplePlugin.Universalis.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Websocket.Client;
using Websocket.Client.Models;

namespace SamplePlugin.Universalis;
public class UniversalisApi
{
    private readonly WebsocketClient _websocketClient;
    private static readonly Uri _apiUri = new Uri("wss://universalis.app/api/ws");
    private static readonly EventSubscription EventSubscription = new EventSubscription
    {
        Event = "subscribe",
        Channel = "listings/add"
    };

    private readonly List<string> strings = new List<string>();

    public Action<ProductListing> ListingAction { get; }

    public UniversalisApi(Action<ProductListing> listingAction)
    {
        ListingAction = listingAction;
        _websocketClient = new WebsocketClient(_apiUri);
    }

    public void StartListening()
    {
        Task.Run(() =>
        {
            _websocketClient.ReconnectionHappened.Subscribe(info => ReconnectionHappenedEvent(info));
            _websocketClient.MessageReceived.Subscribe(msg => MessageReceivedEvent(msg));
            _websocketClient.Start();
            Task.Run(() => _websocketClient.Send(BsonSerialize(EventSubscription)));
        });
    }

    private void MessageReceivedEvent(ResponseMessage msg)
    {
        var listings = ReadMarketListings(msg.Binary);

        strings.Add(listings.ItemId.ToString());

        if (strings.Count > 30)
        {
            PluginLog.Information(string.Join(", ", strings));
            strings.Clear();
        }

        ListingAction?.Invoke(listings);
    }

    private void ReconnectionHappenedEvent(ReconnectionInfo info)
    {
        PluginLog.Warning($"Reconnection happened: {info.Type}");
    }

    private byte[] BsonSerialize(EventSubscription obj)
    {
        var stream = new MemoryStream();
        using var writer = new BsonBinaryWriter(stream);
        BsonSerializer.Serialize(writer, obj.GetType(), obj);

        return stream.ToArray();
    }

    private ProductListing ReadMarketListings(byte[] bytes)
    {
        var data = BsonSerializer.Deserialize<ProductListing>(bytes);
        return data;
    }

    public void StopListening()
    {
        _websocketClient.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "");
    }
}
