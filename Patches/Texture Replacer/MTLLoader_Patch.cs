// using Dummiesman;
// using HarmonyLib;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
//
// namespace TSCompatToolForMainAPIs
// {
// 	public class MTLLoader_Patch
// 	{
// 		public static List<string> SearchPaths = new List<string>()
// 		{
// 			"%FileName%_Textures",
// 			string.Empty
// 		};
// 		
// 		private static FileInfo _objFileInfo = (FileInfo)null;
// 		
// 		public static string pathstart = "plugins/" + TSCompatToolForMainAPIs.Plugin.TextureReplacerMod._typedValue;
// 		
// 		[HarmonyPatch(nameof(MTLLoader.TextureLoadFunction), typeof(Texture2D))]
// 		public static bool TextureLoadFunction(string path, bool isNormalMap)
// 		{
// 			foreach (string searchPath in SearchPaths)
// 			{
// 				string str = Path.Combine(_objFileInfo != null ? searchPath.Replace("%FileName%", Path.GetFileNameWithoutExtension(_objFileInfo.Name)) : searchPath, path);
// 				if (File.Exists(str))
// 				{
// 					Texture2D tex = ImageLoader.LoadTexture(str);
// 					if (Plugin.VerboseLogging.Value == true)
// 					{
// 						Plugin.Log.LogInfo($"Texture loading from {str}");
// 					}
// 					if (isNormalMap)
// 						tex = ImageUtils.ConvertToNormalMap(tex);
// 					return tex;
// 				}
// 			}
// 			return (Texture2D) null;
// 		}
// 		
// 		[HarmonyPatch(nameof(MTLLoader.TryLoadTexture), typeof(Texture2D))]
// 		private static bool TryLoadTexture(string texturePath, bool normalMap = false)
// 		{
// 			texturePath = pathstart + texturePath;
// 			texturePath = texturePath.Replace('\\', Path.DirectorySeparatorChar);
// 			texturePath = texturePath.Replace('/', Path.DirectorySeparatorChar);
// 			return TextureLoadFunction(texturePath, normalMap);
// 		}
// 	}
// }