using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Lumina.Shaders.AO
{
    internal class AssetManager
    {
        public AssetBundle AssetBundle { get; set; }

        private static AssetManager instance;

        public static AssetManager Instance
        {
            get
            {
                return instance ?? (instance = new AssetManager());
            }
        }
    }
}

