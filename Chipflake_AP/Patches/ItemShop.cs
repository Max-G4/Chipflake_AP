using System.Collections.Generic;
using Archipelago.MultiClient.Net.Helpers;
using Chipflake_AP.Archipelago;
using HarmonyLib;
using UnityEngine;

namespace Chipflake_AP.Patches;

public class ItemShop
{
    public static ArchipelagoClient Client;
    public static ILocationCheckHelper Lch;
    public static Dictionary<string, string> ItemToLoc;
    
    public static void Awake()
    {
        FillDicts();
        Harmony.CreateAndPatchAll(typeof(ItemShop));
        foreach (var method in Harmony.GetAllPatchedMethods())
        {
            Plugin.BepinLogger.LogInfo(method.ToString());
        }
    }

    public static void FillDicts()
    {
        ItemToLoc = new Dictionary<string, string>();
        ItemToLoc.Add("grappling", "Shop - Grappling Hook");
        ItemToLoc.Add("speedshoes", "Shop - Speed Shoes");
        ItemToLoc.Add("underwaterGoggles", "Shop - Diver Goggles");
        ItemToLoc.Add("shovel", "Shop - Shovel");
        ItemToLoc.Add("BoughtDiceMice", "Shop - DiceMice Plush");
    }

    [HarmonyPatch(typeof(ShopSystem), "Start")]
    [HarmonyPostfix]
    public static void GetInstance(ShopSystem __instance)
    {
        Plugin.BepinLogger.LogInfo("ShopSystem.Start called");
    }

    [HarmonyPatch(typeof(ShopSystem), "BuyCurrentlySelected")]
    [HarmonyPrefix]
    public static void IsMethodHit()
    {
        Plugin.BepinLogger.LogInfo("ShopSystem.BuyCurrentlySelected called");
    }
    
    [HarmonyPatch(typeof(ShopSystem), "BuyItem")]
    [HarmonyPrefix]
    public static bool InterceptBuyItem(ShopSystem.BuyableItem item, ShopSystem __instance)
    {
        if (item.soldOut)
        {
            Plugin.BepinLogger.LogInfo("Item is sold out");
            return false;
        }
        
        if (item.abilityKey != null) {
            Plugin.BepinLogger.LogInfo("Trying to buy item: " + item.abilityKey);
            
            if (GameManager.Instance.currentMoney >= item.price)
            {
                GameManager.Instance.moneySpend += item.price;
                Plugin.BepinLogger.LogInfo("sending Location: " + ItemToLoc[item.abilityKey]);
                Client.SendLocation(ItemToLoc[item.abilityKey]);
                __instance.RefreshShop();
                __instance.StartCoroutine(__instance.ShowShopkeeperText("Shop_Bought"));
                GameManager.Instance.UpdateCollectibleUI(true);
                GameManager.Instance.player.RefreshAbilities();
                __instance.myNPC.myAnimator.SetTrigger("Jump");
                return false;
            }
        }
        else if (item.worldKey != null)
        {
            if (GameManager.Instance.currentMoney >= item.price)
            {
                GameManager.Instance.moneySpend += item.price;
                Plugin.BepinLogger.LogInfo("sending Location: " + ItemToLoc[item.worldKey]);
                Client.SendLocation(ItemToLoc[item.worldKey]);
                __instance.RefreshShop();
                __instance.StartCoroutine(__instance.ShowShopkeeperText("Shop_Bought"));
                GameManager.Instance.UpdateCollectibleUI(true);
                GameManager.Instance.player.RefreshAbilities();
                __instance.myNPC.myAnimator.SetTrigger("Jump");
                return false;
            }
        }
        Plugin.BepinLogger.LogInfo("Not enough money");
        __instance.StartCoroutine(__instance.ShowShopkeeperText("Shop_NoCash"));
        __instance.myNPC.myAnimator.SetBool("Talking", true);
        return false;
    }
    
    [HarmonyPatch(typeof(ShopSystem), "RefreshShop")]
    [HarmonyPrefix]
    public static void RefreshShop(ShopSystem __instance)
    {
        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<Transform, ShopSystem.BuyableItem> keyValuePair in __instance.buyableItems)
        {
            if ((keyValuePair.value.abilityKey != null && Lch.AllLocationsChecked.Contains(Lch.GetLocationIdFromName("Super Chipflake Ü", ItemToLoc[keyValuePair.Value.abilityKey])))
                || (keyValuePair.Value.worldKey != null && Lch.AllLocationsChecked.Contains(Lch.GetLocationIdFromName("Super Chipflake Ü", ItemToLoc[keyValuePair.Value.worldKey]))))
            {
                keyValuePair.Value.graphic.gameObject.SetActive(false);
                if (!keyValuePair.Value.soldOut)
                {
                    keyValuePair.Value.graphic.gameObject.SetActive(false);
                    keyValuePair.Value.graphic = UnityEngine.Object.Instantiate<Transform>(__instance.soldoutSignPrefab, keyValuePair.Key);
                    keyValuePair.Value.startPos = keyValuePair.Value.graphic.transform.position;
                }
                keyValuePair.Value.priceText.SetText("x", true);
                keyValuePair.Value.soldOut = true;
            }
            else
            {
                keyValuePair.Value.priceText.SetText(keyValuePair.Value.price.ToString(), true);
            }
        }
    }
    
    
    [HarmonyPatch(typeof(ShopSystem), "Update")]
    [HarmonyPrefix]
    private static bool replaceUpdate(ShopSystem __instance)
    { 
        if (!__instance.shopOpen)
            return false;
        if (!__instance.directionPressed && (double) Time.time > (double) __instance.waitTime)
        {
            if ((double) GameManager.Instance.UIMovement.ReadValue<Vector2>().x > 0.30000001192092896 && (UnityEngine.Object) __instance.currentlySelected.rightItem != (UnityEngine.Object) null)
            {
                __instance.directionPressed = true;
                __instance.currentlySelected.DeselectMe(__instance);
                __instance.currentlySelected = __instance.buyableItems[__instance.currentlySelected.rightItem];
                __instance.currentlySelected.SelectMe(__instance);
            }
            if ((double) GameManager.Instance.UIMovement.ReadValue<Vector2>().x < -0.30000001192092896 && (UnityEngine.Object) __instance.currentlySelected.leftItem != (UnityEngine.Object) null)
            {
                __instance.directionPressed = true;
                __instance.currentlySelected.DeselectMe(__instance);
                __instance.currentlySelected = __instance.buyableItems[__instance.currentlySelected.leftItem];
                __instance.currentlySelected.SelectMe(__instance);
            }
            if ((double) GameManager.Instance.UIMovement.ReadValue<Vector2>().y < -0.30000001192092896 && (UnityEngine.Object) __instance.currentlySelected.downItem != (UnityEngine.Object) null)
            {
                __instance.directionPressed = true;
                __instance.currentlySelected.DeselectMe(__instance);
                __instance.currentlySelected = __instance.buyableItems[__instance.currentlySelected.downItem];
                __instance.currentlySelected.SelectMe(__instance);
            }
            if ((double) GameManager.Instance.UIMovement.ReadValue<Vector2>().y > 0.30000001192092896 && (UnityEngine.Object) __instance.currentlySelected.upItem != (UnityEngine.Object) null)
            {
                __instance.directionPressed = true;
                __instance.currentlySelected.DeselectMe(__instance);
                __instance.currentlySelected = __instance.buyableItems[__instance.currentlySelected.upItem];
                __instance.currentlySelected.SelectMe(__instance);
            }
        }
        else if ((double) GameManager.Instance.UIMovement.ReadValue<Vector2>().magnitude < 0.30000001192092896)
            __instance.directionPressed = false;
        if (GameManager.Instance.abortButton.WasPressedThisFrame())
        {
            __instance.CloseShop();
            GameManager.Instance.CloseWindow("SchnitzelvilleShop");
            __instance.gameObject.SetActive(false);
        }
        if (!GameManager.Instance.submitButton.WasPressedThisFrame())
            return false;
        __instance.BuyItem(__instance.currentlySelected);
        return false;
    }
    
    
    
    
    

    [HarmonyPatch(typeof(ShopSystem), "OpenShop")]
    [HarmonyPrefix]
    public static void RefreshOnOpen(ShopSystem __instance)
    {
        __instance.RefreshShop();
    }

    
    // Testing logging and cheats
    /*
    [HarmonyPatch(typeof(GameManager), "Awake")]
    [HarmonyPostfix]
    public static void GetInstance(GameManager __instance)
    {
        
        Plugin.BepinLogger.LogInfo("upgradableAbilities");
        foreach (var item in __instance.upgradeableAbilities)
        {
            Plugin.BepinLogger.LogInfo("item: " + item.Key + " - " + item.Value);
        }
        Plugin.BepinLogger.LogInfo("_upgradableAbilities");
        foreach (var item in __instance._upgradeableAbilities)
        {
            Plugin.BepinLogger.LogInfo("item: " + item.Key + " - " + item.Value);
        }
        
        Plugin.BepinLogger.LogInfo("WorldEvents");
        foreach (var worldEvent in __instance.worldEvents)
        {
            Plugin.BepinLogger.LogInfo("item: " + worldEvent.Key + " - " + worldEvent.Value);

        }
        
        
        
        for(int i = 0; i < __instance.unlockedItems.Length; i++)
        {
            __instance.unlockedItems[i] = true;
        }
        

        __instance._upgradeableAbilities["doubleJump"] = 50;
        
        __instance.moneySpend = 0;
        
    }
    */
    
}