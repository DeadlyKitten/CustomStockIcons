using CustomStockIcons.Managers;
using CustomStockIcons.Utilities;
using HarmonyLib;
using Nick;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CustomStockIcons.Patches
{
    [HarmonyPatch(typeof(PlayerUISlot), "Activate")]
    class PlayerUISlot_Activate
    {
        static GameInstance gi;
        static bool isCoroutineRunning;
        static void Postfix(ref int ___playerIndex, ref Image ___charBackground, ref Image[] ___stocks, GameSetup.PlayerSetting ps)
        {
            if (___playerIndex == -1) return;

            if (!isCoroutineRunning)
            {
                gi = Resources.FindObjectsOfTypeAll<GameInstance>().FirstOrDefault(x => x.AgentCount > 0);
                SharedCoroutineStarter.StartCoroutine(NullGameInstance());
            }

            if (gi.GetAgentFromPlayerIndex(___playerIndex, out var agent))
            {
                if (!agent) return;

                agent.TryGetSkins(out var skins);
                var skinId = skins.lastSkinApplied;

                Plugin.LogInfo($"SkinID for {agent.GameUniqueIdentifier}: {skinId}");

                if (IconManager.TryGetTextureFromCharacterId($"{agent.GameUniqueIdentifier}_{skinId}", out var icon) ||
                    IconManager.TryGetTextureFromCharacterId(agent.GameUniqueIdentifier, out icon))
                {
                    Plugin.LogDebug($"Replacing stock icons for {agent.GameUniqueIdentifier}");

                    for (int i = 0; i < ___stocks.Length; i++)
                        ___stocks[i].sprite = Sprite.Create(icon,new Rect(0, 0, icon.width, icon.height), new Vector2(icon.width / 2f, icon.height / 2f));
                }

                if (Plugin.useSeriesEmblems.Value)
                {
                    if (BackgroundManager.TryGetTextureFromCharacterId(agent.GameUniqueIdentifier, out var background))
                    {
                        Plugin.LogDebug($"Replacing background for {agent.GameUniqueIdentifier}");

                        //Plugin.LogInfo("Sprite Name: " + ___charBackground.sprite.name);

                        //var spriteName = ___charBackground.sprite.name;
                        ___charBackground.sprite = Sprite.Create(background, new Rect(0, 0, background.width, background.height), new Vector2(background.width / 2f, background.height / 2f));
                        //___charBackground.color = teamColors[(int)char.GetNumericValue(spriteName, spriteName.Length - 1) - 1];
                    }
                }
            }
        }

        static IEnumerator NullGameInstance()
        {
            isCoroutineRunning = true;
            yield return new WaitForEndOfFrame();
            gi = null;
            isCoroutineRunning = false;
        }

        static Color[] teamColors = new Color[]
        {
            Color.white,
            Color.magenta,
            Color.yellow,
            Color.cyan
        };
    }
}
