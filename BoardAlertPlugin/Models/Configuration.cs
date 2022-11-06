using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoardAlertPlugin.Models
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public string MarketWorlds { get; set; }
        public List<SelectedProduct> SelectedProducts { get; set; } = new List<SelectedProduct>();

        public uint[] AllowedWorlds
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MarketWorlds))
                {
                    return new uint[0];
                }

                var str = MarketWorlds.Replace(" ", "").Split(",");

                return str.Select(x => uint.Parse(x)).ToArray();
            }
        }

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
        }

        public void Save()
        {
            PluginInterface!.SavePluginConfig(this);
        }
    }
}
