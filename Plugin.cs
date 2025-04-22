using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Linq;
using System.Reflection;
using TSCompatToolForMainAPIs.Patches;

namespace TSCompatToolForMainAPIs
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        // Declare Harmony here for future Harmony patches. You'll use Harmony to patch the game's code outside of the scope of the API.
        public static Harmony harmony = new(PluginGuid);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        // These are variables that exist everywhere in the entire class.
        public const string PluginGuid = "creator.PatcherTool";
        public const string PluginName = "Patcher Tool for the Main APIs";
        public const string PluginVersion = "1.0.0";
        public const string PluginPrefix = "PatcherTool";

        // Configs:
        public static ConfigEntry<bool> VerboseLogging;
        public static ConfigEntry<string> TextureReplacerMod;

        public void Awake()
        {
	        Assembly assembly = Assembly.GetExecutingAssembly();
	        string DLLPath = Path.GetDirectoryName(assembly.Location);
	        string DLLName = DLLPath.Split('\\').Last();
            // Configs;
            VerboseLogging = Config.Bind<bool>("Content.Addition",
	            "Verbose Logging?", 
	            false,
	            "Should Verbose Logging be enabled?");
            
            TextureReplacerMod = Config.Bind<string>("API Settings",
	            "Texture Replacer Mod ID",
	            DLLName,
	            "What mod should be used for Texture Replacer? [Insert the Thunderstore Dependency String, without the version peice, for example; BepInEx-BepInExPack]");
            TXTReplacerPatch.RUNTOPATCH();
            TXTReplacerPatch.AltRUNTOPATCH();
        }
    }
}