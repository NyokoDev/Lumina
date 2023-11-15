using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lumina;


/*
 * The ShaderStructure class encapsulates the structure of shaders used in this project. The complete code for ShaderStructure has been sourced from the Dynamic Resolution project, available at:
 * https://github.com/d235j/Skylines-DynamicResolution/tree/cc7d04df204b74c1ba781dc3a5f492ba30ce6b61
 * This class plays a crucial role in defining the structure and behavior of shaders within our Lumina project. By incorporating the Dynamic Resolution code, we leverage proven implementations and practices, fostering code reuse and maintaining consistency with the Dynamic Resolution project's shader-related functionality. Any modifications or enhancements to the ShaderStructure class should be carefully synchronized with the corresponding code in the Dynamic Resolution repository to ensure compatibility and benefit from ongoing improvements made by the Dynamic Resolution project.
 */

namespace Lumina
{


    public static class ShaderStructure
    {
        public static bool ShaderSetting;
        public static bool IsEnabled;

        // Static property accessing instance property through an instance of Structure
        public static float ssaaFactor
        {
            get => StructureInstance.hook.userSSAAFactor;
            set => StructureInstance.hook.userSSAAFactor = value;
        }

        public static bool unlockSlider = false;
        public static bool ssaoState = true;
        public static bool lowerVRAMUsage = false;
        public static int sliderMaximumIndex = 1;
        public static float LockedSliderValue = 4f;

        // Instance of Structure to access its non-static members
        private static Structure _structureInstance;
        private static Structure StructureInstance
        {
            get
            {
                if (_structureInstance == null)
                {
                    _structureInstance = new Structure();
                    _structureInstance.hook = new CameraHook(); // Initialize hook
                }
                return _structureInstance;
            }
        }
    }

    public class Structure
    {
        public CameraHook hook;

    }
}

