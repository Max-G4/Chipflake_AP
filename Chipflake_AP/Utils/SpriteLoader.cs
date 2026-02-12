namespace Chipflake_AP.Utils;

using System.IO;
using UnityEngine;

public static class SpriteLoader
{
    public static Sprite LoadPngAsSprite(string filePath, float pixelsPerUnit = 1)
    {
        Plugin.BepinLogger.LogInfo($"Loading sprite from {filePath}");
        
        if (!File.Exists(filePath))
        {
            Plugin.BepinLogger.LogWarning($"Sprite file not found: {filePath}");
            return null;
        }

        var pngBytes = File.ReadAllBytes(filePath);

        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
        if (!ImageConversion.LoadImage(tex, pngBytes, markNonReadable: false))
        {
            Plugin.BepinLogger.LogWarning($"Failed to decode PNG: {filePath}");
            return null;
        }

        tex.name = Path.GetFileNameWithoutExtension(filePath);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear; // or Bilinear depending on art style

        var rect = new Rect(0, 0, tex.width, tex.height);
        var pivot = new Vector2(0.5f, 0.5f);

        if (tex == null)
        {
            Plugin.BepinLogger.LogWarning($"tex is null");
        }
        
        Sprite sprite = Sprite.Create(tex, rect, pivot, pixelsPerUnit);
        sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
        if(sprite == null) Plugin.BepinLogger.LogWarning($"Sprite is null after creation");
        return sprite;
    }
}