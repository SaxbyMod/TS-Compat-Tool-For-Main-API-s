using System;
using System.IO;
using System.Reflection;
using TSCompatToolForMainAPIs;

namespace TSCompatToolForMainAPIs.Patches
{
	public class TXTReplacerPatch
	{
		public static void RUNTOPATCH()
		{
			TextureReplacer.BepInExPlugin.path_tex = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.Replace("PatcherTool.dll", "") + "..\\" + Plugin.TextureReplacerMod.Value), "objects_textures/");
			TextureReplacer.BepInExPlugin.path_mes = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.Replace("PatcherTool.dll", "") + "..\\" + Plugin.TextureReplacerMod.Value), "objects_meshes/");
			TextureReplacer.BepInExPlugin.path_nam = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.Replace("PatcherTool.dll", "") + "..\\" + Plugin.TextureReplacerMod.Value), "objects_data/");
		}
		public static void AltRUNTOPATCH()
        {
            // Load the external assembly
            Assembly externalAssembly = Assembly.LoadFrom(Assembly.GetExecutingAssembly().Location.Replace("PatcherTool.dll", "") + "TextureReplacer.dll");
            // Get the type that contains the field
            Type targetType = externalAssembly.GetType("TextureReplacer.BepInExPlugin");
            // Get the field info of Texture Path
            FieldInfo texFieldInfo = targetType.GetField("path_tex", BindingFlags.Static | BindingFlags.Public);
            if (texFieldInfo != null)
            {
                texFieldInfo.SetValue(null, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.Replace("PatcherTool.dll", "") + "..\\" + Plugin.TextureReplacerMod.Value), "objects_textures/"));
                Console.WriteLine("Field Overwritten Successfully!");
            }
            else
            {
                Console.WriteLine("Field Not Found!");
            }
            // Get the field info of Mesh Path
            FieldInfo mesFieldInfo = targetType.GetField("path_mes", BindingFlags.Static | BindingFlags.Public);
            if (mesFieldInfo != null)
            {
                mesFieldInfo.SetValue(null, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.Replace("PatcherTool.dll", "") + "..\\" + Plugin.TextureReplacerMod.Value), "objects_meshes/"));
                Console.WriteLine("Field Overwritten Successfully!");
            }
            else
            {
                Console.WriteLine("Field Not Found!");
            }
            // Get the field info of Data Path
            FieldInfo namFieldInfo = targetType.GetField("path_nam", BindingFlags.Static | BindingFlags.Public);
            if (namFieldInfo != null)
            {
                namFieldInfo.SetValue(null, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.Replace("PatcherTool.dll", "") + "..\\" + Plugin.TextureReplacerMod.Value), "objects_data/"));
                Console.WriteLine("Field Overwritten Successfully!");
            }
            else
            {
                Console.WriteLine("Field Not Found!");
            }
        }
	}
}