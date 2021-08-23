using System.Reflection;
using System.Collections.Generic;
using System;

namespace DuckGame.KonamiKeybind
{
    public class KonamiKeybind : DisabledMod
    {
        public static ModConfiguration Config;

        protected override void OnPreInitialize()
        {
            Config = configuration;

            // Thanks Drake! <3
            MDependencyResolver.ResolveDependencies();
        }

        protected override void OnPostInitialize()
        {
            // Patch
            try
            {
                HarmonyLoader.Loader.harmonyInstance.PatchAll();
            }
            catch (Exception)
            {
                Log("There was an error while patching!", LogType.Error);
                Log("This will lead to malfunctions and/or crashes.", LogType.Error);
            }

            // Initialize classes
            KonamiCooldownManager.Initialize();

            Log("Loaded mod successfully!", LogType.Success);
        }

        public static void Log(string log, LogType logType = LogType.Message)
        {
            DevConsole.Log(DCSection.Mod, $"|DGRED|KONAMIKB {logColors[(int)logType]}{log}");
        }

        public enum LogType
        {
            Message, Warning, Error, Success
        }

        private static readonly string[] logColors = new string[] { "|WHITE|", "|YELLOW|", "|RED|", "|GREEN|" };
    }
}
