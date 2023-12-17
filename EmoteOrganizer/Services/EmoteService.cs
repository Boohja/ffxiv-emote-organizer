using System;
using System.Collections.Generic;
using System.Linq;
using EmoteOrganizer.Structures;
using Lumina.Excel.GeneratedSheets;

namespace EmoteOrganizer.Services;

public class EmoteService
{
    private Plugin Plugin;
    private readonly Dictionary<uint, Emote> emotes;
    // public List<EmoteGroup> groups = new();
    
    public EmoteService(Plugin plugin)
    {
        this.Plugin = plugin;
        
        this.emotes = plugin.Data.GetExcelSheet<Emote>()!.Where(
            emote => emote.Icon > 0 && emote.TextCommand.Value != null && emote.Icon > 0
        ).ToDictionary(e => e.RowId, e => e);

        // this.groups = Plugin.Configuration.EmoteGroups;
        // categories.Add(0, new EmoteGroup());
    }

    public Emote? GetById(uint emoteId)
    {
        return emotes.GetValueOrDefault(emoteId);
    }
    
    public IEnumerable<Emote> GetAllEmotes()
    {
        return this.emotes.Values;
    }
    
    public List<EmoteGroup> GetCustomGroups()
    {
        return Plugin.Configuration.EmoteGroups;
    }
    
    public IEnumerable<Emote> GetByCategory(int? groupId)
    {
        if (groupId == null)
        {
            return Enumerable.Empty<Emote>();
        }
        if (groupId == 0)
        {
            return this.emotes.Values;
        }
        var result = new List<Emote>();
        if (groupId == -1)
        {
            List<uint> assigned = new List<uint>();
            foreach (var eg in Plugin.Configuration.EmoteGroups)
            {
                assigned = assigned.Concat(eg.Emotes).ToList();
            }

            foreach (var emoteId in emotes.Keys.Where(id => !assigned.Contains(id)))
            {
                result.Add(this.GetById(emoteId)!);
            }
        }
        else if (groupId == -2)
        {
            foreach (var emoteId in Plugin.Configuration.History)
            {
                result.Add(this.GetById(emoteId)!);
            }
        }
        else
        {
            var group = Plugin.Configuration.EmoteGroups.Find(g => g.Id == groupId);
            
            foreach (var emoteId in group.Emotes)
            {
                result.Add(this.GetById(emoteId)!);
            }
        }
        
        return result;
    }

    public int GetGroupSize(int? groupId)
    {
        if (groupId == null)
        {
            return 0;
        }
        if (groupId <= 0)
        {
            return this.GetByCategory(groupId).Count();
        }

        var group = Plugin.Configuration.EmoteGroups.Find(g => g.Id == groupId);
        return group.Emotes.Count;
    }
    
    public string GetGroupName(int? groupId)
    {
        if (groupId == null)
        {
            return "";
        }
        if (groupId == 0)
        {
            return "All";
        }
        if (groupId == -1)
        {
            return "Not assigned";
        }
        if (groupId == -2)
        {
            return "History";
        }
        
        var group = Plugin.Configuration.EmoteGroups.Find(g => g.Id == groupId);
        return group.Name;
    }

    private bool GroupExists(int groupId)
    {
        if (groupId <= 0)
        {
            return true;
        }

        return Plugin.Configuration.EmoteGroups.Any(g => g.Id == groupId);
    }

    private uint GetNextGroupId()
    {
        if (Plugin.Configuration.EmoteGroups.Count == 0)
        {
            return 1;
        }
        int maxId = (int)Plugin.Configuration.EmoteGroups.Max(g => g.Id) + 1;
        return (uint) Enumerable.Range(1, maxId).FirstOrDefault(i => !GroupExists(i));
    }

    public void SetCurrentGroup(int? groupId)
    {
        Plugin.Configuration.CurrentGroup = groupId;
        Plugin.Configuration.Save();
    }

    public void AddGroup(string newGroupName)
    {
        uint newId = GetNextGroupId();

        var newGroup = new EmoteGroup(newId, newGroupName);
        Plugin.Configuration.EmoteGroups.Add(newGroup);
        Plugin.Configuration.CurrentGroup = (int) newId;
        Plugin.Configuration.Save();
    }

    public void EditGroup(int groupId, string newGroupName)
    {
        if (groupId <= 0)
        {
            return;
        }
        var group = Plugin.Configuration.EmoteGroups.Find(g => g.Id == groupId);
        var idx = Plugin.Configuration.EmoteGroups.IndexOf(group);
        group.Name = newGroupName;
        Plugin.Configuration.EmoteGroups[idx] = group;
        Plugin.Configuration.Save();
    }
    
    public bool GroupHasEmote(int groupId, uint emoteId)
    {
        EmoteGroup group = Plugin.Configuration.EmoteGroups.Find(g => g.Id == groupId);
        return group.Emotes.Contains(emoteId);
    }
    
    public void ToggleGroup(uint emoteId, int? groupId)
    {
        if (groupId is null or <= 0)
        {
            return;
        }

        EmoteGroup group = Plugin.Configuration.EmoteGroups.Find(g => g.Id == groupId);
        if (group.Emotes.Contains(emoteId))
        {
            group.Emotes.Remove(emoteId);
        }
        else
        {
            group.Emotes.Add(emoteId);
        }
        Plugin.Configuration.Save();
    }
    
    public void DeleteGroup(int? groupId)
    {
        if (groupId is null or <= 0)
        {
            return;
        }

        Plugin.Configuration.EmoteGroups.RemoveAll(g => g.Id == groupId);
        Plugin.Configuration.CurrentGroup = null;
        Plugin.Configuration.Save();
    }

    public void MoveGroupUp(int? groupId)
    {
        MoveGroup(groupId, true);
    }
    
    public void MoveGroupDown(int? groupId)
    {
        MoveGroup(groupId, false);
    }

    private void MoveGroup(int? groupId, bool moveUp)
    {
        if (groupId is null or <= 0)
        {
            return;
        }
        EmoteGroup group = Plugin.Configuration.EmoteGroups.Find(g => g.Id == groupId);
        int idx = Plugin.Configuration.EmoteGroups.IndexOf(group);
        if (moveUp)
        {
            if (idx <= 0) return;
            Plugin.Configuration.EmoteGroups.Insert(idx - 1, group);
            Plugin.Configuration.EmoteGroups.RemoveAt(idx + 1);
        }
        else
        {
            if (idx >= Plugin.Configuration.EmoteGroups.Count - 1) return;
            Plugin.Configuration.EmoteGroups.Insert(idx + 2, group);
            Plugin.Configuration.EmoteGroups.RemoveAt(idx);
        }
        Plugin.Configuration.Save();
    }

    public void RunEmote(Emote emote)
    {
        if (emote.TextCommand.Value is null)
        {
            Plugin.Log.Debug($"No command found for {emote.Name}");
            return;
        }
        
        Plugin.Log.Debug($"clicked! {emote.TextCommand.Value!.Command}");
        try
        {
            Plugin.XivCommon.Functions.Chat.SendMessage(emote.TextCommand.Value.Command.ToString());
            LogEmote(emote.RowId);
        }
        catch (ArgumentException e)
        {
            Plugin.Log.Error(e.ToString());
        }
    }

    private void LogEmote(uint emoteId)
    {
        Plugin.Log.Debug($"Logging emote {emoteId}, logged {Plugin.Configuration.History.Count()}");
        Plugin.Configuration.History.RemoveAll(id => id == emoteId);
        Plugin.Configuration.History.Insert(0, emoteId);
        Plugin.Configuration.Save();
    }
    
    public void MoveUp(uint emoteId, int? groupId)
    {
        MoveEmote(emoteId, true, groupId);
    }
    
    public void MoveDown(uint emoteId, int? groupId)
    {
        MoveEmote(emoteId, false, groupId);
    }

    private void MoveEmote(uint emoteId, bool moveUp, int? groupId)
    {
        if (groupId is null or <= 0)
        {
            return;
        }
        EmoteGroup group = Plugin.Configuration.EmoteGroups.Find(g => g.Id == groupId);
        var emoteIdx = group.Emotes.IndexOf(emoteId);
        if (moveUp)
        {
            if (emoteIdx <= 0) return;
            group.Emotes.Insert(emoteIdx - 1, emoteId);
            group.Emotes.RemoveAt(emoteIdx + 1);
        }
        else
        {
            if (emoteIdx >= group.Emotes.Count - 1) return;
            group.Emotes.Insert(emoteIdx + 2, emoteId);
            group.Emotes.RemoveAt(emoteIdx);
        }
        Plugin.Configuration.Save();
    }
}
