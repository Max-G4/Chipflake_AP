using HarmonyLib;
using Il2CppSystem.Collections.Generic;

namespace Chipflake_AP.Patches;

public class AbilityGiver
{
    private static GameManager instance;
    private static Dictionary<string, string> LocToItem;
    
    
    public static void Awake()
    {
        FillDicts();
        Harmony.CreateAndPatchAll(typeof(GameManager));
    }
    
    public static void FillDicts()
    {
        LocToItem = new Dictionary<string, string>();
        LocToItem = new Dictionary<string, string>();
        LocToItem.Add("Grappling Hook", "grappling");
        LocToItem.Add("Speed Shoes", "speedshoes");
        LocToItem.Add("Diver Goggles", "underwaterGoggles");
        LocToItem.Add("Shovel", "shovel");
        LocToItem.Add("DiceMice Plush", "BoughtDiceMice");
    }
    
    [HarmonyPatch(typeof(QuestManager), "Awake")]
    [HarmonyPrefix]
    public static void GetInstance(GameManager __instance)
    {
        instance = __instance;
    }

    [HarmonyPatch(typeof(GameManager))]
    public static void GiveAbilityItem(string apItemName)
    {
        instance = GameManager.instance;
        if(instance == null) Plugin.BepinLogger.LogError("instance null");
        if(apItemName == null) Plugin.BepinLogger.LogError("apItemName null");
        if (apItemName.Equals("DiceMice Plush"))
        {
            instance.worldEvents[LocToItem[apItemName]] = 1;
        }
        instance._upgradeableAbilities[LocToItem[apItemName]] = 1;
        instance.player.RefreshAbilities();
        QuestManager.instance.ShowQuestMessage(Plugin.APSticker, apItemName, QuestManager.QuestMessageType.NewItem);
    }
    
}