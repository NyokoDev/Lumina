using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ColossalFramework.IO;
using ColossalFramework.PlatformServices;

namespace Lumina
{
    public class Lightstyle
    {
        public Lightstyle() { }

        public float[] m_values;
        public bool m_skyTonemapping, m_canBeDeleted;
        public string m_styleName, m_filePath;

        public void Savestyle(bool addTostylesList)
        {
            GlobalVariables input = new GlobalVariables();
            //string nameTextfield = input.nameTextfield;
            string path = DataLocation.localApplicationData + @"\Addons\Mods\" + m_styleName;
            if (!Directory.Exists(DataLocation.localApplicationData + @"\Lumina\"))
                Directory.CreateDirectory(DataLocation.localApplicationData + @"\Lumina\");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            this.m_canBeDeleted = true;
            this.m_filePath = $"{path}\\{ToFileName(this.m_styleName)}.light";

            using (TextWriter tw = new StreamWriter(this.m_filePath))
            {
                tw.WriteLine("name = " + ((this.m_styleName == "") ? "[none]" : this.m_styleName));
                tw.WriteLine("0 = " + this.m_values[0]);
                tw.WriteLine("1 = " + this.m_values[1]);
                tw.WriteLine("2 = " + this.m_values[2]);
                tw.WriteLine("3 = " + this.m_values[3]);
                tw.WriteLine("4 = " + this.m_values[4]);
                tw.WriteLine("5 = " + this.m_values[5]);
                tw.WriteLine("6 = " + this.m_values[6]);
                tw.WriteLine("7 = " + this.m_values[7]);
                tw.WriteLine("8 = " + this.m_values[8]);
                tw.WriteLine("9 = " + this.m_values[9]);
                tw.WriteLine("10 = " + this.m_values[10]);
                tw.WriteLine("11 = " + this.m_values[11]);
                tw.WriteLine("12 = " + this.m_values[12]);
                tw.WriteLine("skyTmpg = " + this.m_skyTonemapping);
            }

            if (addTostylesList)
                Lightstyles.Add(this);
        }



        public static void LoadLightstyles()
        {
            Lightstyles = new List<Lightstyle>();
            string path = DataLocation.localApplicationData + @"\Addons\Mods\";
            if (!Directory.Exists(DataLocation.localApplicationData + @"\Addons\Mods\")) ;
            Directory.CreateDirectory(DataLocation.localApplicationData + @"\Addons\Mods\");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (string file in Directory.GetFiles(path, "*.light", SearchOption.AllDirectories))
            {
                var style = new Lightstyle();
                style.m_styleName = "[none]";
                style.m_canBeDeleted = true;
                style.m_filePath = file;
                var dict = new Dictionary<int, float>();

                foreach (string line in File.ReadAllLines(file).Where(s => s.Contains(" = ")))
                {
                    string[] data = line.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);
                    if (data[0] == "skyTmpg")
                    {
                        style.m_skyTonemapping = bool.Parse(data[1]);
                    }
                    else if (data[0] == "name")
                    {
                        style.m_styleName = data[1];
                    }
                    else
                    {
                        var id = int.Parse(data[0]);
                        dict[id] = 0;
                        dict[id] = float.Parse(data[1]);
                    }
                }
                style.m_values = new float[]
                {
                dict[0], dict[1], dict[2], dict[3], dict[4], dict[5], dict[6], dict[7], dict[8], dict[9], dict[10], dict[11], dict[12]
                };
                Lightstyles.Add(style);
            }

            foreach (PublishedFileId fileId in PlatformService.workshop.GetSubscribedItems())
            {
                string fileDir = PlatformService.workshop.GetSubscribedItemPath(fileId);
                if (fileDir == null)
                    continue;
                if (fileDir == "")
                    continue;
                if (!Directory.Exists(fileDir))
                    continue;
                var files = Directory.GetFiles(fileDir, "*.light", SearchOption.AllDirectories);
                if (files.Any())
                {
                    foreach (string file in files)
                    {
                        var style = new Lightstyle();
                        style.m_styleName = "[none]";
                        style.m_canBeDeleted = false;
                        var dict = new Dictionary<int, float>();

                        foreach (string line in File.ReadAllLines(file).Where(s => s.Contains(" = ")))
                        {
                            string[] data = line.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);
                            if (data[0] == "skyTmpg")
                            {
                                style.m_skyTonemapping = bool.Parse(data[1]);
                            }
                            else if (data[0] == "name")
                            {
                                style.m_styleName = data[1];
                            }
                            else
                            {
                                var id = int.Parse(data[0]);
                                dict[id] = 0;
                                dict[id] = float.Parse(data[1]);
                            }
                        }
                        style.m_values = new float[]
                        {
                        dict[0], dict[1], dict[2], dict[3], dict[4], dict[5], dict[6], dict[7], dict[8], dict[9], dict[10], dict[11], dict[12]
                        };
                        Lightstyles.Add(style);
                    }
                }
            }

        }
        public static void Deletestyle(Lightstyle style)
        {
            if (style.m_canBeDeleted)
            {
                if (File.Exists(style.m_filePath))
                    File.Delete(style.m_filePath);
                if (Lightstyles.Contains(style))
                    Lightstyles.Remove(style);
            }
        }
        public static List<Lightstyle> Lightstyles;
        public static string ToFileName(string s)
        {
            return s.Replace(" ", "_").Replace(@"\", "").Replace("/", "").Replace("|", "").Replace("<", "").Replace(">", "").Replace("*", "").Replace(":", "").Replace("?", "").Replace("\"", "");
        }
    }
}
