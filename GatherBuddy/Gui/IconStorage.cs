using System;
using System.Collections.Generic;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;

namespace GatherBuddy.Gui;

public class IconStorage : IDisposable
{
    private readonly Dictionary<uint, ISharedImmediateTexture?> _icons;
    public readonly  ITextureProvider                           Provider;

    public IconStorage(ITextureProvider provider, int size = 0)
    {
        _icons    = new Dictionary<uint, ISharedImmediateTexture?>(size);
        Provider = provider;
    }

    public ISharedImmediateTexture? this[uint id]
        => GetTextureFromIconId(id);

    public ISharedImmediateTexture? this[int id]
        => GetTextureFromIconId((uint)id);

    public ISharedImmediateTexture? GetTextureFromIconId(uint iconId, bool highQuality = false, uint stackCount = 0, bool hdIcon = true)
    {
        GameIconLookup gameIconLookup = new GameIconLookup
        {
            IconId = iconId,
            ItemHq = hdIcon,
        };

        return Dalamud.Textures.GetFromGameIcon(gameIconLookup);
    }
    
    public void Dispose()
    {
        //??
    }

    public static IconStorage DefaultStorage { get; private set; } = null!;

    public static void InitDefaultStorage(ITextureProvider provider)
    {
        DefaultStorage = new IconStorage(provider, 1024);
        Icons.Init(Dalamud.GameData,provider,DefaultStorage);
    }
}
