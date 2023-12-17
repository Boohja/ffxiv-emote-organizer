using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace EmoteOrganizer.Widgets;

public class EmoteTable
{
    public Plugin Plugin { get; set; }
    public Emote? EmoteContext { get; set; }
    internal EmotePopup EmotePopup { get; set; }
    
    public EmoteTable(Plugin plugin)
    {
        this.Plugin = plugin;
        this.EmotePopup = new EmotePopup(plugin);
    }
    
    public void Draw(IEnumerable<Emote> emotes)
    {
        if (ImGui.BeginTable(
            "My Table", 
            4, 
            ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingFixedFit, 
            new Vector2(400 * ImGuiHelpers.GlobalScale, 0)
        ))
        {
            if (ImGui.BeginPopup("emoteContextMenu", ImGuiWindowFlags.Tooltip))
            {
                EmotePopup.Draw(EmoteContext);
                ImGui.EndPopup();    
            }
            
            ImGui.TableSetupColumn("##iconCol");
            ImGui.TableSetupColumn("##nameCol");
            ImGui.TableSetupColumn("##commandCol");
            ImGui.TableSetupColumn("##debugCol");
            
            foreach (Emote emote in emotes)
            {
                var texture = Plugin.IconService.GetIcon(emote.Icon);
                if (texture == null || emote.TextCommand.Value == null)
                {
                    continue;
                }
                ImGui.TableNextRow();
                ImGui.BeginPopupContextItem($"row{emote.RowId}");
                ImGui.TableNextColumn();
                if (ImGui.ImageButton(texture.ImGuiHandle, new Vector2(Plugin.Configuration.IconSize, Plugin.Configuration.IconSize)))
                {
                    Plugin.EmoteService.RunEmote(emote);
                }

                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    Plugin.Log.Debug($"clicking icon {emote.RowId}");
                    this.EmoteContext = emote;
                    ImGui.OpenPopup("emoteContextMenu");
                }
                ImGui.TableNextColumn();
                ImGui.SameLine(0, 10);
                ImGui.TextUnformatted(emote.Name.ToString());
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(emote.TextCommand.Value.Command.ToString());
                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton((int) emote.RowId, FontAwesomeIcon.ArrowUp))
                {
                    Plugin.Log.Debug($"move up {emote.Name}");
                    Plugin.EmoteService.MoveUp(emote.RowId, Plugin.Configuration.CurrentGroup);
                }
                ImGui.SameLine();
                if (ImGuiComponents.IconButton((int) emote.RowId, FontAwesomeIcon.ArrowDown))
                {
                    Plugin.Log.Debug($"move down {emote.Name}");
                    Plugin.EmoteService.MoveDown(emote.RowId, Plugin.Configuration.CurrentGroup);
                }
            }
            
            ImGui.EndTable();
        }
    }
}
