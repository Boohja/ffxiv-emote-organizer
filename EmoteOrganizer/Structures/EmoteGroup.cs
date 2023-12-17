using System.Collections.Generic;

namespace EmoteOrganizer.Structures;

public struct EmoteGroup
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public List<uint> Emotes { get; set; }

    public EmoteGroup(uint id, string name)
    {
        Id = id;
        Name = name;
        Emotes = new List<uint>();
    }
}
