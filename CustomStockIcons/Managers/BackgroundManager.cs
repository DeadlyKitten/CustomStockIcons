using BepInEx;
using CustomStockIcons.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CustomStockIcons.Managers
{
    static class BackgroundManager
    {
        static readonly Dictionary<string, Texture2D> backgroundDict = new Dictionary<string, Texture2D>();

        static readonly string folder = Path.Combine(Paths.BepInExRootPath, "Custom Icons", "Backgrounds");
        static readonly List<string> allowedFileTypes = new List<string> { ".png", ".jpg" };

        public static void Init()
        {
            Directory.CreateDirectory(folder);
            LoadAllBackgrounds();
        }

        public static void Refresh()
        {
            backgroundDict.Clear();
            LoadAllBackgrounds();
        }

        static void LoadAllBackgrounds()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            foreach (var file in Directory.GetFiles(folder).Where(x => allowedFileTypes.Contains(Path.GetExtension(x).ToLower())))
            {
                var texture = LoadImageFromFile(file);
                var id = TranslateCharacterNameToId(Path.GetFileNameWithoutExtension(file));
                if (backgroundDict.TryGetValue(id.ToLower(), out _))
                {
                    var character = Constants.nameToId.FirstOrDefault(x => x.Value == id.ToLower()).Key;
                    Plugin.LogWarning($"Multiple backgrounds found for {character ?? id.ToLower()}! Ignoring {Path.GetFileName(file)}");
                    continue;
                }
                backgroundDict.Add(id.ToLower(), texture);
            }

            if (!Plugin.useVanillaIcons.Value)
            {
                foreach (var characterId in Constants.characterIds)
                {
                    if (!backgroundDict.TryGetValue(characterId, out _))
                    {
                        var resourcePath = $"CustomStockIcons.Resources.Backgrounds.{characterId}.png";
                        var texture = LoadImageFromEmbeddedResource(resourcePath);
                        if (texture)
                            backgroundDict.Add(characterId, texture);
                        else
                            Plugin.LogWarning($"Background not found for {characterId}");
                    }
                }
            }

            stopwatch.Stop();
            Plugin.LogInfo($"Loaded {backgroundDict.Keys.Count} background{(backgroundDict.Keys.Count == 1 ? "" : "s")} in {stopwatch.ElapsedMilliseconds} ms.");
        }

        internal static bool TryGetTextureFromCharacterId(string id, out Texture2D texture) => backgroundDict.TryGetValue(id, out texture);

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
