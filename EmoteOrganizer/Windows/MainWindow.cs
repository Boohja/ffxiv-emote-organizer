using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using EmoteOrganizer.Enums;
using EmoteOrganizer.Widgets;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace EmoteOrganizer.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private bool Editing { get; set; } = false;
    public string Filter = "";
    private string NewGroupName = "";
    
    internal EmoteTable EmoteTable { get; set; }
    internal EmoteGrid EmoteGrid { get; set; }
    
    public MainWindow(Plugin plugin) : base("Emote Organizer", ImGuiWindowFlags.MenuBar )
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(250, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.Plugin = plugin;
        this.EmoteTable = new EmoteTable(plugin);
        this.EmoteGrid = new EmoteGrid(plugin);
    }

    public void Dispose()
    {
        Plugin.Log.Debug("Disposing MainWindow");
        Plugin.IconService.Dispose();
    }
    
    public override void Draw()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Table", "", Plugin.Configuration.Layout == EmoteLayout.Table))
                {
                    Plugin.Configuration.Layout = EmoteLayout.Table;
                    Plugin.Configuration.Save();
                }

                if (ImGui.MenuItem("Grid", "", Plugin.Configuration.Layout == EmoteLayout.Grid))
                {
                    Plugin.Configuration.Layout = EmoteLayout.Grid;
                    Plugin.Configuration.Save();
                }
                ImGui.EndMenu();
            }
            if (ImGui.MenuItem("Settings"))
            {
                Plugin.DrawConfigUI();
            }
            ImGui.EndMenuBar();
        }
        ImGui.Text($"Filter: {Filter}");
        
        // @see https://skia.googlesource.com/external/github.com/ocornut/imgui/+/refs/tags/v1.78/imgui_demo.cpp#4570
        {
            ImGui.BeginGroup();
            if (ImGui.BeginChild("leftPane", new Vector2(140, -ImGui.GetFrameHeightWithSpacing()), true))
            {
                foreach(var group in Plugin.Configuration.EmoteGroups)
                {
                    if (ImGui.Selectable($"{group.Name}  ({Plugin.EmoteService.GetGroupSize((int) group.Id)})", Plugin.Configuration.CurrentGroup == group.Id))
                    {
                        Plugin.EmoteService.SetCurrentGroup((int) group.Id);
                    }
                }
                ImGui.Separator();
                if (ImGui.Selectable($"{Plugin.EmoteService.GetGroupName(-2)}  ({Plugin.EmoteService.GetGroupSize(-2)})", Plugin.Configuration.CurrentGroup == -2))
                {
                    Plugin.EmoteService.SetCurrentGroup(-2);
                }
                if (ImGui.Selectable($"{Plugin.EmoteService.GetGroupName(0)}  ({Plugin.EmoteService.GetGroupSize(0)})", Plugin.Configuration.CurrentGroup == 0))
                {
                    Plugin.EmoteService.SetCurrentGroup(0);
                }
                if (ImGui.Selectable($"{Plugin.EmoteService.GetGroupName(-1)}  ({Plugin.EmoteService.GetGroupSize(-1)})", Plugin.Configuration.CurrentGroup == -1))
                {
                    Plugin.EmoteService.SetCurrentGroup(-1);
                }
            }
            ImGui.EndChild();
            
            if (Editing)
            {
                ImGui.SetKeyboardFocusHere();
                ImGui.SetNextItemWidth(140);
                if (ImGui.InputText("", ref NewGroupName, 12, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (Plugin.Configuration.CurrentGroup != null)
                    {
                        Plugin.EmoteService.EditGroup(Plugin.Configuration.CurrentGroup.Value, NewGroupName);   
                    }
                    else
                    {
                        Plugin.EmoteService.AddGroup(NewGroupName);
                    }
                    Editing = false;
                }
            }
            else
            {
                if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus))
                {
                    Editing = true;
                    NewGroupName = "";
                    Plugin.Configuration.CurrentGroup = null;
                }
                ImGui.SameLine();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.Pen))
                {
                    NewGroupName = Plugin.EmoteService.GetGroupName(Plugin.Configuration.CurrentGroup);
                    Editing = true;
                }
                ImGui.SameLine();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.ArrowUp))
                {
                    Plugin.EmoteService.MoveGroupUp(Plugin.Configuration.CurrentGroup);
                }
                ImGui.SameLine();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.ArrowDown))
                {
                    Plugin.EmoteService.MoveGroupDown(Plugin.Configuration.CurrentGroup);
                }
                ImGui.SameLine();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.Trash))
                {
                    Plugin.EmoteService.DeleteGroup(Plugin.Configuration.CurrentGroup);
                }
            }
            ImGui.EndGroup();
        }
        ImGui.SameLine();

        {
            ImGui.BeginGroup();
            ImGui.BeginChild("itemView", new Vector2(0, -ImGui.GetFrameHeightWithSpacing()));

            if (Plugin.Configuration.Layout == EmoteLayout.Table)
            {
                EmoteTable.Draw(GetFilteredEmotes());
            }
            else if (Plugin.Configuration.Layout == EmoteLayout.Grid)
            {
                EmoteGrid.Draw(GetFilteredEmotes());
            }
            
            ImGui.EndChild();
            
            if (ImGui.InputText("Filter", ref Filter, 15))
            {
                Editing = false;
            }
            
            ImGui.EndGroup();
        }
    }

    private IEnumerable<Emote> GetFilteredEmotes()
    {
        var result = Plugin.EmoteService.GetByCategory(Plugin.Configuration.CurrentGroup);
        if (Filter.Length > 0)
        {
            return result.Where(e => e.Name.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase));
        }

        return result;
    }
}
