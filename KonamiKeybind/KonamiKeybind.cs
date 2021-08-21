using System.Reflection;
using System.Collections.Generic;

namespace DuckGame.KonamiKeybind
{
    public class KonamiKeybind : DisabledMod
    {
        public static ModConfiguration Config;

        public static string ReplaceData
        {
            get
            {
                var result = !Config.isWorkshop ? "LOCAL" : SteamIdField.GetValue(Config, null).ToString();
                return result;
            }
        }

        public static bool Disabled
        {
            get => (bool)DisabledField.GetValue(Config, null);
            set => DisabledField.SetValue(Config, value, null);
        }

        private static readonly PropertyInfo SteamIdField =
            typeof(ModConfiguration).GetProperty("workshopID", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly PropertyInfo DisabledField =
            typeof(ModConfiguration).GetProperty("disabled", BindingFlags.Instance | BindingFlags.NonPublic);

        protected override void OnPreInitialize()
        {
            Config = configuration;

            // Thanks Drake! <3
            MDependencyResolver.ResolveDependencies();
        }

        protected override void OnPostInitialize()
        {
            // Patch
            HarmonyLoader.Loader.harmonyInstance.PatchAll();

            // Initialize classes
            KonamiCooldownManager.Initialize();

            Log("Loaded mod successfully!", LogType.Success);
        }

        public static void Log(string log, LogType logType = LogType.Message)
        {
            DevConsole.Log(DCSection.Mod, $"|DGRED|KONAMIKB {logColors[(int)logType]}{log}");
        }

        private void BecomeClientMod()
        {
            typeof(ModLoader).GetField("hiddendata", BindingFlags.GetField | BindingFlags.GetProperty);
            typeof(ModLoader).GetField("loadedmods", BindingFlags.Instance | BindingFlags.Public);
            typeof(ModLoader).GetField("fly you fools", BindingFlags.Instance | BindingFlags.Public);
            typeof(Mod).GetField("AllMods", BindingFlags.Instance | BindingFlags.Public);
            typeof(Mod).GetField("mod", BindingFlags.Instance | BindingFlags.Public);
        }

        public enum LogType
        {
            Message, Warning, Error, Success
        }

        private static readonly string[] logColors = new string[] { "|WHITE|", "|YELLOW|", "|RED|", "|GREEN|" };
    }
}
