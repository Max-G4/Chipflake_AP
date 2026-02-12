using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Chipflake_AP.Patches;

public class StickerGiver
{
    private static QuestManager instance;
    
    
    public static void Awake()
    {
        Harmony.CreateAndPatchAll(typeof(StickerGiver));
    }

    [HarmonyPatch(typeof(QuestManager), "Awake")]
    [HarmonyPrefix]
    public static void GetInstance(QuestManager __instance)
    {
        instance = __instance;
    }

    
    public static void GiveStickerOut(String stickerName)
    {
        if (stickerName == null)
        {
            Plugin.BepinLogger.LogError("stickerName is null in GiveStickerOut");
        }
        stickerName = stickerName.Replace("Sticker - ", "");
        GiveSticker(stickerName);
    }
    
    private static string DumpString(string s)
    {
        if (s == null) return "<null>";
        var cps = string.Join(" ", s.Select(ch => $"U+{(int)ch:X4}"));
        return $"'{s}' (Len={s.Length}) [{cps}]";
    }
    
    [HarmonyPatch(typeof(QuestManager))]
    public static void GiveSticker(String questName)
    {
        if (questName == null)
        {
            Plugin.BepinLogger.LogError("questName is null in GiveSticker");
        }

        if (instance == null)
        {
            Plugin.BepinLogger.LogError("instance is null");
        }

        if (questName.Equals("Find The Stones"))
        {
            Plugin.BepinLogger.LogInfo("Trying to all OpenDoor");
            OpenDoor();
        }
        
        Quest quest = Quest.CreateInstance<Quest>();
        int qIndex = -1;
        Plugin.BepinLogger.LogInfo($"Giving sticker for quest {DumpString(questName)}");
        for(int i = 0; i < instance.allQuests.Length;i++)
        {
            if (instance.allQuests[i].name.Equals(questName))
            {
                quest = instance.allQuests[i];
                qIndex = i;
                if (instance.questStates[i].questDone)
                {
                    return;
                }
                break;
            }
        }
        
        instance.questStates[qIndex].questDone = true;
        instance.ShowQuestMessage(quest.sticker, "Sticker received!", QuestManager.QuestMessageType.CompletedQuest);

    }

    public static void OpenDoor()
    {
        Plugin.BepinLogger.LogInfo("Opening door");
        
        GameObject doorClosed = GameObject.Find("BigDoorClosed");
        GameObject doorOpen = doorClosed.transform.parent.FindChild("BigDoorOpen").gameObject;
        doorClosed.active = false;
        doorOpen.active = true;
    }
    
}