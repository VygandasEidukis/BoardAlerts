using BoardAlertPlugin.Models;
using BoardAlertPlugin.Universalis.Models;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Linq;
using System.Numerics;

namespace BoardAlertPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration Configuration;

    public Plugin CurrentPlugin { get; }

    private bool isRunning;

    public ConfigWindow(Plugin plugin) : base(
        "A Wonderful Configuration Window", ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.AlwaysAutoResize)
    {
        Size = new Vector2(500, 400);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
        CurrentPlugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        DrawStartStopWebSocket();
        DrawWorldSelector();
        DrawCreateItemToSelect();
    }

    private void DrawStartStopWebSocket()
    {
        if (ImGui.Button(isRunning ? "Stop" : "Start"))
        {
            isRunning = !isRunning;

            if (isRunning)
            {
                CurrentPlugin.UniversalisApi.StartListening();
            }
            else
            {
                CurrentPlugin.UniversalisApi.StopListening();
            }
        }
    }

    private void DrawCreateItemToSelect()
    {
        var removeId = -1;
        if (ImGui.BeginTable("table1", 4, ImGuiTableFlags.NoKeepColumnsVisible | ImGuiTableFlags.SizingMask | ImGuiTableFlags.SizingFixedFit))
        {
            var columnCount = 4;

            if (Configuration.SelectedProducts.Count == 0)
            {
                Configuration.SelectedProducts.Add(new SelectedProduct());
                Configuration.Save();
            }

            for (int row = 0; row < Configuration.SelectedProducts.Count; row++)
            {
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.PushID(row * columnCount);
                var id = Configuration.SelectedProducts[row].Id.ToString();
                if (ImGui.InputText("", ref id, 32))
                {
                    if (uint.TryParse(id, out var idnum))
                    {
                        Configuration.SelectedProducts[row].Id = uint.Parse(id);
                        Configuration.Save();
                    }
                }
                ImGui.PopID();

                ImGui.TableSetColumnIndex(1);
                ImGui.PushID(row * columnCount + 1);
                var price = Configuration.SelectedProducts[row].MaxPrice.ToString();
                if (ImGui.InputText("", ref price, 32))
                {
                    if (uint.TryParse(price, out var priceNum))
                    {
                        Configuration.SelectedProducts[row].MaxPrice = priceNum;
                        Configuration.Save();
                    }
                }
                ImGui.PopID();

                ImGui.TableSetColumnIndex(2);
                ImGui.PushID(row * columnCount + 2);

                var item = GetItems().GetRow(Configuration.SelectedProducts[row].Id);
                if (item != null)
                {
                    ImGui.Text(item.Name);
                }
                else
                {
                    ImGui.Text("Unknown product");
                }
                ImGui.PopID();

                ImGui.TableSetColumnIndex(3);
                ImGui.PushID(row * columnCount + 3);
                if (ImGui.Button("Delete"))
                {
                    removeId = row;
                }
                ImGui.PopID();
            }
            ImGui.EndTable();
        }
        if (removeId != -1)
        {
            Configuration.SelectedProducts.RemoveAt(removeId);
            removeId = -1;
            Configuration.Save();
        }

        if (ImGui.Button("Add"))
        {
            Configuration.SelectedProducts.Add(new SelectedProduct());
            Configuration.Save();
        }
    }

    private static ExcelSheet<Item>? GetItems()
    {
        return Plugin.Data.Excel.GetSheet<Item>();
    }

    private static ExcelSheet<World>? GetWorlds()
    {
        return Plugin.Data.Excel.GetSheet<World>();
    }

    private void DrawWorldSelector()
    {
        // can't ref a property, so use a local copy
        var configValue = Configuration.MarketWorlds ?? "";
        if (ImGui.InputText("World ids for markets", ref configValue, 135))
        {
            var worldIds = configValue.Replace(" ", string.Empty).Split(",");
            if (worldIds.Any(val => !int.TryParse(val, out _)) && !worldIds.Any())
            {
                PluginLog.Error("One or more worlds are not numbers");
                return;
            }

            Configuration.MarketWorlds = configValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            Configuration.Save();
        }
    }

    public void HandleListing(ProductListing listing)
    {
        try
        {
            var configuredItem = Configuration.SelectedProducts.FirstOrDefault(x => x.Id == listing.ItemId);
            if (configuredItem == null)
            {
                return;
            }

            if (listing.Listings.Count > 0)
            {
                var product = listing.Listings[0];
                if (product.PricePerUnit > configuredItem.MaxPrice)
                {
                    return;
                }

                if (Configuration.AllowedWorlds.Length > 0 && Configuration.AllowedWorlds.Any(x => x == listing.WorldId))
                {
                    return;
                }

                foreach (var item in listing.Listings)
                {
                    var entryF = new XivChatEntry()
                    {
                        Message = $"Found item '{GetItems().GetRow(listing.ItemId).Name}' listed for {item.PricePerUnit}, x{item.Quantity} in {GetWorlds().GetRow(listing.WorldId).Name}|{listing.WorldId}",
                        Name = SeString.Empty,
                        Type = XivChatType.Echo,
                    };
                    Plugin.Chat.PrintChat(entryF);
                    Plugin.Chat.UpdateQueue();
                }
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"{ex.Message} \n {ex.StackTrace}");
        }
    }
}
