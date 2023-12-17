using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace EmoteOrganizer.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    internal IDalamudTextureWrap IconSample { get; set; }
    private int IconSize;
    
    public ConfigWindow(Plugin plugin) : base(
        "Settings", ImGuiWindowFlags.NoResize)
    {
        this.Size = new Vector2(200, 160);
        this.Configuration = plugin.Configuration;

        var emote = plugin.EmoteService.GetById(4);
        this.IconSample = plugin.IconService.GetIcon(emote!.Icon)!;
        this.IconSize = plugin.Configuration.IconSize;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.SliderInt("##size", ref IconSize, 10, 100);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            IconSize = Math.Clamp(IconSize, 10, 100); 
            if (this.IconSize != Configuration.IconSize)
            {
                Configuration.IconSize = this.IconSize;
                Configuration.Save();
            }
        }
        ImGui.Image(IconSample.ImGuiHandle, new Vector2(IconSize, IconSize));
    }
}
