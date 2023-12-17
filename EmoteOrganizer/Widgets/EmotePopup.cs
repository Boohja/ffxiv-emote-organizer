using System.Numerics;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace EmoteOrganizer.Widgets;

public class EmotePopup
{
    internal Plugin Plugin { get; set; }
    
    public EmotePopup(Plugin plugin)
    {
        this.Plugin = plugin;
    }


    public void Draw(Emote? emote)
    {
        if (emote == null)
        {
            return;
        }
        var texture = Plugin.IconService.GetIcon(emote.Icon);
        ImGui.Image(texture!.ImGuiHandle, new Vector2(40, 40));
        ImGui.SameLine();
        ImGui.BeginGroup();
        ImGui.Text(emote.Name);
        ImGui.Text($"{emote.TextCommand.Value!.Command} {emote.TextCommand.Value!.ShortCommand} {emote.TextCommand.Value!.Alias}");
        ImGui.EndGroup();
        ImGui.Separator();
                    
        foreach (var group in Plugin.EmoteService.GetCustomGroups())
        {
            var contains = Plugin.EmoteService.GroupHasEmote((int) group.Id, emote.RowId);
            if (ImGui.Checkbox(group.Name, ref contains))
            {
                Plugin.EmoteService.ToggleGroup(emote.RowId, (int) group.Id);
            }
        }
    }
}
