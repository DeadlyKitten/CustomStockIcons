﻿using BepInEx;
using CustomStockIcons.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CustomStockIcons.Managers
{
    static class IconManager
    {
        static readonly Dictionary<string, Texture2D> iconDict = new Dictionary<string, Texture2D>();

        static readonly string folder = Path.Combine(Paths.BepInExRootPath, "Custom Icons", "Stock Icons");
        static readonly List<string> allowedFileTypes = new List<string> { ".png", ".jpg" };

        public static void Init()
        {
            Directory.CreateDirectory(folder);
            LoadAllIcons();
        }

        public static void Refresh()
        {
            iconDict.Clear();
            LoadAllIcons();
        }

        static void LoadAllIcons()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            foreach (var file in Directory.GetFiles(folder).Where(x => allowedFileTypes.Contains(Path.GetExtension(x).ToLower())))
            {
                var texture = LoadImageFromFile(file);
                var id = TranslateCharacterNameToId(Path.GetFileNameWithoutExtension(file));
                if (iconDict.TryGetValue(id.ToLower(), out _))
                {
                    var character = Constants.nameToId.FirstOrDefault(x => x.Value == id.ToLower()).Key;
                    Plugin.LogWarning($"Multiple stock icons found for {character ?? id.ToLower()}! Ignoring {Path.GetFileName(file)}");
                    continue;
                }
                iconDict.Add(id.ToLower(), texture);
            }

            if (!Plugin.useVanillaIcons.Value)
            {
                foreach (var characterId in Constants.characterIds.Concat(Constants.skinIds))
                {
                    if (!iconDict.TryGetValue(characterId, out _))
                    {
                        var resourcePath = $"CustomStockIcons.Resources.Stock_Icons.{characterId}.png";
                        var texture = LoadImageFromEmbeddedResource(resourcePath);
                        if (texture)
                            iconDict.Add(characterId, texture);
                        else
                            Plugin.LogWarning($"Icon not found for {characterId}");
                    }
                }
            }

            stopwatch.Stop();
            Plugin.LogInfo($"Loaded {iconDict.Keys.Count} icon{(iconDict.Keys.Count == 1 ? "" : "s")} in {stopwatch.ElapsedMilliseconds} ms.");
        }

        internal static bool TryGetTextureFromCharacterId(string id, out Texture2D texture) => iconDict.TryGetValue(id, out texture);

        internal static string TranslateCharacterNameToId(string name)
        {
            if (Constants.nameToId.TryGetValue(name.ToLower(), out var id))
                return id;
            return name;
        }

        static Texture2D LoadImageFromFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var image = new Texture2D(2, 2);
            image.LoadImage(bytes);
            image.wrapMode = TextureWrapMode.Clamp;

            return image;
        }

        static Texture2D LoadImageFromEmbeddedResource(string path)
        {
            Texture2D image = new Texture2D(2, 2);

            var assembly = Assembly.GetExecutingAssembly();

            if (!assembly.GetManifestResourceNames().Contains(path)) return null;

            using (var stream = assembly.GetManifestResourceStream(path))
            {
                byte[] imageData = new byte[stream.Length];
                stream.Read(imageData, 0, (int)stream.Length);
                image.LoadImage(imageData);
                image.wrapMode = TextureWrapMode.Clamp;
            }

            return image;
        }
    }
}
