using System.IO;
using BepInEx;
using BepInEx.Logging;
using Chipflake_AP.Archipelago;
using Chipflake_AP.Utils;
using BepInEx.Unity.IL2CPP;
using Chipflake_AP.Patches;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace Chipflake_AP;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class Plugin : BasePlugin
{
    public const string PluginGUID = "com.yourName.projectName";
    public const string PluginName = "Chipflake_AP";
    public const string PluginVersion = "0.0.1";

    public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
    private const string APDisplayInfo = $"Archipelago v{ArchipelagoClient.APVersion}";
    public static ManualLogSource BepinLogger;
    public static ArchipelagoClient ArchipelagoClient;
    public static Sprite APSticker;

    public override void Load()
    {
        // Plugin startup logic
        BepinLogger = Log;
        ArchipelagoClient = new ArchipelagoClient();
        ArchipelagoConsole.Awake();
        
        DialoguePatcher.Awake();
        

        
        // replace Sticker with a custom AP sticker
        var spritePath = Path.Combine(Paths.PluginPath, "Chipflake_AP", "Assets", "AP_Sticker.png");
        APSticker = SpriteLoader.LoadPngAsSprite(spritePath, pixelsPerUnit: 1);
        if (APSticker == null)
        {
            BepinLogger.LogError($"Sprite is null while passing to Plugin.APSticker");
        }
        
        QuestPatcher.Awake();
        QuestPatcher.Client = ArchipelagoClient;
        ItemShop.Awake();
        ItemShop.Client = ArchipelagoClient;
        ItemSpawnPatcher.Awake();
        ItemSpawnPatcher.Client = ArchipelagoClient;
        StickerGiver.Awake();
        AbilityGiver.Awake();
        
        // Register + spawn a MonoBehaviour so Unity can call OnGUI().
        ClassInjector.RegisterTypeInIl2Cpp<ArchipelagoUI>();
        var go = new GameObject("Chipflake_AP_GUI");
        Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.AddComponent<ArchipelagoUI>();
        go.GetComponent<ArchipelagoUI>().ArchipelagoClient = ArchipelagoClient;

        BepinLogger.LogMessage($"Archipelago Client Version: {ArchipelagoClient.APVersion}");
        ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
    }

    private void OnGUI()
    {
        // show the mod is currently loaded in the corner
        GUI.Label(new Rect(16, 16, 300, 20), ModDisplayInfo);
        ArchipelagoConsole.OnGUI();

        string statusMessage;
        // show the Archipelago Version and whether we're connected or not
        if (ArchipelagoClient.Authenticated)
        {
            // if your game doesn't usually show the cursor this line may be necessary
            //Cursor.visible = false;

            statusMessage = " Status: Connected";
            GUI.Label(new Rect(16, 50, 300, 20), APDisplayInfo + statusMessage);
        }
        else
        {
            // if your game doesn't usually show the cursor this line may be necessary
            // Cursor.visible = true;

            statusMessage = " Status: Disconnected";
            GUI.Label(new Rect(16, 50, 300, 20), APDisplayInfo + statusMessage);
            GUI.Label(new Rect(16, 70, 150, 20), "Host: ");
            GUI.Label(new Rect(16, 90, 150, 20), "Player Name: ");
            GUI.Label(new Rect(16, 110, 150, 20), "Password: ");

            ArchipelagoClient.ServerData.Uri = GUI.TextField(new Rect(150, 70, 150, 20),
                ArchipelagoClient.ServerData.Uri);
            ArchipelagoClient.ServerData.SlotName = GUI.TextField(new Rect(150, 90, 150, 20),
                ArchipelagoClient.ServerData.SlotName);
            ArchipelagoClient.ServerData.Password = GUI.TextField(new Rect(150, 110, 150, 20),
                ArchipelagoClient.ServerData.Password);

            // requires that the player at least puts *something* in the slot name
            if (GUI.Button(new Rect(16, 130, 100, 20), "Connect") &&
                !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
            {
                ArchipelagoClient.Connect();
            }
        }
        // this is a good place to create and add a bunch of debug buttons
    }
}