using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using EmoteOrganizer.Enums;
using EmoteOrganizer.Structures;

namespace EmoteOrganizer
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public int? CurrentGroup { get; set; } = 0;
        public List<EmoteGroup> EmoteGroups { get; set; } = new();
        public int IconSize { get; set; } = 30;
        public List<uint> History { get; set; } = new();

        public EmoteLayout Layout { get; set; } = EmoteLayout.Table;

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
