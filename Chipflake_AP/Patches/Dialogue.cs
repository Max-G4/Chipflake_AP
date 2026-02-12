using HarmonyLib;
using NodeCanvas.DialogueTrees;

namespace Chipflake_AP.Patches;

public class DialoguePatcher
{
    public static void Awake()
    {
        Harmony.CreateAndPatchAll(typeof(DialoguePatcher));
    }

    [HarmonyPatch(typeof(PlayDirectorCamNode), "OnExecute")]
    [HarmonyPrefix]
    public static bool HinderDirectorCam(PlayDirectorCamNode __instance)
    {
        __instance.OnDirectorFinished(__instance.director);
        return false;
    }
}