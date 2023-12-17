using System.Collections.Generic;
using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;

namespace EmoteOrganizer.Services;

public class IconService
{
    private readonly Dictionary<ushort, IDalamudTextureWrap?> iconCache = new();
    internal readonly ITextureProvider TextureProvider;
    
    public IconService(ITextureProvider textureProvider)
    {
        this.TextureProvider = textureProvider;
    }
    
    public IDalamudTextureWrap? GetIcon(ushort iconId)
    {
        if (iconCache.TryGetValue(iconId, out var iconWrap))
            return iconWrap;

        iconWrap = TextureProvider.GetIcon(iconId);
        iconCache[iconId] = iconWrap;
        return iconWrap;
    }

    public void Dispose()
    {
        
        foreach (var icon in iconCache.Values)
        {
            icon?.Dispose();
        }
        iconCache.Clear();
    }
}
