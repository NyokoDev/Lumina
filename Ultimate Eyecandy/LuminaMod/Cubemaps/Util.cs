using System;
using System.IO;
using System.Linq;
using ColossalFramework.Plugins;
using ICities;
using UnityEngine;

namespace Lumina
{
    public class Util
    {
        public static string AssemblyDirectory
        {
            get
            {
                var pluginManager = PluginManager.instance;
                var plugins = pluginManager.GetPluginsInfo();

                foreach (var item in plugins)
                {
                    try
                    {
                        var instances = item.GetInstances<IUserMod>();
                        if (!(instances.FirstOrDefault() is LuminaMod))
                        {
                            continue;
                        }
                        return item.modPath;
                    }
                    catch
                    {

                    }
                }
                throw new Exception("Failed to find SkyboxReplacer assembly!");

            }
        }

        public static Texture2D LoadTextureFromFile(string path, bool readOnly = false)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            try
            {
                using (var textureStream = File.OpenRead(path))
                {
                    return LoadTextureFromStream(readOnly, textureStream);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
                return null;
            }
        }

        private static Texture2D LoadTextureFromStream(bool readOnly, Stream textureStream)
        {
            var buf = new byte[textureStream.Length]; //declare arraysize
            textureStream.Read(buf, 0, buf.Length); // read from stream to byte array
            textureStream.Close();
            var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            tex.LoadImage(buf);
            tex.name = Guid.NewGuid().ToString();
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 9;
            tex.Apply(false, readOnly);
            return tex;
        }

    }
}