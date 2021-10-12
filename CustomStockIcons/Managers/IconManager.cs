using BepInEx;
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

        static readonly string folder = Path.Combine(Paths.PluginPath, "Steven-CustomStockIcons", "Stock Icons");
        static readonly List<string> allowedFileTypes = new List<string> { ".png", ".jpg" };

        public static void Init()
        {
            Directory.CreateDirectory(folder);
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

            stopwatch.Stop();
            Plugin.LogInfo($"Loaded {iconDict.Keys.Count} icon{(iconDict.Keys.Count == 1 ? "" : "s")} in {stopwatch.ElapsedMilliseconds} ms.");

            foreach (var characterId in Constants.characterIds)
            {
                if (!iconDict.TryGetValue(characterId, out _))
                {
                    var resourcePath = $"CustomStockIcons.Resources.{characterId}.png";
                    var texture = LoadImageFromEmbeddedResource(resourcePath);
                    iconDict.Add(characterId, texture);
                }
            }
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
            Texture2D image = new Texture2D(4, 4);

            var assembly = Assembly.GetExecutingAssembly();
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
