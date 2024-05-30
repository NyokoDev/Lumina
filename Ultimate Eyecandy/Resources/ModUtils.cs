using ColossalFramework.Plugins;
using System.Reflection;
using Lumina;

namespace Lumina
{
    public static class ModUtils
    {
     
        
        public static bool IsAnyModsEnabled(string[] names)
        {
            foreach (string name in names)
            {
                if (IsModEnabled(name))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Helper method to check for enabled mods.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsModEnabled(string name)
        {
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    if (assembly.GetName().Name.ToLower() == name.ToLower())
                    {
                        return plugin.isEnabled;
                    }
                }
            }

            return false;
        }
    }
}
