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
        static void Postfix(ref int ___playerIndex, ref Image[] ___stocks)
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

                if (IconManager.TryGetTextureFromCharacterId(agent.GameUniqueIdentifier, out var texture))
                {
                    Plugin.LogDebug($"Replacing stock icons for {agent.GameUniqueIdentifier}");
                    for (int i = 0; i < ___stocks.Length; i++)
                        ___stocks[i].sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2f, texture.height / 2f));
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
    }
}
