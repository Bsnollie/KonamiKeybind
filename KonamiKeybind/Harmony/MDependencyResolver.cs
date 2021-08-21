using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DuckGame.KonamiKeybind
{
    /// <summary>
    /// This is a file from Drake's Client Mod Template.<br/>
    /// Find it here => https://github.com/Drakeyn/MClientTemplate <br/>
    /// Great Stuff!
    /// </summary>
    public static class MDependencyResolver
    {

        /// <summary>
        /// Registers this class as a handler for the Assembly Resolve Event.
        /// </summary>
        public static void ResolveDependencies()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        /// <summary>
        /// The event handler. Attempts to resolve a dependency when it's not found by the system.
        /// </summary>
        private static Assembly Resolve(object sender, ResolveEventArgs args)
        {

            string assemblyFullName = args.Name;
            string assemblyShortName = assemblyFullName;

            try
            {
                assemblyShortName = assemblyFullName.Substring(0, assemblyFullName.IndexOf(",", StringComparison.Ordinal));
            }
            catch (Exception)
            {
                KonamiKeybind.Log("Assembly resolve name was not in the expected format, using full name!", KonamiKeybind.LogType.Message);
            }

            /*
            Checks if the dependency is part of this mod. If it's not, we don't attempt to
            resolve it. This is to prevent multiple mods using the resolver from all attempting
            to resolve the same dependencies, and also to prevent any situations where a mod that
            does not have the dependency resolver seems to be functional, but only because another
            mod is resolving its dependencies for it, which would be a nightmare to debug on their
            end.
            */
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
            {
                KonamiKeybind.Log("Skipping external dependency: " + assemblyShortName);
                return null;
            }


            KonamiKeybind.Log("Attempting to resolve " + assemblyShortName);

            //Checks if the assembly is already loaded in the program, and just returns it if it is.
            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == assemblyFullName);

                if (!(assembly is null))
                {
                    //Hooray we found it!
                    KonamiKeybind.Log("Found assembly " + assemblyShortName + " already loaded!");
                    return assembly;
                }
            }
            catch (InvalidOperationException)
            {
                //Oh well we don't care.
            }

            //Time to look in the mod dlls folder!
            string path = Mod.GetPath<KonamiKeybind>("/dlls/" + assemblyShortName + ".dll");

            if (!File.Exists(path))
            {
                //We don't have it and we're probably about to crash D:
                KonamiKeybind.Log("Unable to resolve assembly " + assemblyShortName, KonamiKeybind.LogType.Error);
                return null;
            }

            Assembly loadedAssembly = null;

            //Down here we know the file exists, so let's try and load it!
            try
            {
                //Attempt #1 - LoadFrom()
                loadedAssembly = Assembly.LoadFrom(path);
            }
            catch (Exception)
            {
                //Attempt #1 didn't work. Try again!
                try
                {
                    //Attempt #2 - Load(bytes[])
                    loadedAssembly = Assembly.Load(File.ReadAllBytes(path));
                }
                catch (Exception)
                {
                    //Attempt #2 didn't work. Weird.
                    KonamiKeybind.Log("Failed to load assembly " + assemblyShortName, KonamiKeybind.LogType.Error);
                }
            }

            KonamiKeybind.Log("Loaded assembly " + assemblyShortName + " from disk!", KonamiKeybind.LogType.Success);

            return loadedAssembly;
        }
    }
}