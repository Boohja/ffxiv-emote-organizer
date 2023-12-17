using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace EmoteOrganizer.Widgets;

public class EmoteGrid
{
    public Plugin Plugin { get; set; }
    public Emote? EmoteContext { get; set; }
    internal EmotePopup EmotePopup { get; set; }
    
    public EmoteGrid(Plugin plugin)
    {
        this.Plugin = plugin;
        this.EmotePopup = new EmotePopup(plugin);
    }

    public void Draw(IEnumerable<Emote> emotes)
    {
        var style = ImGui.GetStyle();
        var buttonSz = new Vector2(Plugin.Configuration.IconSize);
        float windowVisibleX = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
        
        if (ImGui.BeginPopup("emoteContextMenu", ImGuiWindowFlags.Tooltip))
        {
            EmotePopup.Draw(EmoteContext);
            ImGui.EndPopup();    
        }
        foreach (Emote emote in emotes)
        {
            var texture = Plugin.IconService.GetIcon(emote.Icon);
            if (ImGui.ImageButton(texture!.ImGuiHandle, buttonSz))
            {
                Plugin.EmoteService.RunEmote(emote);
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(emote.Name);
            
            float lastButtonX = ImGui.GetItemRectMax().X;
            float nextButtonX = lastButtonX + style.ItemSpacing.X + buttonSz.X; // Expected position if next button was on same line
            if (nextButtonX < windowVisibleX)
                ImGui.SameLine();
            
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                Plugin.Log.Debug($"clicking icon {emote.RowId}");
                this.EmoteContext = emote;
                ImGui.OpenPopup("emoteContextMenu");
            }
        }
    }
}
