using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace PlayerJobsPlugin;

[BepInPlugin("com.machaceleste.playerjobsplugin", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public static ConfigEntry<int> maxMissions;
        
    private void Awake()
    {
        maxMissions = Config.Bind("Main", "Max Missions", 6, new ConfigDescription("Sets the max number of missions able to be generated on a player site.", new AcceptableValueRange<int>(3, 20)));

        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        var harmony = new Harmony("com.machaceleste.playerjobsplugin");
        harmony.PatchAll();
    }
}