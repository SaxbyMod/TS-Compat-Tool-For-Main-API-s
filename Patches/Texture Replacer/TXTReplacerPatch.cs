using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Dummiesman;
using HarmonyLib;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TSCompatToolForMainAPIs.Patches.Texture_Replacer
{
	[HarmonyPatch]
	public class TXTReplacerPatch
	{
		static Assembly assembly = Assembly.GetExecutingAssembly();
		static string DLLPath = Path.GetDirectoryName(assembly.Location);
		private static string path_tex = Path.Combine(DLLPath + "..\\..\\" + Plugin.TextureReplacerMod.Value, "objects_textures/");
		private static string path_mes = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "objects_meshes/");
		private static string path_nam = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "objects_data/");
		private static MeshFilter[] mesh_list = new MeshFilter[0];
		private static Material[] mat_list = new Material[0];
		private static UnityEngine.UI.Image[] image_list = new UnityEngine.UI.Image[0];
		private static bool init_inTitle = false;
		private static bool init_inGame = false;
		private static bool init_filePaths = false;
		private static bool shouldReload = false;
		public static Dictionary<string, string> filePaths_tex = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
		public static Dictionary<string, string> filePaths_obj = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
		public static Dictionary<string, string> filePaths_ttf = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
		public static Dictionary<string, string> filePaths_nam = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
		private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();
		private static Dictionary<string, Mesh> cachedMeshes = new Dictionary<string, Mesh>();
		private static Dictionary<string, Cubemap> cachedCubemap = new Dictionary<string, Cubemap>();
		private static Dictionary<string, TMP_FontAsset> cachedFonts = new Dictionary<string, TMP_FontAsset>();
		private static Dictionary<string, string> cachedData = new Dictionary<string, string>();
		private static Texture2D CardFoilMaskC;
		private static UnityEngine.Color card_color;
		private static UnityEngine.Color card_color_outline;
		private static float card_outline = -1f;
		private static TMP_FontAsset billboardTextFont;
		public static GameObject tempmesh = new GameObject((string)null);
		private static Cubemap cubemap_day;
		private static Cubemap cubemap_night;
		private static Cubemap cubemap_sun;
		private static CubemapFace c1 = CubemapFace.NegativeZ;
		private static CubemapFace c2 = CubemapFace.PositiveX;
		private static CubemapFace c3 = CubemapFace.NegativeY;
		private static CubemapFace c4 = CubemapFace.PositiveY;
		private static CubemapFace c5 = CubemapFace.NegativeX;
		private static CubemapFace c6 = CubemapFace.PositiveZ;
		private static string bagname = "";
		private static string playtablename = "";
		private static string cardname = "";
		private static readonly string[] renderer_names = new string[54]
		{
			"SmallMetalShelf",
			"SmallPersonalShelf",
			"PersonalShelfGlass",
			"HugePersonalShelf",
			"TallCardDisplayCase",
			"SleekCardDisplayCase",
			"Weapons closet",
			"AdapterMesh",
			"CardDisplayTable",
			"ShelvingModel",
			"CounterModel",
			"Monitor",
			"DrawerModel",
			"LongShelf",
			"Model",
			"Workbench",
			"Table 1_Half",
			"Table 1",
			"Chair 2 (12)",
			"Chair 2 (13)",
			"Chair 2 (11)",
			"Chair 2 (10)",
			"Chair 2 (14)",
			"Chair 2 (15)",
			"door B",
			"windows door",
			"GlassDoor",
			"House01_BtmCut",
			"House_01_BtmCut1",
			"Door",
			"crank",
			"door frame",
			"Wall 2",
			"Wall 1 (4)",
			"Wall 1 (5)",
			"WallMesh_Side",
			"WallMesh_Front",
			"Ceiling (1)",
			"Ceiling (3)",
			"Floor (1)",
			"Floor (3)",
			"House01_Cut",
			"House19_Cut",
			"House19_BtmCut",
			"WallMesh (2)",
			"Wall_SideB_LotA_1",
			"Wall_Back_LotA_1",
			"Wall_Back_LotA_2",
			"Wall_Back_LotB",
			"Wall 1 (1)",
			"Wall_Side_LotB_1",
			"Wall_Side_LotA_1",
			"Wall_Side_LotA_2",
			"Wall_Side_LotB_2"
		};
		
		[HarmonyPatch("checkFolders", typeof(TextureReplacer.BepInExPlugin)), HarmonyPrefix]
		private static bool checkFolders()
		{
			Console.WriteLine($"{path_tex}");
			Console.WriteLine($"{path_mes}");
			Console.WriteLine($"{path_nam}");
			if (!Directory.Exists(path_tex))
				Directory.CreateDirectory(path_tex);
			if (!Directory.Exists(path_mes))
				Directory.CreateDirectory(path_mes);
			if (!Directory.Exists(path_nam))
				Directory.CreateDirectory(path_nam);
			if (!Directory.Exists(path_nam + "accessories"))
				Directory.CreateDirectory(path_nam + "accessories");
			if (!Directory.Exists(path_nam + "boosterpacks"))
				Directory.CreateDirectory(path_nam + "boosterpacks");
			if (!Directory.Exists(path_nam + "cards"))
				Directory.CreateDirectory(path_nam + "cards");
			if (!Directory.Exists(path_nam + "figurines"))
				Directory.CreateDirectory(path_nam + "figurines");
			return false;
		}
		
		
		[HarmonyPatch("Update", typeof(TextureReplacer.BepInExPlugin)), HarmonyPrefix]
		private static bool Update()
		{
			Scene activeScene;
			if (SceneManager.GetActiveScene().name == "Title" && !TextureReplacer.BepInExPlugin.init_filePaths || TextureReplacer.BepInExPlugin.shouldReload)
			{
				TextureReplacer.BepInExPlugin.init_filePaths = true;
				checkFolders();
				try
				{
					string[] strArray = new string[2]
					{
						"*.png",
						"*.txt"
					};
					foreach (string searchPattern in strArray)
					{
						foreach (string file in Directory.GetFiles(path_tex, searchPattern, SearchOption.AllDirectories))
						{
							string fileName = Path.GetFileName(file);
							string directoryName = Path.GetDirectoryName(file);
							if (!TextureReplacer.BepInExPlugin.filePaths_tex.ContainsKey(fileName))
								TextureReplacer.BepInExPlugin.filePaths_tex.Add(fileName, directoryName + "/");
						}
					}
				}
				catch
				{
				}
				try
				{
					string[] strArray = new string[1] { "*.obj" };
					foreach (string searchPattern in strArray)
					{
						foreach (string file in Directory.GetFiles(path_mes, searchPattern, SearchOption.AllDirectories))
						{
							string fileName = Path.GetFileName(file);
							string directoryName = Path.GetDirectoryName(file);
							if (!TextureReplacer.BepInExPlugin.filePaths_obj.ContainsKey(fileName))
								TextureReplacer.BepInExPlugin.filePaths_obj.Add(fileName, directoryName + "/");
						}
					}
				}
				catch
				{
				}
				try
				{
					string[] strArray = new string[2]
					{
						"*.ttf",
						"*.otf"
					};
					foreach (string searchPattern in strArray)
					{
						foreach (string file in Directory.GetFiles(path_tex, searchPattern, SearchOption.AllDirectories))
						{
							string fileName = Path.GetFileName(file);
							string directoryName = Path.GetDirectoryName(file);
							if (!TextureReplacer.BepInExPlugin.filePaths_ttf.ContainsKey(fileName))
								TextureReplacer.BepInExPlugin.filePaths_ttf.Add(fileName, directoryName + "/");
						}
					}
				}
				catch
				{
				}
				try
				{
					string[] strArray = new string[1] { "*.txt" };
					foreach (string searchPattern in strArray)
					{
						foreach (string file in Directory.GetFiles(path_nam, searchPattern, SearchOption.AllDirectories))
						{
							string fileName = Path.GetFileName(file);
							string directoryName = Path.GetDirectoryName(file);
							if (!TextureReplacer.BepInExPlugin.filePaths_nam.ContainsKey(fileName))
								TextureReplacer.BepInExPlugin.filePaths_nam.Add(fileName, directoryName + "/");
						}
					}
				}
				catch
				{
				}
				CacheMeshesAtStart();
				CacheTexturesAtStart();
				CacheCubemapsAtStart();
				CacheFontsAtStart();
				CacheCardDataAtStart();
				if ((UnityEngine.Object)TextureReplacer.BepInExPlugin.CardFoilMaskC == (UnityEngine.Object)null)
				{
					Assembly assembly = Assembly.GetExecutingAssembly();
					string DLLPath = Path.GetDirectoryName(assembly.Location);
					Texture2D tex = ImageLoader.LoadTexture(DLLPath + "CardFoilMaskC.png");
					tex.name = "CardFoilMask";
					tex.name = "CardFoilMask";
					TextureReplacer.BepInExPlugin.CardFoilMaskC = tex;
				}
				if (TextureReplacer.BepInExPlugin.shouldReload)
				{
					TextureReplacer.BepInExPlugin.shouldReload = false;
					DoReplace();
					UnityEngine.Resources.UnloadUnusedAssets();
					activeScene = SceneManager.GetActiveScene();
					if (activeScene.name == "Start")
					{
						CSingleton<SkyboxBlender>.Instance.UpdateLightingAndReflections(true);
						DynamicGI.UpdateEnvironment();
						CSingleton<LightManager>.Instance.ToggleShopLight();
						CSingleton<LightManager>.Instance.ToggleShopLight();
					}
				}
			}
			activeScene = SceneManager.GetActiveScene();
			if (activeScene.name == "Title" && !TextureReplacer.BepInExPlugin.init_inTitle)
			{
				TextureReplacer.BepInExPlugin.init_inTitle = true;
				bagname = "";
				playtablename = "";
				TextureReplacer.BepInExPlugin.init_inGame = false;
				DoReplace();
			}
			activeScene = SceneManager.GetActiveScene();
			if (activeScene.name == "Start" && !TextureReplacer.BepInExPlugin.init_inGame)
			{
				TextureReplacer.BepInExPlugin.init_inGame = true;
				TextureReplacer.BepInExPlugin.init_inTitle = false;
				DoReplace();
			}
			TextureReplacer.BepInExPlugin.shouldReload = true;
			return false;
		}

		private static void CacheTexturesAtStart()
		{
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_tex)
			{
				string key = keyValuePair.Key.Replace(".png", "");
				Texture2D texture2D = !key.ToLower().EndsWith("_n") && !key.ToLower().EndsWith("_normal") && !key.ToLower().EndsWith(" n") ? TextureReplacer.BepInExPlugin.LoadPNG(keyValuePair.Value + key + ".png") : TextureReplacer.BepInExPlugin.LoadPNG_Bump(keyValuePair.Value + key + ".png");
				if ((UnityEngine.Object)texture2D != (UnityEngine.Object)null)
				{
					texture2D.Apply(true, true);
					TextureReplacer.BepInExPlugin.cachedTextures[key] = texture2D;
				}
			}
			Console.WriteLine((object)"Textures cached at start");
		}

		private static Texture2D GetCachedTexture(string name)
		{
			Texture2D texture2D;
			return TextureReplacer.BepInExPlugin.cachedTextures.TryGetValue(name, out texture2D) ? texture2D : (Texture2D)null;
		}

		private static void CacheMeshesAtStart()
		{
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_obj)
			{
				string key = keyValuePair.Key.Replace(".obj", "");
				TextureReplacer.BepInExPlugin.tempmesh = new OBJLoader().Load(keyValuePair.Value + key + ".obj");
				if ((UnityEngine.Object)TextureReplacer.BepInExPlugin.tempmesh != (UnityEngine.Object)null)
				{
					try
					{
						TextureReplacer.BepInExPlugin.tempmesh.name = key;
						List<Mesh> meshList = new List<Mesh>();
						foreach (Component component in TextureReplacer.BepInExPlugin.tempmesh.transform)
						{
							Mesh mesh = component.gameObject.GetComponent<MeshFilter>().mesh;
							Vector3[] vertices = mesh.vertices;
							for (int index = 0; index < vertices.Length; ++index)
								vertices[index].x = -vertices[index].x;
							mesh.vertices = vertices;
							int[] triangles = mesh.triangles;
							for (int index = 0; index < triangles.Length; index += 3)
							{
								int num = triangles[index];
								triangles[index] = triangles[index + 2];
								triangles[index + 2] = num;
							}
							mesh.triangles = triangles;
							mesh.RecalculateNormals();
							mesh.RecalculateBounds();
							meshList.Add(mesh);
						}
						CombineInstance[] combine = new CombineInstance[meshList.Count];
						for (int index = 0; index < meshList.Count; ++index)
						{
							combine[index].mesh = meshList[index];
							combine[index].transform = Matrix4x4.identity;
						}
						Mesh mesh1 = new Mesh();
						mesh1.CombineMeshes(combine, false);
						mesh1.name = key;
						TextureReplacer.BepInExPlugin.cachedMeshes[key] = mesh1;
						TextureReplacer.BepInExPlugin.cachedMeshes[key].UploadMeshData(true);
					}
					catch
					{
					}
				}
				UnityEngine.Object.Destroy((UnityEngine.Object)TextureReplacer.BepInExPlugin.tempmesh);
			}
			Console.WriteLine((object)"Meshes cached at start");
		}

		private static Mesh GetCachedMesh(string name)
		{
			Mesh mesh;
			return TextureReplacer.BepInExPlugin.cachedMeshes.TryGetValue(name, out mesh) ? mesh : (Mesh)null;
		}

		private static void ApplyFace(
			Cubemap cubemap,
			int faceIndex,
			Texture2D faceTexture,
			CubemapFace[] faceMapping)
		{
			cubemap.SetPixels(faceTexture.GetPixels(), faceMapping[faceIndex]);
		}

		private static void CacheCubemapsAtStart()
		{
			Dictionary<string, Cubemap> dictionary = new Dictionary<string, Cubemap>()
			{
				{
					"FS003_Day",
					(Cubemap)null
				},
				{
					"FS003_Night",
					(Cubemap)null
				},
				{
					"FS003_Sun",
					(Cubemap)null
				}
			};
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_tex)
			{
				string key1 = keyValuePair.Key.Replace(".png", "");
				foreach (string key2 in dictionary.Keys)
				{
					if (key1.StartsWith(key2))
					{
						Texture2D texture2D = TextureReplacer.BepInExPlugin.LoadPNGHDR(keyValuePair.Value + key1 + ".png");
						if (texture2D.width == 1024 && texture2D.height == 6144)
						{
							Cubemap cubemap = new Cubemap(1024, UnityEngine.TextureFormat.RGBA32, true);
							for (int faceIndex = 0; faceIndex < 6; ++faceIndex)
							{
								Texture2D faceTexture = new Texture2D(1024, 1024, UnityEngine.TextureFormat.RGBA32, false);
								faceTexture.SetPixels(texture2D.GetPixels(0, faceIndex * 1024, 1024, 1024));
								faceTexture.Apply();
								ApplyFace(cubemap,
									faceIndex,
									faceTexture,
									new CubemapFace[6]
									{
										TextureReplacer.BepInExPlugin.c1,
										TextureReplacer.BepInExPlugin.c2,
										TextureReplacer.BepInExPlugin.c3,
										TextureReplacer.BepInExPlugin.c4,
										TextureReplacer.BepInExPlugin.c5,
										TextureReplacer.BepInExPlugin.c6
									});
							}
							cubemap.Apply();
							UpdateEnvironment(cubemap);
							switch (key2)
							{
								case "FS003_Day":
									cubemap_day = cubemap;
									TextureReplacer.BepInExPlugin.cachedCubemap[key1] = cubemap_day;
									goto label_16;
								case "FS003_Night":
									cubemap_night = cubemap;
									TextureReplacer.BepInExPlugin.cachedCubemap[key1] = cubemap_night;
									goto label_16;
								case "FS003_Sun":
									cubemap_sun = cubemap;
									TextureReplacer.BepInExPlugin.cachedCubemap[key1] = cubemap_sun;
									goto label_16;
								default:
									goto label_16;
							}
						}
						else
						{
							Console.WriteLine((object)string.Format("Skipping {0}: Texture dimensions are {1}x{2}, not suitable for cubemap (expected 1024x6144).", (object)key1, (object)texture2D.width, (object)texture2D.height));
							break;
						}
					}
				}
				label_16: ;
			}
			Console.WriteLine((object)"Cubemaps cached at start");
		}

		private static void UpdateEnvironment(Cubemap cubemap)
		{
			CSingleton<SkyboxBlender>.Instance.UpdateLightingAndReflections(true);
		}

		private static Cubemap GetCachedCubemap(string name)
		{
			Cubemap cubemap;
			return TextureReplacer.BepInExPlugin.cachedCubemap.TryGetValue(name, out cubemap) ? cubemap : (Cubemap)null;
		}

		private static void CacheFontsAtStart()
		{
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_ttf)
			{
				string key = keyValuePair.Key.Substring(0, keyValuePair.Key.LastIndexOf('.'));
				switch (key)
				{
					case "Card_Font":
						TMP_FontAsset fontAsset1 = TMP_FontAsset.CreateFontAsset(new UnityEngine.Font(keyValuePair.Value + keyValuePair.Key));
						fontAsset1.material.shader = Shader.Find("TextMeshPro/Distance Field");
						TextureReplacer.BepInExPlugin.cachedFonts[key] = fontAsset1;
						break;
					case "ShopName_Font":
						TMP_FontAsset fontAsset2 = TMP_FontAsset.CreateFontAsset(new UnityEngine.Font(keyValuePair.Value + keyValuePair.Key));
						fontAsset2.material.shader = Shader.Find("TextMeshPro/Distance Field");
						TextureReplacer.BepInExPlugin.cachedFonts[key] = fontAsset2;
						break;
				}
			}
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_tex)
			{
				if (keyValuePair.Key == "Card_Font.txt")
				{
					string[] strArray = File.ReadAllLines(keyValuePair.Value + keyValuePair.Key);
					if (strArray.Length == 3)
					{
						UnityEngine.Color color;
						if (ColorUtility.TryParseHtmlString("#" + strArray[0], out color))
							TextureReplacer.BepInExPlugin.card_color = color;
						if (ColorUtility.TryParseHtmlString("#" + strArray[1], out color))
							TextureReplacer.BepInExPlugin.card_color_outline = color;
						try
						{
							TextureReplacer.BepInExPlugin.card_outline = float.Parse(strArray[2], (IFormatProvider)CultureInfo.InvariantCulture);
						}
						catch
						{
						}
					}
				}
			}
			Console.WriteLine((object)"Card Font cached at start");
		}

		private static TMP_FontAsset GetCachedFont(string name)
		{
			TMP_FontAsset tmpFontAsset;
			return TextureReplacer.BepInExPlugin.cachedFonts.TryGetValue(name, out tmpFontAsset) ? tmpFontAsset : (TMP_FontAsset)null;
		}

		private static void CacheCardDataAtStart()
		{
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_nam)
			{
				string[] strArray = File.ReadAllLines(keyValuePair.Value + keyValuePair.Key);
				if (strArray.Length != 0 && !string.IsNullOrEmpty(strArray[0]))
					TextureReplacer.BepInExPlugin.cachedData[keyValuePair.Key] = strArray[0];
			}
			Console.WriteLine((object)"Card Data cached at start");
		}

		private static string GetCachedCardData(string name)
		{
			string str;
			return TextureReplacer.BepInExPlugin.cachedData.TryGetValue(name, out str) ? str : (string)null;
		}

		private static void OnShopSignChanged(object sender, EventArgs e)
		{
			if (!(SceneManager.GetActiveScene().name == "Start"))
				return;
			UpdateShopSign();
		}

		private static void OnShopSignFontChanged(object sender, EventArgs e)
		{
			if (!(SceneManager.GetActiveScene().name == "Start"))
				return;
			UpdateShopSignFont();
		}

		private static void OnShopSignOBJChanged(object sender, EventArgs e)
		{
			if (!(SceneManager.GetActiveScene().name == "Start"))
				return;
			UpdateShopOBJSign();
		}

		private static void UpdateShopSign()
		{
			RectTransform rectTransform = ((IEnumerable<RectTransform>)UnityEngine.Resources.FindObjectsOfTypeAll<RectTransform>()).FirstOrDefault<RectTransform>((Func<RectTransform, bool>)(rt => rt.gameObject.name == "Billboard_ShopName_Text"));
			if ((UnityEngine.Object)rectTransform != (UnityEngine.Object)null)
			{
				rectTransform.position = new Vector3(TextureReplacer.BepInExPlugin.ShopSignX.Value, TextureReplacer.BepInExPlugin.ShopSignY.Value, TextureReplacer.BepInExPlugin.ShopSignZ.Value);
				rectTransform.sizeDelta = new Vector2(TextureReplacer.BepInExPlugin.ShopSignW.Value, TextureReplacer.BepInExPlugin.ShopSignH.Value);
			}
			CanvasRenderer canvasRenderer = ((IEnumerable<CanvasRenderer>)UnityEngine.Resources.FindObjectsOfTypeAll<CanvasRenderer>()).FirstOrDefault<CanvasRenderer>((Func<CanvasRenderer, bool>)(rt => rt.gameObject.name == "Billboard_ShopName_Text"));
			if (!((UnityEngine.Object)canvasRenderer != (UnityEngine.Object)null))
				return;
			TMP_Text component = canvasRenderer.GetComponent<TMP_Text>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
			{
				if (component.enableVertexGradient)
				{
					VertexGradient colorGradient = component.colorGradient with
					{
						topLeft = TextureReplacer.BepInExPlugin.ShopSignColor.Value,
						topRight = TextureReplacer.BepInExPlugin.ShopSignColor.Value,
						bottomLeft = TextureReplacer.BepInExPlugin.ShopSignColor2.Value,
						bottomRight = TextureReplacer.BepInExPlugin.ShopSignColor2.Value
					};
					component.colorGradient = colorGradient;
				}
				component.color = UnityEngine.Color.white;
				component.fontSize = TextureReplacer.BepInExPlugin.ShopSignFontSize.Value;
				component.fontSizeMax = TextureReplacer.BepInExPlugin.ShopSignFontSize.Value;
				CSingleton<LightManager>.Instance.ToggleShopLight();
				CSingleton<LightManager>.Instance.ToggleShopLight();
			}
		}

		private static void UpdateShopSignFont()
		{
			CanvasRenderer canvasRenderer = ((IEnumerable<CanvasRenderer>)UnityEngine.Resources.FindObjectsOfTypeAll<CanvasRenderer>()).FirstOrDefault<CanvasRenderer>((Func<CanvasRenderer, bool>)(rt => rt.gameObject.name == "Billboard_ShopName_Text"));
			if (!((UnityEngine.Object)canvasRenderer != (UnityEngine.Object)null))
				return;
			TMP_Text component = canvasRenderer.GetComponent<TMP_Text>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
			{
				if (TextureReplacer.BepInExPlugin.UseCustomShopSignFont.Value)
				{
					TMP_FontAsset cachedFont = TextureReplacer.BepInExPlugin.Instance.GetCachedFont("ShopName_Font");
					if ((UnityEngine.Object)cachedFont != (UnityEngine.Object)null)
						component.font = cachedFont;
				}
				else
				{
					if ((UnityEngine.Object)TextureReplacer.BepInExPlugin.billboardTextFont == (UnityEngine.Object)null)
						TextureReplacer.BepInExPlugin.billboardTextFont = component.font;
					component.font = TextureReplacer.BepInExPlugin.billboardTextFont;
				}
			}
		}

		private static void UpdateShopOBJSign()
		{
			MeshFilter meshFilter = ((IEnumerable<MeshFilter>)UnityEngine.Resources.FindObjectsOfTypeAll<MeshFilter>()).FirstOrDefault<MeshFilter>((Func<MeshFilter, bool>)(rt => rt.gameObject.name == "Billboard"));
			if (!((UnityEngine.Object)meshFilter != (UnityEngine.Object)null))
				return;
			Transform transform1 = meshFilter.gameObject.transform;
			Transform transform2 = CSingleton<LightManager>.Instance.m_ShoplightGrp.transform;
			Vector3 vector3 = new Vector3(4.941549f, 1.716644f, -5.770413f) - new Vector3(4.809998f, 3.940002f, -9.816345f);
			transform1.position = new Vector3(TextureReplacer.BepInExPlugin.ShopSignOBJX.Value, TextureReplacer.BepInExPlugin.ShopSignOBJY.Value, TextureReplacer.BepInExPlugin.ShopSignOBJZ.Value);
			transform2.position = transform1.position + vector3;
		}

		private static IEnumerator UpdateShopSignDelay()
		{
			CanvasRenderer billboardRectTransform = (CanvasRenderer)null;
			while ((UnityEngine.Object)billboardRectTransform == (UnityEngine.Object)null)
			{
				billboardRectTransform = ((IEnumerable<CanvasRenderer>)UnityEngine.Resources.FindObjectsOfTypeAll<CanvasRenderer>()).FirstOrDefault<CanvasRenderer>((Func<CanvasRenderer, bool>)(rt => rt.gameObject.name == "Billboard_ShopName_Text"));
				if ((UnityEngine.Object)billboardRectTransform == (UnityEngine.Object)null)
					yield return (object)new WaitForSeconds(1f);
			}
			TMP_Text billboardText;
			for (billboardText = billboardRectTransform.GetComponent<TMP_Text>(); (UnityEngine.Object)billboardText == (UnityEngine.Object)null; billboardText = billboardRectTransform.GetComponent<TMP_Text>())
				yield return (object)new WaitForSeconds(1f);
			for (float timeout = 10f; billboardText.text == "MY CARD EMPIRE" && (double)timeout > 0.0; --timeout)
				yield return (object)new WaitForSeconds(1f);
			CSingleton<LightManager>.Instance.m_BillboardText.color = UnityEngine.Color.white;
			UnityEngine.Color bcolor = (UnityEngine.Color)Traverse.Create((object)CSingleton<LightManager>.Instance).Field("m_BillboardTextOriginalColor").GetValue();
			bcolor = UnityEngine.Color.white;
			Traverse.Create((object)CSingleton<LightManager>.Instance).Field("m_BillboardTextOriginalColor").SetValue((object)bcolor);
			if ((UnityEngine.Object)TextureReplacer.BepInExPlugin.billboardTextFont == (UnityEngine.Object)null)
				TextureReplacer.BepInExPlugin.billboardTextFont = billboardText.font;
			if (TextureReplacer.BepInExPlugin.UseCustomShopSignFont.Value)
			{
				TMP_FontAsset customFont = TextureReplacer.BepInExPlugin.Instance.GetCachedFont("ShopName_Font");
				if ((UnityEngine.Object)customFont != (UnityEngine.Object)null)
					billboardText.font = customFont;
				customFont = (TMP_FontAsset)null;
			}
			cardname = "";
			UpdateShopSign();
		}

		private static IEnumerator UpdateShopSignOBJDelay()
		{
			MeshFilter billboardRectTransform = (MeshFilter)null;
			while ((UnityEngine.Object)billboardRectTransform == (UnityEngine.Object)null)
			{
				billboardRectTransform = ((IEnumerable<MeshFilter>)UnityEngine.Resources.FindObjectsOfTypeAll<MeshFilter>()).FirstOrDefault<MeshFilter>((Func<MeshFilter, bool>)(rt => rt.gameObject.name == "Billboard"));
				if ((UnityEngine.Object)billboardRectTransform == (UnityEngine.Object)null)
					yield return (object)new WaitForSeconds(1f);
			}
			UpdateShopOBJSign();
		}

		private static void DoReplace()
		{
			TextureReplacer.BepInExPlugin.mesh_list = UnityEngine.Resources.FindObjectsOfTypeAll<MeshFilter>();
			if (TextureReplacer.BepInExPlugin.mesh_list.Length != 0)
			{
				foreach (MeshFilter mesh in TextureReplacer.BepInExPlugin.mesh_list)
				{
					if ((UnityEngine.Object)mesh != (UnityEngine.Object)null && (UnityEngine.Object)mesh.sharedMesh != (UnityEngine.Object)null)
					{
						Mesh cachedMesh = GetCachedMesh(mesh.sharedMesh.name.Replace("'s", ""));
						if ((UnityEngine.Object)cachedMesh != (UnityEngine.Object)null)
						{
							mesh.sharedMesh = cachedMesh;
							mesh.sharedMesh.UploadMeshData(true);
						}
					}
				}
				Console.WriteLine((object)"Custom 3D Objects loaded!");
			}
			TextureReplacer.BepInExPlugin.mat_list = UnityEngine.Resources.FindObjectsOfTypeAll<Material>();
			if (TextureReplacer.BepInExPlugin.mat_list.Length != 0)
			{
				foreach (Material mat in TextureReplacer.BepInExPlugin.mat_list)
				{
					bool flag = false;
					if ((UnityEngine.Object)mat != (UnityEngine.Object)null)
					{
						Material material = mat;
						foreach (string texturePropertyName in material.GetTexturePropertyNames())
						{
							try
							{
								string str = "";
								if ((UnityEngine.Object)material.GetTexture(texturePropertyName) != (UnityEngine.Object)null)
									str = material.GetTexture(texturePropertyName).name;
								Texture2D texture2D;
								if (str == "T_PaperBagAlbedoClosed" || str == "T_PaperBagAlbedoOpen")
								{
									if (bagname == "")
									{
										texture2D = GetRandomTexture(str);
									}
									else
									{
										texture2D = GetCachedTexture(bagname.Replace("T_PaperBagAlbedoClosed", str).Replace("T_PaperBagAlbedoOpen", str));
										if ((UnityEngine.Object)texture2D == (UnityEngine.Object)null)
											texture2D = GetRandomTexture(str);
									}
								}
								else
									texture2D = !(str == "wood1") && !(str == "credit_card_D") && !(str == "mcp_building_32_billboards_d") ? GetCachedTexture(str) : (!(str == "wood1") ? GetRandomTexture(str) : GetRandomTexture("wood"));
								if ((UnityEngine.Object)texture2D != (UnityEngine.Object)null)
								{
									texture2D.name = str;
									if (!flag && (texturePropertyName.Contains("_BaseMap") || texturePropertyName.Contains("_MainTex")))
									{
										flag = true;
										if (texturePropertyName.Contains("_MainTex"))
											material.mainTexture = (Texture)texture2D;
										else
											material.SetTexture("_BaseMap", (Texture)texture2D);
										if (material.HasProperty("_BaseColor"))
										{
											Vector4 vector = material.GetVector("_BaseColor");
											material.SetVector("_BaseColor", new Vector4(1f, 1f, 1f, vector.w));
										}
										if (str == "floor" || str == "wood 1" || str == "wood 2 wall" || str == "Ceiling")
										{
											string name1 = "";
											string name2 = "";
											switch (str)
											{
												case "floor":
													name1 = "floor_m";
													name2 = "floor_n";
													break;
												case "wood 1":
													name1 = "wood 1_m";
													name2 = "wood 1_n";
													break;
												case "wood 2 wall":
													name1 = "wood 2 wall_m";
													name2 = "wood 2 wall_n";
													break;
												case "Ceiling":
													name1 = "Ceiling M";
													material.SetFloat("_Mode", 2f);
													material.SetOverrideTag("RenderType", "Transparent");
													material.SetInt("_SrcBlend", 5);
													material.SetInt("_DstBlend", 10);
													material.SetInt("_ZWrite", 1);
													material.DisableKeyword("_ALPHATEST_ON");
													material.EnableKeyword("_ALPHABLEND_ON");
													material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
													material.renderQueue = 3000;
													break;
											}
											Texture2D cachedTexture1 = GetCachedTexture(name1);
											if ((UnityEngine.Object)cachedTexture1 != (UnityEngine.Object)null && material.HasProperty("_MetallicGlossMap"))
											{
												material.EnableKeyword("_METALLICGLOSSMAP");
												material.SetTexture("_MetallicGlossMap", (Texture)cachedTexture1);
											}
											Texture2D cachedTexture2 = GetCachedTexture(name2);
											if ((UnityEngine.Object)cachedTexture2 != (UnityEngine.Object)null && material.HasProperty("_BumpMap"))
											{
												material.EnableKeyword("_NORMALMAP");
												material.SetTexture("_BumpMap", (Texture)cachedTexture2);
												material.SetInt("_UseNormalMap", 1);
											}
										}
									}
									else if (!flag || !texturePropertyName.Contains("_BaseMap") && !texturePropertyName.Contains("_MainTex"))
									{
										if (texturePropertyName.Contains("_BumpMap"))
										{
											material.EnableKeyword("_NORMALMAP");
											material.SetTexture("_BumpMap", (Texture)texture2D);
											material.SetInt("_UseNormalMap", 1);
										}
										else if (texturePropertyName.Contains("_MetallicGlossMap"))
										{
											material.EnableKeyword("_METALLICGLOSSMAP");
											material.SetTexture("_MetallicGlossMap", (Texture)texture2D);
										}
										else if (texturePropertyName.Contains("_OcclusionMap"))
											material.SetTexture("_OcclusionMap", (Texture)texture2D);
									}
									else
										continue;
									if (str.StartsWith("FS003_"))
									{
										Cubemap cachedCubemap = GetCachedCubemap(str);
										if ((UnityEngine.Object)cachedCubemap != (UnityEngine.Object)null)
										{
											cachedCubemap.name = str;
											material.SetTexture(texturePropertyName, (Texture)cachedCubemap);
										}
									}
									else if (!texturePropertyName.Contains("_BaseMap") && !texturePropertyName.Contains("_MainTex"))
										material.SetTexture(texturePropertyName, (Texture)texture2D);
								}
							}
							catch
							{
							}
						}
					}
				}
			}
			TextureReplacer.BepInExPlugin.image_list = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.UI.Image>();
			if (TextureReplacer.BepInExPlugin.image_list.Length != 0)
			{
				foreach (UnityEngine.UI.Image image in TextureReplacer.BepInExPlugin.image_list)
				{
					if ((UnityEngine.Object)image != (UnityEngine.Object)null && (UnityEngine.Object)image.sprite != (UnityEngine.Object)null)
					{
						Texture2D texture;
						if (image.sprite.name == "GameStartBG_Blur")
							texture = GetRandomTexture("GameStartBG_Blur");
						else if (image.sprite.name == "PhoneBG2")
							texture = GetRandomTexture("PhoneBG");
						else if (image.sprite.name == "CardBack")
						{
							if (cardname == "")
							{
								texture = GetRandomTexture("CardBack");
							}
							else
							{
								texture = GetCachedTexture(cardname);
								if ((UnityEngine.Object)texture == (UnityEngine.Object)null)
									texture = GetRandomTexture("CardBack");
							}
						}
						else
							texture = GetCachedTexture(image.sprite.name);
						if ((UnityEngine.Object)texture != (UnityEngine.Object)null)
						{
							texture.name = image.sprite.name;
							Sprite sprite = TextureReplacer.BepInExPlugin.TextureToSprite(texture);
							image.sprite = sprite;
							image.sprite.name = texture.name;
						}
					}
				}
			}
			FixSomeObjects();
			FixPhone();
			changeMenu();
			ReplaceSpriteLists();
			FixControllerSprites();
			if (SceneManager.GetActiveScene().name == "Start")
			{
				UpdateShopSignDelay();
				UpdateShopSignOBJDelay();
			}
			Console.WriteLine((object)"Custom Textures loaded!");
		}

		private static Texture2D GetRandomTexture(string baseName)
		{
			List<string> source = new List<string>();
			foreach (string key in TextureReplacer.BepInExPlugin.cachedTextures.Keys)
			{
				if (key == baseName)
					source.Add(key);
				else if (key.StartsWith(baseName) && !key.StartsWith("wood ") && int.TryParse(key.Substring(baseName.Length), out int _))
					source.Add(key);
			}
			if (source.Count == 0)
				return (Texture2D)null;
			List<string> list = source.OrderBy<string, float>((Func<string, float>)(x => UnityEngine.Random.value)).ToList<string>();
			string key1 = list[UnityEngine.Random.Range(0, list.Count)];
			if (key1.Contains("T_PaperBagAlbedoClosed") || key1.Contains("T_PaperBagAlbedoOpen"))
				bagname = key1;
			if (key1.Contains("PlayTable_chair") || key1.Contains("PlayTable_chair_metal") || key1.Contains("PlayTable_metal") || key1.Contains("PlayTable_wood"))
				playtablename = key1;
			if (key1.Contains("CardBack"))
				cardname = key1;
			return TextureReplacer.BepInExPlugin.cachedTextures[key1];
		}

		private static void FixSomeObjects()
		{
			foreach (Renderer renderer in UnityEngine.Resources.FindObjectsOfTypeAll<Renderer>())
			{
				if ((UnityEngine.Object)renderer != (UnityEngine.Object)null && ((IEnumerable<string>)TextureReplacer.BepInExPlugin.renderer_names).Contains<string>(renderer.name))
				{
					Material[] materials = renderer.materials;
					if (renderer.name == "SmallMetalShelf")
					{
						UpdateMaterial(renderer.name, renderer, materials, "DarkMetal", "SmallCabinet_metal_dark");
						UpdateMaterial(renderer.name, renderer, materials, "MediumMetal", "SmallCabinet_metal_medium");
					}
					else if (renderer.name == "SmallPersonalShelf")
					{
						UpdateMaterial(renderer.name, renderer, materials, "DarkMetal", "PersonalShelfSmall_metal_dark");
						UpdateMaterial(renderer.name, renderer, materials, "MediumLightMetal", "PersonalShelfSmall_metal_medium");
						UpdateMaterialGlass(materials, "Glass_03(Clear)", "PersonalShelfSmall_glass.txt");
					}
					else if (renderer.name == "PersonalShelfGlass")
					{
						UpdateMaterial(renderer.name, renderer, materials, "DarkMetal", "PersonalShelfBig_metal_dark");
						UpdateMaterial(renderer.name, renderer, materials, "MediumLightMetal", "PersonalShelfBig_metal_medium");
						UpdateMaterial(renderer.name, renderer, materials, "LightMetal", "PersonalShelfBig_metal_light");
						UpdateMaterialGlass(materials, "Glass_02(Clear)", "PersonalShelfBig_glass.txt");
					}
					else if (renderer.name == "HugePersonalShelf")
					{
						UpdateMaterial(renderer.name, renderer, materials, "DarkMetal", "PersonalShelfHuge_metal_dark");
						UpdateMaterial(renderer.name, renderer, materials, "metal_bronze", "PersonalShelfHuge_bronze");
						UpdateMaterial(renderer.name, renderer, materials, "dark_bronze", "PersonalShelfHuge_bronze_dark");
						UpdateMaterialGlass(materials, "Glass_02(Clear)", "PersonalShelfHuge_glass.txt");
					}
					else if (renderer.name == "TallCardDisplayCase")
					{
						UpdateMaterial(renderer.name, renderer, materials, "DarkMetal", "CardDisplaySmall_metal_dark");
						UpdateMaterial(renderer.name, renderer, materials, "MediumMetal", "CardDisplaySmall_metal_medium");
						UpdateMaterialGlass(materials, "Glass_03(Clear)", "CardDisplaySmall_glass.txt");
					}
					else if (renderer.name == "AdapterMesh" && GetFirstParent(renderer.gameObject).name == "DisplayCardShelfA")
						UpdateMaterial(renderer.name, renderer, materials, "adapter plastic", "CardDisplaySmall_plastic");
					else if (renderer.name == "SleekCardDisplayCase")
					{
						UpdateMaterial(renderer.name, renderer, materials, "DarkMetal", "CardDisplayBig_metal_dark");
						UpdateMaterial(renderer.name, renderer, materials, "LightMetal", "CardDisplayBig_metal_light");
						UpdateMaterial(renderer.name, renderer, materials, "LightBright", "CardDisplayBig_metal_bright");
						UpdateMaterialGlass(materials, "Glass_03(Clear)", "CardDisplayBig_glass.txt");
					}
					else if (renderer.name == "AdapterMesh" && GetFirstParent(renderer.gameObject).name == "DisplayCardShelfB")
						UpdateMaterial(renderer.name, renderer, materials, "adapter plastic", "CardDisplayBig_plastic");
					else if (renderer.name == "Weapons closet")
					{
						UpdateMaterial(renderer.name, renderer, materials, "Weapons closet", "CardTable");
						UpdateMaterial(renderer.name, renderer, materials, "Weapons closet metal", "CardTable_metal");
					}
					else if (renderer.name == "AdapterMesh" && GetFirstParent(renderer.gameObject).name == "CardShelfGrp")
						UpdateMaterial(renderer.name, renderer, materials, "adapter plastic", "CardTable_plastic");
					else if (renderer.name == "CardDisplayTable")
					{
						UpdateMaterial(renderer.name, renderer, materials, "black material", "CardDisplayTable");
						UpdateMaterialGlass(materials, "Glass_03(Clear)", "CardDisplayTable_glass.txt");
						UpdateMaterialGlass(materials, "GlassEdge", "CardDisplayTable_glass_edge.txt");
					}
					else if (renderer.name == "AdapterMesh" && GetFirstParent(renderer.gameObject).name == "DisplayCardTableA")
						UpdateMaterial(renderer.name, renderer, materials, "adapter plastic", "CardDisplayTable_plastic");
					else if (renderer.name == "AdapterMesh" && GetFirstParent(renderer.gameObject).name == "VintageCardTableLong")
						UpdateMaterial(renderer.name, renderer, materials, "adapter plastic", "CardVintageTable_plastic");
					else if (renderer.name == "ShelvingModel" && GetFirstParent(renderer.gameObject).name == "SmallShelfA")
						UpdateMaterial(renderer.name, renderer, materials, "MAT_ShelfA", "SidedShelfSingle");
					else if (renderer.name == "ShelvingModel" && GetFirstParent(renderer.gameObject).name == "ShelfGrp")
						UpdateMaterial(renderer.name, renderer, materials, "shelving", "SidedShelfDouble");
					else if (renderer.name == "CounterModel")
					{
						UpdateMaterial(renderer.name, renderer, materials, "Weapons closet", "CheckoutCounter");
						UpdateMaterial(renderer.name, renderer, materials, "Weapons closet metal", "CheckoutCounter_metal");
					}
					else if (renderer.name == "Monitor" && GetFirstParent(renderer.gameObject).name == "Interactable_CashierCounter")
					{
						UpdateMaterial(renderer.name, renderer, materials, "Cash_R_Monitor_Button", "CashRegister_button");
						UpdateMaterial(renderer.name, renderer, materials, "Cash_R_Monitor_Silver", "CashRegister_silver");
						UpdateMaterial(renderer.name, renderer, materials, "Cash_R_Monitor_Screen", "CashRegister_Screen");
						UpdateMaterial(renderer.name, renderer, materials, "Cash_R_Monitor_Black_Plastic", "CashRegister_plastic");
						UpdateMaterial(renderer.name, renderer, materials, "lambert3", "CashRegister_lambert1");
						UpdateMaterial(renderer.name, renderer, materials, "lambert4", "CashRegister_lambert2");
						UpdateMaterial(renderer.name, renderer, materials, "lambert5", "CashRegister_lambert3");
						UpdateMaterial(renderer.name, renderer, materials, "lambert6", "CashRegister_lambert4");
					}
					else if (renderer.name == "DrawerModel" && GetFirstParent(renderer.gameObject).name == "Interactable_CashierCounter")
					{
						UpdateMaterial(renderer.name, renderer, materials, "lambert3", "CashRegister_lambert5");
						UpdateMaterial(renderer.name, renderer, materials, "lambert4", "CashRegister_lambert6");
					}
					else if (renderer.name == "LongShelf")
						UpdateMaterial(renderer.name, renderer, materials, "DarkMetal", "WideShelf");
					else if (renderer.name == "Model")
					{
						if (GetFirstParent(renderer.gameObject).name == "Interactable_TrashBin" || GetFirstParent(renderer.gameObject).name == "ShelfManager")
						{
							UpdateMaterial(renderer.name, renderer, materials, "black plastic", "TrashBin_plastic");
							UpdateMaterial(renderer.name, renderer, materials, "bin metallic", "TrashBin_metal");
						}
					}
					else if (renderer.name == "Workbench" && GetFirstParent(renderer.gameObject).name == "InteractableWorkbench")
					{
						UpdateMaterial(renderer.name, renderer, materials, "DarkMetal", "Workbench_metal_dark");
						UpdateMaterial(renderer.name, renderer, materials, "MediumMetal", "Workbench_metal_medium");
					}
					else if (GetFirstParent(renderer.gameObject).name == "Interactable_PlayTable")
					{
						if (renderer.name == "Table 1_Half")
						{
							UpdateMaterial(renderer.name, renderer, materials, "wood1", "PlayTable_wood");
							UpdateMaterial(renderer.name, renderer, materials, "table metallic", "PlayTable_metal");
						}
						else if (renderer.name == "Table 1")
						{
							UpdateMaterial(renderer.name, renderer, materials, "wood1", "PlayTable_wood");
							UpdateMaterial(renderer.name, renderer, materials, "table metallic", "PlayTable_metal");
						}
						else if (renderer.name == "Chair 2 (10)" || renderer.name == "Chair 2 (11)" || renderer.name == "Chair 2 (12)" || renderer.name == "Chair 2 (13)" || renderer.name == "Chair 2 (14)" || renderer.name == "Chair 2 (15)")
						{
							UpdateMaterial(renderer.name, renderer, materials, "white wood", "PlayTable_chair");
							UpdateMaterial(renderer.name, renderer, materials, "chair metallic", "PlayTable_chair_metal");
						}
					}
					else if (renderer.name == "door B")
					{
						if (GetParent(renderer.gameObject).name == "Windows door")
						{
							UpdateMaterialBuildings(renderer.name, renderer, materials, "metal material", "StoreFront_metal_dark");
							UpdateMaterialBuildings(renderer.name, renderer, materials, "black metal", "StoreFront_metal_bright");
							UpdateMaterialGlass(materials, "window glass", "StoreFront_glass.txt");
						}
					}
					else if (renderer.name == "windows door")
					{
						if (GetParent(renderer.gameObject).name == "Windows door")
						{
							UpdateMaterialBuildings(renderer.name, renderer, materials, "metal material", "StoreFront_metal_dark");
							UpdateMaterialGlass(materials, "window glass", "StoreFront_glass.txt");
						}
					}
					else if (renderer.name == "GlassDoor")
					{
						if (GetGrandParent(renderer.gameObject).name.Contains("GlassDoorGrp"))
						{
							UpdateMaterialBuildings(renderer.name, renderer, materials, "metal material", "StoreFront_metal_dark");
							UpdateMaterialGlass(materials, "window glass", "StoreFront_glass.txt");
						}
					}
					else if (renderer.name == "House01_BtmCut")
					{
						UpdateMaterialBuildings(renderer.name, renderer, materials, "House_01_Wall_01", "StoreBFront_wall_stone");
						UpdateMaterialBuildings(renderer.name, renderer, materials, "House_01_Wall_02", "stone shopBwall");
						UpdateMaterialBuildings(renderer.name, renderer, materials, "door wood", "StoreBFront_doorframe2_wood");
						UpdateMaterialBuildings(renderer.name, renderer, materials, "House_01_Window", "shopBwindow_frame");
						UpdateMaterialGlass(materials, "Glass_02(Fade)", "StoreBFront_glass.txt");
					}
					else if (renderer.name == "House_01_BtmCut1")
						UpdateMaterialBuildings(renderer.name, renderer, materials, "House_01_Window", "shopBwindow");
					else if (renderer.name == "Door")
					{
						if (GetParent(renderer.gameObject).name.Contains("OpenedDoor1") || GetParent(renderer.gameObject).name.Contains("LockedDoor1"))
						{
							UpdateMaterialBuildings(renderer.name, renderer, materials, "door wood", "StoreBFront_door_wood");
							UpdateMaterialBuildings(renderer.name, renderer, materials, "door metal", "StoreBFront_metal_bright");
						}
						if (GetParent(renderer.gameObject).name.Contains("OpenedDoor2") || GetParent(renderer.gameObject).name.Contains("LockedDoor2"))
						{
							UpdateMaterialBuildings(renderer.name, renderer, materials, "door wood", "StoreFront_door_wood");
							UpdateMaterialBuildings(renderer.name, renderer, materials, "door metal", "StoreFront_metal_bright");
						}
					}
					else if (renderer.name == "crank")
					{
						if (GetGrandParent(renderer.gameObject).name.Contains("OpenedDoor1") || GetGrandParent(renderer.gameObject).name.Contains("LockedDoor1"))
							UpdateMaterialBuildings(renderer.name, renderer, materials, "door metal", "StoreBFront_crank_metal_bright");
						if (GetGrandParent(renderer.gameObject).name.Contains("OpenedDoor2") || GetGrandParent(renderer.gameObject).name.Contains("LockedDoor2"))
							UpdateMaterialBuildings(renderer.name, renderer, materials, "door metal", "StoreFront_crank_metal_bright");
					}
					else if (renderer.name == "door frame")
					{
						if (GetParent(renderer.gameObject).name.Contains("OpenedDoor1") || GetParent(renderer.gameObject).name.Contains("LockedDoor1"))
							UpdateMaterialBuildings(renderer.name, renderer, materials, "door wood", "StoreBFront_doorframe_wood");
						if (GetParent(renderer.gameObject).name.Contains("OpenedDoor2") || GetParent(renderer.gameObject).name.Contains("LockedDoor2"))
							UpdateMaterialBuildings(renderer.name, renderer, materials, "door wood", "StoreFront_doorframe_wood");
					}
					else if (renderer.name == "House19_Cut")
					{
						if (GetParent(renderer.gameObject).name.Contains("ShopGrp"))
						{
							UpdateMaterialOnce(renderer.name, renderer, materials[0], "House_02_Wall_01", "StoreFront_brickwall_outside");
							UpdateMaterialOnce(renderer.name, renderer, materials[2], "Roof_02", "StoreFront_roof");
							UpdateMaterialOnce(renderer.name, renderer, materials[3], "House_03_Roof", "StoreFront_roof_windows");
							UpdateMaterialOnce(renderer.name, renderer, materials[4], "Bay_Window", "StoreFront_bay_windows");
							UpdateMaterialOnce(renderer.name, renderer, materials[5], "Glass_01", "StoreFront_windowsGlass_outside");
							UpdateMaterialOnce(renderer.name, renderer, materials[6], "House_02_Door", "StoreFront_windows_frame");
							UpdateMaterialOnce(renderer.name, renderer, materials[7], "House_07_Windows", "StoreFront_windows_walledup");
							UpdateMaterialOnce(renderer.name, renderer, materials[1], "Porch_02", "Store_AltCeiling");
						}
					}
					else if (renderer.name == "Wall 1 (1)" || renderer.name == "Wall_SideB_LotA_1" || renderer.name == "Wall_Back_LotA_1" || renderer.name == "Wall_Back_LotA_2" || renderer.name == "Wall 1 (1)")
					{
						if (GetParent(renderer.gameObject).name.Contains("ShopGrp"))
						{
							UpdateMaterialOnce(renderer.name, renderer, materials[0], "wood 2 wall", "wood 2 wall");
							UpdateMaterialOnce(renderer.name, renderer, materials[1], "wood 1", "wood 1");
							UpdateMaterialOnce(renderer.name, renderer, materials[2], "wood 2 wall", "wood 2 wall");
						}
					}
					else if (renderer.name == "House19_BtmCut")
					{
						if (GetGrandParent(renderer.gameObject).name.Contains("LockedRoomBlocker_Grp"))
						{
							UpdateMaterialOnce(renderer.name, renderer, materials[0], "House_02_Wall_01", "StoreFront_brickwallBottom_outside");
							UpdateMaterialOnce(renderer.name, renderer, materials[1], "House_02_Door", "StoreFront_windowsBottom_frame");
							UpdateMaterialOnce(renderer.name, renderer, materials[2], "Glass_01", "StoreFront_windowsGlassBottom_outside");
						}
					}
					else if (renderer.name == "Wall_Side_LotA_1" || renderer.name == "Wall_Side_LotA_2")
					{
						if (GetParent(renderer.gameObject).name.Contains("WarehouseGrp"))
						{
							UpdateMaterialOnce(renderer.name, renderer, materials[0], "wood 2 wall", "wood 2 wall");
							UpdateMaterialOnce(renderer.name, renderer, materials[1], "wood 1", "wood 1");
						}
					}
					else if (renderer.name == "Wall 2")
					{
						if (GetParent(renderer.gameObject).name.Contains("WarehouseGrp"))
							UpdateMaterialOnce(renderer.name, renderer, materials[0], "wood 2 wall", "shopBwall");
					}
					else if (renderer.name == "Wall 1 (4)" || renderer.name == "Wall 1 (5)")
					{
						if (GetParent(renderer.gameObject).name.Contains("ShopGrp"))
						{
							UpdateMaterialOnce(renderer.name, renderer, materials[0], "wood 2 wall", "shopBwall");
							UpdateMaterialOnce(renderer.name, renderer, materials[2], "wood 2 wall", "shopBwall");
							UpdateMaterialOnce(renderer.name, renderer, materials[1], "wood 1", "wood shopBwall");
						}
					}
					else if (renderer.name == "Wall_Back_LotB")
					{
						if (GetParent(renderer.gameObject).name.Contains("ShopGrp"))
						{
							UpdateMaterialOnce(renderer.name, renderer, materials[1], "wood 1", "wood shopBwall");
							UpdateMaterialOnce(renderer.name, renderer, materials[2], "wood 2 wall", "shopBwall");
						}
					}
					else if (renderer.name == "Wall_Side_LotB_1" || renderer.name == "Wall_Side_LotB_2")
					{
						if (GetParent(renderer.gameObject).name.Contains("WarehouseGrp"))
						{
							UpdateMaterialOnce(renderer.name, renderer, materials[0], "wood 2 wall", "shopBwall");
							UpdateMaterialOnce(renderer.name, renderer, materials[1], "wood 1", "wood shopBwall");
							UpdateMaterialOnce(renderer.name, renderer, materials[2], "wood 2 wall", "shopBwall");
						}
					}
					else if (renderer.name == "WallMesh_Side" || renderer.name == "WallMesh_Front" || renderer.name == "WallMesh (2)")
					{
						if (GetGrandParent(renderer.gameObject).name.Contains("WHLockedRoomBlocker_Grp"))
						{
							UpdateMaterialOnce(renderer.name, renderer, materials[0], "wood 2 wall", "shopBwall");
							UpdateMaterialOnce(renderer.name, renderer, materials[2], "wood 2 wall", "shopBwall");
							UpdateMaterialOnce(renderer.name, renderer, materials[1], "wood 1", "wood shopBwall");
						}
					}
					else if (renderer.name == "Ceiling (1)" || renderer.name == "Ceiling (3)")
					{
						if ((!(renderer.name == "Ceiling (1)") || (double)renderer.transform.position.z >= -15.0) && GetParent(renderer.gameObject).name.Contains("ShopGrp"))
							UpdateMaterialOnce(renderer.name, renderer, materials[0], "Ceiling", "Ceiling2");
					}
					else if (renderer.name == "Floor (1)" || renderer.name == "Floor (3)")
					{
						if (renderer.name == "Floor (1)")
						{
							Console.WriteLine((object)renderer.transform.position.z);
							if ((double)renderer.transform.position.z < -15.0)
								continue;
						}
						if (GetParent(renderer.gameObject).name.Contains("ShopGrp"))
							UpdateMaterialOnce(renderer.name, renderer, materials[0], "floor", "floor2");
					}
					else if (renderer.name == "House01_Cut")
					{
						UpdateMaterialBuildings(renderer.name, renderer, materials, "House_01_Wall_01", "StoreBFront_stonewall_outside");
						UpdateMaterialBuildings(renderer.name, renderer, materials, "Roof_02", "StoreBFront_roof");
						UpdateMaterialBuildings(renderer.name, renderer, materials, "House_01_Window", "StoreBFront_windows_outside");
						UpdateMaterialBuildings(renderer.name, renderer, materials, "Glass_01", "StoreBFront_windowsGlass_outside");
						UpdateMaterialOnce(renderer.name, renderer, materials[2], "House_01_Wall_02", "StoreB_HiddenObject");
						UpdateMaterialOnce(renderer.name, renderer, materials[3], "House_01_Wall_02", "StoreB_HiddenWall");
						UpdateMaterialOnce(renderer.name, renderer, materials[4], "House_01_Wall_02", "StoreB_AltCeiling");
					}
				}
			}
		}

		private static void FixPhone()
		{
			string str = "!!!";
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_tex)
			{
				if (keyValuePair.Key == "MobileProviderText.txt")
				{
					string[] strArray = File.ReadAllLines(keyValuePair.Value + keyValuePair.Key);
					if (strArray.Length != 0)
						str = strArray[0];
				}
			}
			string[] strArray1 = new string[0];
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_tex)
			{
				if (keyValuePair.Key == "MobileButtonsText.txt")
				{
					string[] strArray2 = File.ReadAllLines(keyValuePair.Value + keyValuePair.Key);
					if (strArray2.Length != 0)
						strArray1 = strArray2;
				}
			}
			GameObject gameObject1 = ((IEnumerable<GameObject>)UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault<GameObject>((Func<GameObject, bool>)(obj => obj.name == "UI_PhoneScreen_Grp"));
			if ((UnityEngine.Object)gameObject1 != (UnityEngine.Object)null)
			{
				GameObject inChildren1 = FindInChildren(gameObject1.transform, "PhoneButtonGrp_RestockBoardGame");
				if ((UnityEngine.Object)inChildren1 != (UnityEngine.Object)null)
				{
					UpdateIconSprite(inChildren1, "Icon", "Phone_RestockBoardGame_icon");
					UpdateIconSprite(inChildren1, "Icon2", "Phone_RestockBoardGame_icon_small");
					UpdateIconSprite(inChildren1, "BG", "Phone_RestockBoardGame_back");
					UpdateIconSprite(inChildren1, "BG2", "Phone_RestockBoardGame_back2");
					UpdateIconSprite(inChildren1, "BtnInteraction", "Phone_RestockBoardGame_over");
					if (strArray1.Length == 10)
						UpdateButtonText(inChildren1, "Text", strArray1[0]);
				}
				GameObject inChildren2 = FindInChildren(gameObject1.transform, "PhoneButtonGrp_CustomerReview");
				if ((UnityEngine.Object)inChildren2 != (UnityEngine.Object)null)
				{
					GameObject inChildren3 = FindInChildren(inChildren2.transform, "Icon");
					if ((UnityEngine.Object)inChildren3 != (UnityEngine.Object)null)
						UpdateIconSprite(inChildren3, "Icon", "Phone_CustomerReview_icon");
					UpdateIconSprite(inChildren2, "Icon (1)", "Phone_CustomerReview_icon_right");
					UpdateIconSprite(inChildren2, "Icon (2)", "Phone_CustomerReview_icon_left");
					UpdateIconSprite(inChildren2, "BG", "Phone_CustomerReview_back");
					UpdateIconSprite(inChildren2, "BG2", "Phone_CustomerReview_back2");
					UpdateIconSprite(inChildren2, "BtnInteraction", "Phone_CustomerReview_over");
					if (strArray1.Length == 10)
						UpdateButtonText(inChildren2, "Text", strArray1[1]);
				}
				GameObject inChildren4 = FindInChildren(gameObject1.transform, "PhoneButtonGrp_Restock");
				if ((UnityEngine.Object)inChildren4 != (UnityEngine.Object)null)
				{
					UpdateIconSprite(inChildren4, "Icon", "Phone_Restock_icon");
					UpdateIconSprite(inChildren4, "Icon2", "Phone_Restock_icon_small");
					UpdateIconSprite(inChildren4, "BG", "Phone_Restock_back");
					UpdateIconSprite(inChildren4, "BG2", "Phone_Restock_back2");
					UpdateIconSprite(inChildren4, "BtnInteraction", "Phone_Restock_over");
					if (strArray1.Length == 10)
						UpdateButtonText(inChildren4, "Text", strArray1[2]);
				}
				GameObject inChildren5 = FindInChildren(gameObject1.transform, "PhoneButtonGrp_Furniture");
				if ((UnityEngine.Object)inChildren5 != (UnityEngine.Object)null)
				{
					UpdateIconSprite(inChildren5, "Icon", "Phone_Furniture_icon");
					UpdateIconSprite(inChildren5, "BG", "Phone_Furniture_back");
					UpdateIconSprite(inChildren5, "BG2", "Phone_Furniture_back2");
					UpdateIconSprite(inChildren5, "BtnInteraction", "Phone_Furniture_over");
					if (strArray1.Length == 10)
						UpdateButtonText(inChildren5, "Text", strArray1[3]);
				}
				GameObject inChildren6 = FindInChildren(gameObject1.transform, "PhoneButtonGrp_ExpandShop");
				if ((UnityEngine.Object)inChildren6 != (UnityEngine.Object)null)
				{
					UpdateIconSprite(inChildren6, "Icon", "Phone_ExpandShop_icon");
					UpdateIconSprite(inChildren6, "BG", "Phone_ExpandShop_back");
					UpdateIconSprite(inChildren6, "BG2", "Phone_ExpandShop_back2");
					UpdateIconSprite(inChildren6, "BtnInteraction", "Phone_ExpandShop_over");
					if (strArray1.Length == 10)
						UpdateButtonText(inChildren6, "Text", strArray1[4]);
				}
				GameObject inChildren7 = FindInChildren(gameObject1.transform, "PhoneButtonGrp_Setting");
				if ((UnityEngine.Object)inChildren7 != (UnityEngine.Object)null)
				{
					UpdateIconSprite(inChildren7, "Icon", "Phone_Setting_icon");
					UpdateIconSprite(inChildren7, "BG", "Phone_Setting_back");
					UpdateIconSprite(inChildren7, "BG2", "Phone_Setting_back2");
					UpdateIconSprite(inChildren7, "BtnInteraction", "Phone_Setting_over");
					if (strArray1.Length == 10)
						UpdateButtonText(inChildren7, "Text", strArray1[5]);
				}
				GameObject inChildren8 = FindInChildren(gameObject1.transform, "PhoneButtonGrp_GameEvent");
				if ((UnityEngine.Object)inChildren8 != (UnityEngine.Object)null)
				{
					UpdateIconSprite(inChildren8, "Icon", "Phone_GameEvent_icon");
					UpdateIconSprite(inChildren8, "BG", "Phone_GameEvent_back");
					UpdateIconSprite(inChildren8, "BG2", "Phone_GameEvent_back2");
					UpdateIconSprite(inChildren8, "BtnInteraction", "Phone_GameEvent_over");
					if (strArray1.Length == 10)
						UpdateButtonText(inChildren8, "Text", strArray1[6]);
				}
				GameObject inChildren9 = FindInChildren(gameObject1.transform, "PhoneButtonGrp_PriceCheck");
				if ((UnityEngine.Object)inChildren9 != (UnityEngine.Object)null)
				{
					GameObject inChildren10 = FindInChildren(inChildren9.transform, "Icon");
					if ((UnityEngine.Object)inChildren10 != (UnityEngine.Object)null)
						UpdateIconSprite(inChildren10, "Icon (3)", "Phone_PriceCheck_icon_small");
					UpdateIconSprite(inChildren9, "Icon", "Phone_PriceCheck_icon");
					UpdateIconSprite(inChildren9, "BG", "Phone_PriceCheck_back");
					UpdateIconSprite(inChildren9, "BG2", "Phone_PriceCheck_back2");
					UpdateIconSprite(inChildren9, "BtnInteraction", "Phone_PriceCheck_over");
					if (strArray1.Length == 10)
						UpdateButtonText(inChildren9, "Text", strArray1[7]);
				}
				GameObject inChildren11 = FindInChildren(gameObject1.transform, "PhoneButtonGrp_Hiring");
				if ((UnityEngine.Object)inChildren11 != (UnityEngine.Object)null)
				{
					UpdateIconSprite(inChildren11, "Icon", "Phone_Hiring_icon");
					UpdateIconSprite(inChildren11, "BG", "Phone_Hiring_back");
					UpdateIconSprite(inChildren11, "BG2", "Phone_Hiring_back2");
					UpdateIconSprite(inChildren11, "BtnInteraction", "Phone_Hiring_over");
					if (strArray1.Length == 10)
						UpdateButtonText(inChildren11, "Text", strArray1[8]);
				}
				GameObject inChildren12 = FindInChildren(gameObject1.transform, "PhoneButtonGrp_RentBill");
				if ((UnityEngine.Object)inChildren12 != (UnityEngine.Object)null)
				{
					UpdateIconSprite(inChildren12, "Icon", "Phone_RentBill_icon");
					UpdateIconSprite(inChildren12, "BG", "Phone_RentBill_back");
					UpdateIconSprite(inChildren12, "BG2", "Phone_RentBill_back2");
					UpdateIconSprite(inChildren12, "BtnInteraction", "Phone_RentBill_over");
					if (strArray1.Length == 10)
						UpdateButtonText(inChildren12, "Text", strArray1[9]);
				}
				GameObject inChildren13 = FindInChildren(gameObject1.transform, "TopBarGrp");
				if ((UnityEngine.Object)inChildren13 != (UnityEngine.Object)null)
				{
					UpdateIconSprite(inChildren13, "TopBar", "Phone_TopBar");
					GameObject inChildren14 = FindInChildren(inChildren13.transform, "MobileProviderText");
					if ((UnityEngine.Object)inChildren14 != (UnityEngine.Object)null)
					{
						TextMeshProUGUI component = inChildren14.GetComponent<TextMeshProUGUI>();
						if ((UnityEngine.Object)component != (UnityEngine.Object)null && str != "!!!")
							component.text = str;
					}
				}
			}
			string[] strArray3 = new string[0];
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_tex)
			{
				if (keyValuePair.Key == "MobileUIText.txt")
				{
					string[] strArray4 = File.ReadAllLines(keyValuePair.Value + keyValuePair.Key);
					if (strArray4.Length != 0)
						strArray3 = strArray4;
				}
			}
			if (strArray3.Length != 13)
				return;
			GameObject gameObject2 = ((IEnumerable<GameObject>)UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault<GameObject>((Func<GameObject, bool>)(obj => obj.name == "RestockItemBoardGameScreen_Grp"));
			if ((UnityEngine.Object)gameObject2 != (UnityEngine.Object)null)
			{
				foreach (TextMeshProUGUI textMeshProUgui in TextureReplacer.BepInExPlugin.FindTextMeshProInChildren(gameObject2.transform).ToArray())
				{
					if ((UnityEngine.Object)textMeshProUgui != (UnityEngine.Object)null)
					{
						if (textMeshProUgui.text == "TABLE TOP GEEK")
							textMeshProUgui.text = strArray3[0];
						if (textMeshProUgui.text == "Speedrobo Games")
							textMeshProUgui.text = strArray3[1];
						if (textMeshProUgui.text == "https://speedrobogames.com/")
							textMeshProUgui.text = strArray3[2];
						if (textMeshProUgui.text == "www.tabletopgeek.com/products/list")
							textMeshProUgui.text = strArray3[3];
					}
				}
			}
			GameObject gameObject3 = ((IEnumerable<GameObject>)UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault<GameObject>((Func<GameObject, bool>)(obj => obj.name == "RestockItemScreen_Grp"));
			if ((UnityEngine.Object)gameObject3 != (UnityEngine.Object)null)
			{
				foreach (TextMeshProUGUI textMeshProUgui in TextureReplacer.BepInExPlugin.FindTextMeshProInChildren(gameObject3.transform).ToArray())
				{
					if ((UnityEngine.Object)textMeshProUgui != (UnityEngine.Object)null && textMeshProUgui.text == "www.tetramon-tcg.com/store/products")
						textMeshProUgui.text = strArray3[4];
				}
			}
			GameObject gameObject4 = ((IEnumerable<GameObject>)UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault<GameObject>((Func<GameObject, bool>)(obj => obj.name == "FurnitureShopUIScreen_Grp"));
			if ((UnityEngine.Object)gameObject4 != (UnityEngine.Object)null)
			{
				foreach (TextMeshProUGUI textMeshProUgui in TextureReplacer.BepInExPlugin.FindTextMeshProInChildren(gameObject4.transform).ToArray())
				{
					if ((UnityEngine.Object)textMeshProUgui != (UnityEngine.Object)null)
					{
						if (textMeshProUgui.text == "MY DIY RACKS")
							textMeshProUgui.text = strArray3[5];
						if (textMeshProUgui.text == "We Got Everything")
							textMeshProUgui.text = strArray3[6];
						if (textMeshProUgui.text == "www.mydiyracks.com/shop/product-listing")
							textMeshProUgui.text = strArray3[7];
					}
				}
			}
			GameObject gameObject5 = ((IEnumerable<GameObject>)UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault<GameObject>((Func<GameObject, bool>)(obj => obj.name == "ExpansionShopUIScreen_Grp"));
			if ((UnityEngine.Object)gameObject5 != (UnityEngine.Object)null)
			{
				foreach (TextMeshProUGUI textMeshProUgui in TextureReplacer.BepInExPlugin.FindTextMeshProInChildren(gameObject5.transform).ToArray())
				{
					if ((UnityEngine.Object)textMeshProUgui != (UnityEngine.Object)null)
					{
						if (textMeshProUgui.text == "RENO BIGG")
						{
							textMeshProUgui.text = strArray3[8];
							textMeshProUgui.alignment = TextAlignmentOptions.Left;
						}
						if (textMeshProUgui.text == "BIGG")
						{
							textMeshProUgui.text = strArray3[9];
							textMeshProUgui.alignment = TextAlignmentOptions.Center;
						}
						if (textMeshProUgui.text == "www.renobigg.com/index.php?ws=service_id=1302938")
							textMeshProUgui.text = strArray3[10];
					}
				}
			}
			GameObject gameObject6 = ((IEnumerable<GameObject>)UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault<GameObject>((Func<GameObject, bool>)(obj => obj.name == "CheckPriceScreen_Grp"));
			if ((UnityEngine.Object)gameObject6 != (UnityEngine.Object)null)
			{
				foreach (TextMeshProUGUI textMeshProUgui in TextureReplacer.BepInExPlugin.FindTextMeshProInChildren(gameObject6.transform).ToArray())
				{
					if ((UnityEngine.Object)textMeshProUgui != (UnityEngine.Object)null)
					{
						if (textMeshProUgui.text == "TCG PRICE")
							textMeshProUgui.text = strArray3[11];
						if (textMeshProUgui.text == "www.tcgprice.com/info/pricelist")
							textMeshProUgui.text = strArray3[12];
					}
				}
			}
		}

		private static void UpdateIconSprite(GameObject parent, string childName, string spriteName)
		{
			GameObject inChildren = FindInChildren(parent.transform, childName);
			if (!((UnityEngine.Object)inChildren != (UnityEngine.Object)null))
				return;
			UnityEngine.UI.Image component = inChildren.GetComponent<UnityEngine.UI.Image>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
				UpdateSprite(component, spriteName);
		}

		public static GameObject[] FindChildrenWithNamePrefix(Transform parent, string prefix)
		{
			return ((IEnumerable<Transform>)parent.GetComponentsInChildren<Transform>(true)).Where<Transform>((Func<Transform, bool>)(child => child.name.StartsWith(prefix))).Select<Transform, GameObject>((Func<Transform, GameObject>)(child => child.gameObject)).ToArray<GameObject>();
		}

		private static void changeMenu()
		{
			string[] source1 = new string[5]
			{
				"BGBlack",
				"BGBorder",
				"BGHighlight",
				"BGMidtone",
				"BtnInteraction"
			};
			string[] source2 = new string[8]
			{
				"BGBlack",
				"BGBorder",
				"BGHighlight",
				"BGMidtone",
				"BtnInteraction",
				"BG",
				"Gradient (1)",
				"Text"
			};
			string[] parentNames = new string[10]
			{
				"MainMenuBtn",
				"QuitBtn",
				"ResumeBtn",
				"SaveGameBtn",
				"SettingBtn",
				"LoadGameBtn",
				"NewGameBtn",
				"QuitBtn",
				"BackBtn",
				"FeedbackBtn"
			};
			foreach (CanvasRenderer canvasRenderer in UnityEngine.Resources.FindObjectsOfTypeAll<CanvasRenderer>())
			{
				if ((UnityEngine.Object)canvasRenderer != (UnityEngine.Object)null && GetFirstParent(canvasRenderer.gameObject).name == "Canvas")
				{
					if (((IEnumerable<string>)source1).Contains<string>(canvasRenderer.name) && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && !HasSpecParent(canvasRenderer.gameObject, "GameUIScreen"))
						UpdateIconColor_Parent(canvasRenderer.gameObject, "All_" + canvasRenderer.name);
					if (canvasRenderer.name.Contains("Text") && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText"))
						UpdateTextColor_Parent(canvasRenderer.gameObject, "All_Menu_Text_Color");
					if (((IEnumerable<string>)source1).Contains<string>(canvasRenderer.name) && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && !HasSpecParent(canvasRenderer.gameObject, "GameUIScreen"))
					{
						if (canvasRenderer.name == "BG" && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && GetParent(canvasRenderer.gameObject).name != "ScreenGrp" && GetParent(canvasRenderer.gameObject).name == "AnimGrp")
							UpdateIconColor_Parent(canvasRenderer.gameObject, "All_BG_Color");
						else if (canvasRenderer.name == "BG" && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && GetParent(canvasRenderer.gameObject).name != "ScreenGrp" && GetParent(canvasRenderer.gameObject).name == "Mask")
							UpdateIconColor_Parent(canvasRenderer.gameObject, "All_BG2_Color");
						else if (canvasRenderer.name == "BG" && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && GetParent(canvasRenderer.gameObject).name != "ScreenGrp" && GetParent(canvasRenderer.gameObject).name == "UIGrp")
							UpdateIconColor_Parent(canvasRenderer.gameObject, "All_BG3_Color");
						else if (canvasRenderer.name == "BG" && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && HasSpecParent(canvasRenderer.gameObject, "SettingScreen") && GetParent(canvasRenderer.gameObject).name == "ScreenGrp")
							UpdateIconColor_Parent(canvasRenderer.gameObject, "All_BG4_Color");
						else if (canvasRenderer.name == "BG" && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && HasSpecParent(canvasRenderer.gameObject, "LanguageScreen"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "All_BG5_Color");
					}
					if (((IEnumerable<string>)source2).Contains<string>(canvasRenderer.name) && GetFirstParent(canvasRenderer.gameObject).name == "Canvas")
					{
						if (HasSpecParent(canvasRenderer.gameObject, "MainMenuBtn"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "MainMenuBtn_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "QuitBtn"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "QuitBtn_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "ResumeBtn"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "ResumeBtn_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "SaveGameBtn"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "SaveGameBtn_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "SettingBtn"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "SettingBtn_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "LoadGameBtn"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "LoadGameBtn_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "NewGameBtn"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "NewGameBtn_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "QuitBtn"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "QuitBtn_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "BackBtn"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "BackBtn_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "FeedbackBtn"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "FeedbackBtn_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "ConfirmOverwriteSaveScreen") && HasSpecParent(canvasRenderer.gameObject, "BGBarGrp"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "OverwriteSaveBtnYes_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "ConfirmOverwriteSaveScreen") && HasSpecParent(canvasRenderer.gameObject, "BGBarGrp (1)"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "OverwriteSaveBtnNo_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "LoadGameOverwriteAutoSaveSlotScreen") && HasSpecParent(canvasRenderer.gameObject, "BGBarGrp"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "OverwriteLoadBtnYes_" + canvasRenderer.name);
						else if (HasSpecParent(canvasRenderer.gameObject, "LoadGameOverwriteAutoSaveSlotScreen") && HasSpecParent(canvasRenderer.gameObject, "BGBarGrp (1)"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "OverwriteLoadBtnNo_" + canvasRenderer.name);
						else if (canvasRenderer.name == "BG" && HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && GetParent(canvasRenderer.gameObject).name == "ScaleGrp")
							UpdateIconColor_Parent(canvasRenderer.gameObject, "Roadmap_Frame");
						else if (canvasRenderer.name == "BG" && HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && GetParent(canvasRenderer.gameObject).name == "Mask")
							UpdateIconColor_Parent(canvasRenderer.gameObject, "Roadmap_BG");
						else if (canvasRenderer.name == "Gradient (1)" && HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && GetParent(canvasRenderer.gameObject).name == "Seperator")
							UpdateIconColor_Parent(canvasRenderer.gameObject, "Roadmap_Text_Seperator_Color");
						else if (HasSpecParent(canvasRenderer.gameObject, "SettingScreen"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "SettingScreenBtn_" + canvasRenderer.name);
						if (canvasRenderer.name == "Text" && HasAnyParent(canvasRenderer.gameObject, parentNames))
							UpdateTextColor_Parent(canvasRenderer.gameObject, "Menu_Text_Color");
					}
					if (canvasRenderer.name == "Mask" && HasSpecParent(canvasRenderer.gameObject, "RoadmapText"))
					{
						GameObject[] childrenWithNamePrefix1 = TextureReplacer.BepInExPlugin.FindChildrenWithNamePrefix(canvasRenderer.transform, "Text");
						if (childrenWithNamePrefix1 != null)
						{
							foreach (GameObject parent in childrenWithNamePrefix1)
							{
								if ((UnityEngine.Object)parent != (UnityEngine.Object)null)
									UpdateTextColor_Parent(parent, "Roadmap_Text_Color");
							}
						}
						GameObject[] childrenWithNamePrefix2 = TextureReplacer.BepInExPlugin.FindChildrenWithNamePrefix(canvasRenderer.transform, "Title");
						if (childrenWithNamePrefix2 != null)
						{
							foreach (GameObject parent in childrenWithNamePrefix2)
							{
								if ((UnityEngine.Object)parent != (UnityEngine.Object)null)
									UpdateTextColor_Parent(parent, "Roadmap_Text_Color");
							}
						}
					}
					if (HasSpecParent(canvasRenderer.gameObject, "PauseScreen") || HasSpecParent(canvasRenderer.gameObject, "SaveLoadGameSlotSelectScreen") || HasSpecParent(canvasRenderer.gameObject, "SettingScreen") || HasSpecParent(canvasRenderer.gameObject, "TitleScreen") || HasSpecParent(canvasRenderer.gameObject, "ConfirmOverwriteSaveScreen") ||
						HasSpecParent(canvasRenderer.gameObject, "ControllerSelectorUIGrp"))
					{
						if (canvasRenderer.name == "BG" && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && GetParent(canvasRenderer.gameObject).name != "ScreenGrp" && GetParent(canvasRenderer.gameObject).name == "AnimGrp")
							UpdateIconColor_Parent(canvasRenderer.gameObject, "BG_Color");
						else if (canvasRenderer.name == "BG" && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && GetParent(canvasRenderer.gameObject).name != "ScreenGrp" && GetParent(canvasRenderer.gameObject).name == "Mask")
							UpdateIconColor_Parent(canvasRenderer.gameObject, "BG2_Color");
						else if (canvasRenderer.name == "BG" && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && GetParent(canvasRenderer.gameObject).name != "ScreenGrp" && GetParent(canvasRenderer.gameObject).name == "UIGrp")
							UpdateIconColor_Parent(canvasRenderer.gameObject, "BG3_Color");
						else if (canvasRenderer.name == "BG" && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && HasSpecParent(canvasRenderer.gameObject, "SettingScreen") && GetParent(canvasRenderer.gameObject).name == "ScreenGrp")
							UpdateIconColor_Parent(canvasRenderer.gameObject, "BG4_Color");
						else if (canvasRenderer.name == "BG" && !HasSpecParent(canvasRenderer.gameObject, "RoadmapText") && HasSpecParent(canvasRenderer.gameObject, "LanguageScreen"))
							UpdateIconColor_Parent(canvasRenderer.gameObject, "BG5_Color");
					}
				}
			}
		}

		private static void UpdateIconColor_Parent(GameObject parent, string fileName)
		{
			if (!((UnityEngine.Object)parent != (UnityEngine.Object)null))
				return;
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_tex)
			{
				if (keyValuePair.Key == fileName + ".txt")
				{
					string[] strArray = File.ReadAllLines(keyValuePair.Value + keyValuePair.Key);
					UnityEngine.Color color;
					if (strArray.Length != 0 && ColorUtility.TryParseHtmlString("#" + strArray[0], out color))
					{
						UnityEngine.UI.Image component = parent.GetComponent<UnityEngine.UI.Image>();
						if ((UnityEngine.Object)component != (UnityEngine.Object)null)
							component.color = color;
						break;
					}
					break;
				}
			}
		}

		private static void UpdateTextColor_Parent(GameObject parent, string fileName)
		{
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_tex)
			{
				if (keyValuePair.Key == fileName + ".txt")
				{
					string[] strArray = File.ReadAllLines(keyValuePair.Value + keyValuePair.Key);
					if (strArray.Length == 2)
					{
						TextMeshProUGUI component = parent.GetComponent<TextMeshProUGUI>();
						if ((UnityEngine.Object)component != (UnityEngine.Object)null)
						{
							UnityEngine.Color color1;
							if (ColorUtility.TryParseHtmlString("#" + strArray[0], out color1))
								component.color = color1;
							UnityEngine.Color color2;
							if (ColorUtility.TryParseHtmlString("#" + strArray[1], out color2))
								component.outlineColor = (Color32)color2;
						}
					}
				}
			}
		}

		private static void UpdateSprite(UnityEngine.UI.Image image, string spritename)
		{
			Texture2D cachedTexture = GetCachedTexture(spritename);
			if (!((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null))
				return;
			Sprite sprite = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
			sprite.name = image.sprite.name;
			image.sprite = sprite;
			image.color = UnityEngine.Color.white;
		}

		private static void UpdateButtonText(GameObject parent, string childName, string text)
		{
			GameObject inChildren = FindInChildren(parent.transform, childName);
			if (!((UnityEngine.Object)inChildren != (UnityEngine.Object)null))
				return;
			TextMeshProUGUI component = inChildren.GetComponent<TextMeshProUGUI>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
				component.text = text;
		}

		private static GameObject FindInChildren(Transform parent, string childName)
		{
			foreach (Transform parent1 in parent)
			{
				if (parent1.name == childName)
					return parent1.gameObject;
				GameObject inChildren = FindInChildren(parent1, childName);
				if ((UnityEngine.Object)inChildren != (UnityEngine.Object)null)
					return inChildren;
			}
			return (GameObject)null;
		}

		public static List<GameObject> FindAllInChildren(Transform parent, string name)
		{
			List<GameObject> allInChildren = new List<GameObject>();
			foreach (Transform parent1 in parent)
			{
				if (parent1.name == name)
					allInChildren.Add(parent1.gameObject);
				allInChildren.AddRange((IEnumerable<GameObject>)TextureReplacer.BepInExPlugin.FindAllInChildren(parent1, name));
			}
			return allInChildren;
		}

		public static List<TextMeshProUGUI> FindTextMeshProInChildren(Transform parent)
		{
			List<TextMeshProUGUI> meshProInChildren = new List<TextMeshProUGUI>();
			foreach (Transform parent1 in parent)
			{
				TextMeshProUGUI component = parent1.GetComponent<TextMeshProUGUI>();
				if ((UnityEngine.Object)component != (UnityEngine.Object)null)
					meshProInChildren.Add(component);
				meshProInChildren.AddRange((IEnumerable<TextMeshProUGUI>)TextureReplacer.BepInExPlugin.FindTextMeshProInChildren(parent1));
			}
			return meshProInChildren;
		}

		private static GameObject GetFirstParent(GameObject obj)
		{
			Transform parent = obj.transform.parent;
			while ((UnityEngine.Object)parent != (UnityEngine.Object)null && (UnityEngine.Object)parent.parent != (UnityEngine.Object)null)
				parent = parent.parent;
			return (UnityEngine.Object)parent != (UnityEngine.Object)null ? parent.gameObject : (GameObject)null;
		}

		private static GameObject GetParent(GameObject obj)
		{
			Transform parent = obj.transform.parent;
			return (UnityEngine.Object)parent != (UnityEngine.Object)null ? parent.gameObject : (GameObject)null;
		}

		private static GameObject GetGrandParent(GameObject obj)
		{
			Transform parent = obj.transform.parent?.parent;
			return (UnityEngine.Object)parent != (UnityEngine.Object)null ? parent.gameObject : (GameObject)null;
		}

		private static GameObject GetSpecParent(GameObject obj, string parentName)
		{
			for (Transform parent = obj.transform.parent; (UnityEngine.Object)parent != (UnityEngine.Object)null; parent = parent.parent)
			{
				if (parent.name == parentName)
					return parent.gameObject;
			}
			return (GameObject)null;
		}

		private static bool HasSpecParent(GameObject obj, string parentName)
		{
			for (Transform parent = obj.transform.parent; (UnityEngine.Object)parent != (UnityEngine.Object)null; parent = parent.parent)
			{
				if (parent.name == parentName)
					return true;
			}
			return false;
		}

		private static bool HasAnyParent(GameObject obj, string[] parentNames)
		{
			for (Transform currentTransform = obj.transform.parent; (UnityEngine.Object)currentTransform != (UnityEngine.Object)null; currentTransform = currentTransform.parent)
			{
				if (Array.Exists<string>(parentNames, (Predicate<string>)(name => name == currentTransform.name)))
					return true;
			}
			return false;
		}

		private static void UpdateMaterialGlass(Material[] mats, string materialName, string textureKey)
		{
			if (mats == null)
				return;
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_tex)
			{
				if (keyValuePair.Key == textureKey)
				{
					string[] strArray = File.ReadAllLines(keyValuePair.Value + keyValuePair.Key);
					UnityEngine.Color color;
					float result;
					if (strArray.Length == 2 && ColorUtility.TryParseHtmlString("#" + strArray[0], out color) && float.TryParse(strArray[1], NumberStyles.Float, (IFormatProvider)CultureInfo.InvariantCulture, out result))
					{
						color.a = Mathf.Clamp01(result);
						for (int index = 0; index < mats.Length; ++index)
						{
							Material mat = mats[index];
							if ((UnityEngine.Object)mat != (UnityEngine.Object)null && (mat.name == materialName || mat.name == materialName + " (Instance)"))
								mat.SetColor("_Color", color);
						}
					}
				}
			}
		}

		private static void UpdateMaterial(
			string objname,
			Renderer renderer,
			Material[] mats,
			string materialName,
			string textureKey)
		{
			if (mats == null)
				return;
			bool flag = false;
			for (int index = 0; index < mats.Length; ++index)
			{
				Material mat = mats[index];
				if ((UnityEngine.Object)mat != (UnityEngine.Object)null && (mat.name == materialName + " (Instance)" || mat.name == materialName))
				{
					if (objname == "Weapons closet")
					{
						if (index == 0 && mat.name == "Weapons closet (Instance)")
							textureKey = "CardTable_bottom";
						else if (index == 2 && mat.name == "Weapons closet (Instance)")
							textureKey = "CardTable_top";
					}
					Material material = new Material(mat);
					material.name = materialName;
					string str = "";
					Texture2D texture2D1;
					if (textureKey == "PlayTable_chair" || textureKey == "PlayTable_chair_metal" || textureKey == "PlayTable_metal" || textureKey == "PlayTable_wood")
					{
						if (playtablename == "")
						{
							texture2D1 = GetRandomTexture(textureKey);
							str = playtablename;
						}
						else
						{
							texture2D1 = GetCachedTexture(playtablename.Replace("PlayTable_chair", textureKey).Replace("PlayTable_chair_metal", textureKey).Replace("PlayTable_metal", textureKey).Replace("PlayTable_wood", textureKey));
							if ((UnityEngine.Object)texture2D1 == (UnityEngine.Object)null)
								texture2D1 = GetRandomTexture(textureKey);
							else
								str = playtablename.Replace("PlayTable_chair", textureKey).Replace("PlayTable_chair_metal", textureKey).Replace("PlayTable_metal", textureKey).Replace("PlayTable_wood", textureKey);
						}
					}
					else
						texture2D1 = GetCachedTexture(textureKey);
					if ((UnityEngine.Object)texture2D1 != (UnityEngine.Object)null)
					{
						try
						{
							material.mainTexture = (Texture)texture2D1;
							material.mainTexture.name = "_MainTex";
							material.SetVector("_Color", new Vector4(1f, 1f, 1f, 1f));
							material.color = UnityEngine.Color.white;
							mats[index] = material;
							Texture2D texture2D2 = (Texture2D)null;
							Texture2D cachedTexture;
							if (str != "")
							{
								cachedTexture = GetCachedTexture(str + "_m");
								if ((UnityEngine.Object)cachedTexture == (UnityEngine.Object)null)
									cachedTexture = GetCachedTexture(textureKey + "_m");
							}
							else
								cachedTexture = GetCachedTexture(textureKey + "_m");
							if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null && mats[index].HasProperty("_MetallicGlossMap"))
							{
								mats[index].EnableKeyword("_METALLICGLOSSMAP");
								mats[index].SetTexture("_MetallicGlossMap", (Texture)cachedTexture);
							}
							Texture2D texture2D3 = (Texture2D)null;
							if (str != "")
							{
								texture2D3 = GetCachedTexture(str + "_n");
								if ((UnityEngine.Object)texture2D3 == (UnityEngine.Object)null)
									texture2D3 = GetCachedTexture(textureKey + "_n");
							}
							else
								texture2D2 = GetCachedTexture(textureKey + "_n");
							if ((UnityEngine.Object)texture2D3 != (UnityEngine.Object)null && mats[index].HasProperty("_BumpMap"))
							{
								mats[index].EnableKeyword("_NORMALMAP");
								mats[index].SetTexture("_BumpMap", (Texture)texture2D3);
								mats[index].SetInt("_UseNormalMap", 1);
							}
							Texture2D texture2D4 = (Texture2D)null;
							if (str != "")
							{
								texture2D4 = GetCachedTexture(str + "_o");
								if ((UnityEngine.Object)texture2D4 == (UnityEngine.Object)null)
									texture2D4 = GetCachedTexture(textureKey + "_o");
							}
							else
								texture2D2 = GetCachedTexture(textureKey + "_o");
							if ((UnityEngine.Object)texture2D4 != (UnityEngine.Object)null && mats[index].HasProperty("_OcclusionMap"))
								mats[index].SetTexture("_OcclusionMap", (Texture)texture2D4);
							flag = true;
						}
						catch
						{
						}
					}
				}
			}
			if (flag)
				renderer.materials = mats;
		}

		private static void UpdateMaterialBuildings(
			string objname,
			Renderer renderer,
			Material[] mats,
			string materialName,
			string textureKey)
		{
			if (mats == null)
				return;
			bool flag = false;
			for (int index = 0; index < mats.Length; ++index)
			{
				Material mat = mats[index];
				if ((UnityEngine.Object)mat != (UnityEngine.Object)null && (mat.name == materialName + " (Instance)" || mat.name == materialName))
				{
					if (objname == "Weapons closet")
					{
						if (index == 0 && mat.name == "Weapons closet (Instance)")
							textureKey = "CardTable_bottom";
						else if (index == 2 && mat.name == "Weapons closet (Instance)")
							textureKey = "CardTable_top";
					}
					Material material = new Material(Shader.Find("Standard"));
					Texture2D cachedTexture1 = GetCachedTexture(textureKey);
					if ((UnityEngine.Object)cachedTexture1 != (UnityEngine.Object)null)
					{
						try
						{
							material.name = mat.name;
							material.mainTexture = (Texture)cachedTexture1;
							material.color = UnityEngine.Color.white;
							mats[index] = material;
							if (materialName == "Glass_01" || materialName.StartsWith("Ceiling"))
							{
								material.SetFloat("_Mode", 2f);
								material.SetOverrideTag("RenderType", "Transparent");
								material.SetInt("_SrcBlend", 5);
								material.SetInt("_DstBlend", 10);
								material.SetInt("_ZWrite", 1);
								material.DisableKeyword("_ALPHATEST_ON");
								material.EnableKeyword("_ALPHABLEND_ON");
								material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
								material.renderQueue = 3000;
							}
							Texture2D cachedTexture2 = GetCachedTexture(textureKey + "_m");
							if ((UnityEngine.Object)cachedTexture2 != (UnityEngine.Object)null && material.HasProperty("_MetallicGlossMap"))
							{
								material.EnableKeyword("_METALLICGLOSSMAP");
								material.SetTexture("_MetallicGlossMap", (Texture)cachedTexture2);
							}
							Texture2D cachedTexture3 = GetCachedTexture(textureKey + "_n");
							if ((UnityEngine.Object)cachedTexture3 != (UnityEngine.Object)null && material.HasProperty("_BumpMap"))
							{
								material.EnableKeyword("_NORMALMAP");
								material.SetTexture("_BumpMap", (Texture)cachedTexture3);
								material.SetInt("_UseNormalMap", 1);
							}
							Texture2D cachedTexture4 = GetCachedTexture(textureKey + "_o");
							if ((UnityEngine.Object)cachedTexture4 != (UnityEngine.Object)null && material.HasProperty("_OcclusionMap"))
								material.SetTexture("_OcclusionMap", (Texture)cachedTexture4);
							flag = true;
						}
						catch
						{
						}
					}
				}
			}
			if (flag)
				renderer.materials = mats;
		}

		private static void UpdateMaterialOnce(
			string objname,
			Renderer renderer,
			Material mat,
			string materialName,
			string textureKey)
		{
			if ((UnityEngine.Object)renderer == (UnityEngine.Object)null || (UnityEngine.Object)mat == (UnityEngine.Object)null || !(mat.name == materialName) && !(mat.name == materialName + " (Instance)"))
				return;
			Texture2D cachedTexture1 = GetCachedTexture(textureKey);
			if ((UnityEngine.Object)cachedTexture1 != (UnityEngine.Object)null)
			{
				try
				{
					Material material1 = new Material(Shader.Find("Standard"));
					material1.name = mat.name;
					material1.mainTexture = (Texture)cachedTexture1;
					material1.color = UnityEngine.Color.white;
					Material material2 = material1;
					material2.mainTexture.name = "_MainTex";
					material2.SetColor("_Color", UnityEngine.Color.white);
					Texture2D texture2D1 = GetCachedTexture(textureKey + "_m") ?? GetCachedTexture(textureKey + " M");
					if ((UnityEngine.Object)texture2D1 != (UnityEngine.Object)null && material2.HasProperty("_MetallicGlossMap"))
					{
						material2.EnableKeyword("_METALLICGLOSSMAP");
						material2.SetTexture("_MetallicGlossMap", (Texture)texture2D1);
					}
					Texture2D texture2D2 = GetCachedTexture(textureKey + "_n") ?? GetCachedTexture(textureKey + " N");
					if ((UnityEngine.Object)texture2D2 != (UnityEngine.Object)null && material2.HasProperty("_BumpMap"))
					{
						material2.EnableKeyword("_NORMALMAP");
						material2.SetTexture("_BumpMap", (Texture)texture2D2);
						material2.SetInt("_UseNormalMap", 1);
					}
					Texture2D cachedTexture2 = GetCachedTexture(textureKey + "_o");
					if ((UnityEngine.Object)cachedTexture2 != (UnityEngine.Object)null && material2.HasProperty("_OcclusionMap"))
						material2.SetTexture("_OcclusionMap", (Texture)cachedTexture2);
					if (materialName == "Glass_01" || materialName.StartsWith("Ceiling") || materialName == "House_07_Windows" || materialName == "House_01_Wall_02")
					{
						material2.SetFloat("_Mode", 2f);
						material2.SetOverrideTag("RenderType", "Transparent");
						material2.SetInt("_SrcBlend", 5);
						material2.SetInt("_DstBlend", 10);
						material2.SetInt("_ZWrite", 1);
						material2.DisableKeyword("_ALPHATEST_ON");
						material2.EnableKeyword("_ALPHABLEND_ON");
						material2.DisableKeyword("_ALPHAPREMULTIPLY_ON");
						material2.renderQueue = 3000;
					}
					Material[] materials = renderer.materials;
					for (int index = 0; index < materials.Length; ++index)
					{
						if (materials[index].name == materialName || materials[index].name == materialName + " (Instance)")
						{
							materials[index] = material2;
							renderer.materials = materials;
							break;
						}
					}
				}
				catch
				{
				}
			}
		}

		private static void ReplaceSpriteLists()
		{
			if (!(SceneManager.GetActiveScene().name == "Start"))
				return;
			foreach (List<Sprite> spriteList in new List<List<Sprite>>()
				{
					(List<Sprite>)CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_CardBorderList,
					(List<Sprite>)CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_CardBGList,
					(List<Sprite>)CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_CardFrontImageList,
					(List<Sprite>)CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_CardBackImageList,
					(List<Sprite>)CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_CardFoilMaskImageList,
					(List<Sprite>)CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_TetramonImageList
				})
				ReplaceSpritesInList(spriteList);
			foreach (List<MonsterData> spriteList in new List<List<MonsterData>>()
				{
					(List<MonsterData>)CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_DataList,
					(List<MonsterData>)CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_MegabotDataList,
					(List<MonsterData>)CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_FantasyRPGDataList,
					(List<MonsterData>)CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_CatJobDataList,
					(List<MonsterData>)CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_SpecialCardImageList
				})
				ReplaceMonsterDataInList(spriteList);
			foreach (List<CollectionPackImageSprite> spriteList in new List<List<CollectionPackImageSprite>>()
				{
					(List<CollectionPackImageSprite>)CSingleton<InventoryBase>.Instance.m_StockItemData_SO.m_CollectionPackImageSpriteList
				})
				ReplaceStockDataInList(spriteList);
			foreach (List<ItemData> spriteList in new List<List<ItemData>>()
				{
					(List<ItemData>)CSingleton<InventoryBase>.Instance.m_StockItemData_SO.m_ItemDataList
				})
				ReplaceItemDataInList(spriteList);
			foreach (List<ItemMeshData> spriteList in new List<List<ItemMeshData>>()
				{
					(List<ItemMeshData>)CSingleton<InventoryBase>.Instance.m_StockItemData_SO.m_ItemMeshDataList
				})
				ReplaceItemDataMeshInList(spriteList);
			ReplaceReStockImagesInList((List<FurniturePurchaseData>)CSingleton<InventoryBase>.Instance.m_ObjectData_SO.m_FurniturePurchaseDataList);
			if (SceneManager.GetActiveScene().name == "Start")
			{
				foreach (UI_PriceTag priceTag in new List<UI_PriceTag>()
					{
						CSingleton<PriceTagUISpawner>.Instance.m_UIPriceTagCardPrefab,
						CSingleton<PriceTagUISpawner>.Instance.m_UIPriceTagItemBoxPrefab,
						CSingleton<PriceTagUISpawner>.Instance.m_UIPriceTagPackageBoxPrefab,
						CSingleton<PriceTagUISpawner>.Instance.m_UIPriceTagPrefab,
						CSingleton<PriceTagUISpawner>.Instance.m_UIPriceTagWarehouseRackPrefab
					})
					ReplaceSpritesInPriceList(priceTag);
				ReplacePriceTagBackWithDelay();
			}
			ReplaceCardExpansionNameList((List<string>)CSingleton<InventoryBase>.Instance.m_TextSO.m_CardExpansionNameList);
		}

		private static IEnumerator ReplacePriceTagBackWithDelay()
		{
			while (((List<UnityEngine.Color>)CSingleton<LightManager>.Instance.m_ItemMatOriginalColorList).Count == 0)
				yield return (object)new WaitForSeconds(1f);
			foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_tex)
			{
				KeyValuePair<string, string> filePath = keyValuePair;
				if (filePath.Key == "PriceTag_back_color.txt")
				{
					string[] lines = File.ReadAllLines(filePath.Value + filePath.Key);
					UnityEngine.Color color;
					if (lines.Length != 0 && ColorUtility.TryParseHtmlString("#" + lines[0], out color))
					{
						if (((List<UnityEngine.Color>)CSingleton<LightManager>.Instance.m_ItemMatOriginalColorList).Count > 4)
							((List<UnityEngine.Color>)CSingleton<LightManager>.Instance.m_ItemMatOriginalColorList)[4] = color;
						if (((List<Material>)CSingleton<LightManager>.Instance.m_ItemMatList).Count > 4 && (UnityEngine.Object)((List<Material>)CSingleton<LightManager>.Instance.m_ItemMatList)[4] != (UnityEngine.Object)null)
							((List<Material>)CSingleton<LightManager>.Instance.m_ItemMatList)[4].color = color;
						break;
					}
					break;
				}
				filePath = new KeyValuePair<string, string>();
			}
		}

		private static void ReplaceSpritesInList(List<Sprite> spriteList)
		{
			for (int index = 0; index < spriteList.Count; ++index)
			{
				Sprite sprite1 = spriteList[index];
				if ((UnityEngine.Object)sprite1 != (UnityEngine.Object)null)
				{
					if (sprite1.name == "CardFoilMask")
					{
						if ((TextureReplacer.BepInExPlugin.ShowFull.Value || TextureReplacer.BepInExPlugin.ShowFullFrame.Value) && (UnityEngine.Object)TextureReplacer.BepInExPlugin.CardFoilMaskC != (UnityEngine.Object)null)
						{
							Sprite sprite2 = TextureReplacer.BepInExPlugin.TextureToSprite(TextureReplacer.BepInExPlugin.CardFoilMaskC);
							sprite2.name = sprite1.name;
							spriteList[index] = sprite2;
						}
					}
					else
					{
						Texture2D cachedTexture = GetCachedTexture(sprite1.name);
						if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
						{
							Sprite sprite3 = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
							sprite3.name = sprite1.name;
							spriteList[index] = sprite3;
						}
					}
				}
			}
		}

		private static void ReplaceMonsterDataInList(List<MonsterData> spriteList)
		{
			for (int index = 0; index < spriteList.Count; ++index)
			{
				Sprite icon = spriteList[index].Icon;
				Sprite ghostIcon = spriteList[index].GhostIcon;
				string name = spriteList[index].Name;
				EElementIndex elementIndex = spriteList[index].ElementIndex;
				if (name != "" && elementIndex.ToString() != "")
				{
					string str1;
					if (TextureReplacer.BepInExPlugin.cachedData.TryGetValue(name + "_" + elementIndex.ToString() + "_NAME.txt", out str1))
						spriteList[index].Name = str1;
					string str2;
					if (TextureReplacer.BepInExPlugin.cachedData.TryGetValue(name + "_" + elementIndex.ToString() + "_DESC.txt", out str2))
						spriteList[index].Description = str2;
					string str3;
					if (TextureReplacer.BepInExPlugin.cachedData.TryGetValue(name + "_" + elementIndex.ToString() + "_ARTIST.txt", out str3))
						spriteList[index].ArtistName = str3;
				}
				if ((UnityEngine.Object)icon != (UnityEngine.Object)null)
				{
					Texture2D cachedTexture = GetCachedTexture(icon.name);
					if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
					{
						Sprite sprite = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
						sprite.name = icon.name;
						spriteList[index].Icon = sprite;
					}
				}
				if ((UnityEngine.Object)ghostIcon != (UnityEngine.Object)null)
				{
					Texture2D cachedTexture = GetCachedTexture(ghostIcon.name);
					if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
					{
						Sprite sprite = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
						sprite.name = ghostIcon.name;
						spriteList[index].GhostIcon = sprite;
					}
				}
			}
		}

		private static void ReplaceStockDataInList(List<CollectionPackImageSprite> spriteList)
		{
			for (int index = 0; index < spriteList.Count; ++index)
			{
				CollectionPackImageSprite sprite1 = spriteList[index];
				Sprite fullSprite = sprite1.fullSprite;
				Sprite bottomSprite = sprite1.bottomSprite;
				Sprite topSprite = sprite1.topSprite;
				if ((UnityEngine.Object)fullSprite != (UnityEngine.Object)null)
				{
					Texture2D cachedTexture = GetCachedTexture(fullSprite.name);
					if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
					{
						Sprite sprite2 = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
						sprite2.name = fullSprite.name;
						sprite1.fullSprite = sprite2;
					}
				}
				if ((UnityEngine.Object)bottomSprite != (UnityEngine.Object)null)
				{
					Texture2D cachedTexture = GetCachedTexture(bottomSprite.name);
					if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
					{
						Sprite sprite3 = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
						sprite3.name = bottomSprite.name;
						sprite1.bottomSprite = sprite3;
					}
				}
				if ((UnityEngine.Object)topSprite != (UnityEngine.Object)null)
				{
					Texture2D cachedTexture = GetCachedTexture(topSprite.name);
					if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
					{
						Sprite sprite4 = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
						sprite4.name = topSprite.name;
						sprite1.topSprite = sprite4;
					}
				}
			}
		}
		
		
		[HarmonyPatch("ReplaceItemDataInList", typeof(TextureReplacer.BepInExPlugin)), HarmonyPrefix]
		private static bool ReplaceItemDataInList(List<ItemData> spriteList)
		{
			for (int index = 0; index < spriteList.Count; ++index)
			{
				Sprite icon = spriteList[index].icon;
				if ((UnityEngine.Object)icon != (UnityEngine.Object)null)
				{
					if (spriteList[index].name != "")
					{
						string str1 = SanitizeFileName(spriteList[index].name);
						string str2 = spriteList[index].icon.name.Replace("Icon_Playmat", "");
						string searchPattern1 = (string)null;
						string path1 = (string)null;
						if (str2 != null && str1 != null && str1.StartsWith("Playmat"))
							searchPattern1 = str1.Replace("Playmat ", "Playmat_" + str2 + "_") + "_NAME.txt";
						string searchPattern2 = str1 + "_NAME.txt";
						if (searchPattern1 != null)
							path1 = Directory.EnumerateFiles(path_nam, searchPattern1, SearchOption.AllDirectories).Where<string>((Func<string, bool>)(f => !f.Contains(Path.Combine(path_nam, "cards")))).FirstOrDefault<string>();
						if (path1 != null)
						{
							try
							{
								string[] strArray = File.ReadAllLines(path1);
								spriteList[index].name = strArray[0];
							}
							catch
							{
							}
						}
						else if (searchPattern2 != null)
						{
							string path2 = Directory.EnumerateFiles(path_nam, searchPattern2, SearchOption.AllDirectories).Where<string>((Func<string, bool>)(f => !f.Contains(Path.Combine(path_nam, "cards")))).FirstOrDefault<string>();
							if (path2 != null)
							{
								try
								{
									string[] strArray = File.ReadAllLines(path2);
									spriteList[index].name = strArray[0];
								}
								catch
								{
								}
							}
						}
					}
					Texture2D cachedTexture = GetCachedTexture(icon.name);
					if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
					{
						Sprite sprite = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
						sprite.name = icon.name;
						spriteList[index].icon = sprite;
					}
				}
			}
			return false;
		}

		private static string SanitizeFileName(string fileName)
		{
			foreach (char invalidFileNameChar in Path.GetInvalidFileNameChars())
				fileName = fileName.Replace(invalidFileNameChar.ToString(), "_");
			return fileName;
		}

		private static void ReplaceItemDataMeshInList(List<ItemMeshData> spriteList)
		{
			for (int index = 0; index < spriteList.Count; ++index)
			{
				ReplaceMesh(spriteList[index], true);
				ReplaceMesh(spriteList[index], false);
			}
		}

		private static void ReplaceMesh(ItemMeshData itemData, bool isPrimary)
		{
			Mesh mesh1 = isPrimary ? itemData.mesh : itemData.meshSecondary;
			if ((UnityEngine.Object)mesh1 == (UnityEngine.Object)null || (UnityEngine.Object)itemData.material == (UnityEngine.Object)null)
				return;
			string name = itemData.material.mainTexture.name;
			Mesh mesh2 = !name.StartsWith("T_Manga_")
				? (!(name == "T_D20Dice") || !(mesh1.name == "DiceBox")
					? (!(name == "T_D20Dice") || !(mesh1.name == "DiceBoxGlass")
						? (!name.StartsWith("T_D20Dice") || !(mesh1.name == "DiceBox")
							? (!name.StartsWith("T_D20Dice") || !(mesh1.name == "DiceBoxGlass") ? GetCachedMesh(mesh1.name) : GetCachedMesh(name.Replace(" ", "").Replace("T_D20Dice", "DiceBoxGlass")) ?? GetCachedMesh(mesh1.name))
							: GetCachedMesh(name.Replace(" ", "").Replace("T_D20Dice", "DiceBox")) ?? GetCachedMesh(mesh1.name))
						: GetCachedMesh((name + "1").Replace("T_D20Dice", "DiceBoxGlass")) ?? GetCachedMesh(mesh1.name))
					: GetCachedMesh((name + "1").Replace("T_D20Dice", "DiceBox")) ?? GetCachedMesh(mesh1.name))
				: GetCachedMesh(name.Replace("T_Manga_", "Manga_Mesh")) ?? GetCachedMesh(mesh1.name);
			if (!((UnityEngine.Object)mesh2 != (UnityEngine.Object)null))
				return;
			if (isPrimary)
				itemData.mesh = mesh2;
			else
				itemData.meshSecondary = mesh2;
			mesh2.UploadMeshData(true);
		}

		private static void ReplaceReStockImagesInList(List<FurniturePurchaseData> items)
		{
			for (int index = 0; index < items.Count; ++index)
			{
				Sprite icon = items[index].icon;
				if ((UnityEngine.Object)icon != (UnityEngine.Object)null)
				{
					Texture2D cachedTexture = GetCachedTexture(icon.name);
					if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
					{
						Sprite sprite = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
						sprite.name = icon.name;
						items[index].icon = sprite;
					}
				}
			}
		}
		
		[HarmonyPatch("ReplaceCardExpansionNameList", typeof(TextureReplacer.BepInExPlugin)), HarmonyPrefix]
		private static bool ReplaceCardExpansionNameList(List<string> items)
		{
			string searchPattern = "Expansions_Names.txt";
			string path = Directory.EnumerateFiles(path_nam, searchPattern, SearchOption.AllDirectories).FirstOrDefault<string>();
			try
			{
				string[] strArray = File.ReadAllLines(path);
				for (int index = 0; index < Math.Min(items.Count, strArray.Length); ++index)
					items[index] = strArray[index];
				LanguageSourceData source = ((List<LanguageSourceData>)LocalizationManager.Sources)[0];
				foreach (KeyValuePair<string, int> keyValuePair in new Dictionary<string, int>()
					{
						{
							"Tetramon Base",
							0
						},
						{
							"Tetramon Destiny",
							1
						},
						{
							"Tetramon Ghost",
							2
						},
						{
							"Tetramon",
							3
						},
						{
							"Ghost",
							4
						},
						{
							"Destiny",
							5
						}
					})
				{
					TermData termData;
					if (((Dictionary<string, TermData>)source.mDictionary).TryGetValue(keyValuePair.Key, out termData))
					{
						string str = strArray[keyValuePair.Value];
						for (int index = 0; index < termData.Languages.Length; ++index)
							termData.Languages[index] = str;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		private static void ReplaceSpritesCardsInList(List<Sprite> spriteList)
		{
			for (int index = 0; index < spriteList.Count; ++index)
			{
				Sprite sprite1 = spriteList[index];
				if ((UnityEngine.Object)sprite1 != (UnityEngine.Object)null)
				{
					Texture2D cachedTexture = GetCachedTexture(sprite1.name);
					if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
					{
						Sprite sprite2 = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
						sprite2.name = sprite1.name;
						spriteList[index] = sprite2;
					}
				}
			}
		}

		private static void ReplaceSpritesFoilsInList(List<UnityEngine.UI.Image> spriteList)
		{
			for (int index = 0; index < spriteList.Count; ++index)
			{
				UnityEngine.UI.Image sprite1 = spriteList[index];
				if ((UnityEngine.Object)sprite1 != (UnityEngine.Object)null && (UnityEngine.Object)sprite1.sprite != (UnityEngine.Object)null)
				{
					Texture2D cachedTexture = GetCachedTexture(sprite1.sprite.name);
					if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
					{
						Sprite sprite2 = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
						sprite2.name = sprite1.sprite.name;
						sprite1.sprite = sprite2;
					}
				}
			}
		}

		private static void ReplaceSpritesInPriceList(UI_PriceTag priceTag)
		{
			if ((UnityEngine.Object)priceTag.m_UIGrp == (UnityEngine.Object)null)
				return;
			ApplyColorToChild("BG", "PriceTag_front_color.txt");
			ApplyColorToChild("IconBG", "PriceTag_backIcon_color.txt");
			ApplyColorToChild("PriceBG", "PriceTag_backPrice_color.txt");
			ApplyColorToChildText("PriceText", "PriceTag_text_color.txt");
			ApplyColorToChildText("AmountText", "PriceTag_textAmount_color.txt");

			void ApplyColorToChild(string childName, string fileNameKey)
			{
				Transform transform = priceTag.m_UIGrp.Find(childName);
				if ((UnityEngine.Object)transform == (UnityEngine.Object)null)
					return;
				UnityEngine.UI.Image component = transform.GetComponent<UnityEngine.UI.Image>();
				if ((UnityEngine.Object)component == (UnityEngine.Object)null)
					return;
				foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_tex)
				{
					if (!(keyValuePair.Key != fileNameKey))
					{
						string[] strArray = File.ReadAllLines(keyValuePair.Value + keyValuePair.Key);
						UnityEngine.Color color;
						if (strArray.Length != 0 && ColorUtility.TryParseHtmlString("#" + strArray[0], out color))
							component.color = color;
					}
				}
			}

			void ApplyColorToChildText(string childName, string fileNameKey)
			{
				Transform transform = priceTag.m_UIGrp.Find(childName);
				if ((UnityEngine.Object)transform == (UnityEngine.Object)null)
					return;
				TextMeshProUGUI component = transform.GetComponent<TextMeshProUGUI>();
				if ((UnityEngine.Object)component == (UnityEngine.Object)null)
					return;
				foreach (KeyValuePair<string, string> keyValuePair in TextureReplacer.BepInExPlugin.filePaths_tex)
				{
					if (!(keyValuePair.Key != fileNameKey))
					{
						string[] strArray = File.ReadAllLines(keyValuePair.Value + keyValuePair.Key);
						UnityEngine.Color color1;
						if (strArray.Length > 1 && ColorUtility.TryParseHtmlString("#" + strArray[0], out color1))
						{
							component.color = color1;
							UnityEngine.Color color2;
							if (strArray.Length > 1 && ColorUtility.TryParseHtmlString("#" + strArray[1], out color2))
								component.outlineColor = (Color32)color2;
						}
					}
				}
			}
		}

		private static Vector4 HexToRgba(string hexColor)
		{
			hexColor = hexColor.TrimStart('#');
			return new Vector4((float)int.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber) / (float)byte.MaxValue,
				(float)int.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber) / (float)byte.MaxValue,
				(float)int.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber) / (float)byte.MaxValue,
				(hexColor.Length == 8 ? (float)int.Parse(hexColor.Substring(6, 2), NumberStyles.HexNumber) : (float)byte.MaxValue) / (float)byte.MaxValue);
		}

		public static Texture2D LoadPNG(string filePath)
		{
			Texture2D tex = (Texture2D)null;
			if (File.Exists(filePath))
			{
				byte[] data = File.ReadAllBytes(filePath);
				tex = new Texture2D(2, 2);
				tex.LoadImage(data);
			}
			return tex;
		}

		public static Texture2D LoadPNG_Bump(string filePath)
		{
			Texture2D tex = (Texture2D)null;
			if (File.Exists(filePath))
			{
				byte[] data = File.ReadAllBytes(filePath);
				tex = new Texture2D(2, 2, UnityEngine.TextureFormat.R8, true, true);
				tex.LoadImage(data);
			}
			return tex;
		}

		public static Texture2D LoadPNGHDR(string filePath, float gamma = 0.4f)
		{
			Texture2D tex = (Texture2D)null;
			if (File.Exists(filePath))
			{
				byte[] data = File.ReadAllBytes(filePath);
				tex = new Texture2D(2, 2, UnityEngine.TextureFormat.RGBA32, false);
				if (!tex.LoadImage(data))
				{
					Console.WriteLine((object)"Failed to load PNG image.");
					return (Texture2D)null;
				}
				UnityEngine.Color[] pixels = tex.GetPixels();
				for (int index = 0; index < pixels.Length; ++index)
				{
					UnityEngine.Color color = pixels[index];
					color.r = TextureReplacer.BepInExPlugin.ApplyGamma(color.r, gamma);
					color.g = TextureReplacer.BepInExPlugin.ApplyGamma(color.g, gamma);
					color.b = TextureReplacer.BepInExPlugin.ApplyGamma(color.b, gamma);
					pixels[index] = color;
				}
				tex.SetPixels(pixels);
				tex.Apply();
			}
			return tex;
		}

		private static float ApplyGamma(float colorValue, float gamma)
		{
			if ((double)gamma <= 0.0)
				gamma = 1f;
			return Mathf.Clamp01(Mathf.Pow(colorValue, 1f / gamma));
		}

		public static Sprite TextureToSprite(Texture2D texture)
		{
			return Sprite.Create(texture, new Rect(0.0f, 0.0f, (float)texture.width, (float)texture.height), new Vector2(0.5f, 0.5f), 50f, 0U, SpriteMeshType.FullRect);
		}

		private static void FixControllerSprites()
		{
			ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_KeyboardBtnImage);
			ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_LeftMouseClickImage);
			ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_RightMouseClickImage);
			ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_LeftMouseHoldImage);
			ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_RightMouseHoldImage);
			ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_MiddleMouseScrollImage);
			ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_EnterImage);
			ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_SpacebarImage);
			ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_TabImage);
			ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_ShiftImage);
			ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_QuestionMarkImage);
			ReplaceSpritesInListController((List<Sprite>)CSingleton<CGameManager>.Instance.m_TextSO.m_GamepadCtrlBtnSpriteList);
			ReplaceSpritesInListController((List<Sprite>)CSingleton<CGameManager>.Instance.m_TextSO.m_XBoxCtrlBtnSpriteList);
			ReplaceSpritesInListController((List<Sprite>)CSingleton<CGameManager>.Instance.m_TextSO.m_PSCtrlBtnSpriteList);
		}

		private static void ReplaceSprite(ref Sprite sprite)
		{
			if (!((UnityEngine.Object)sprite != (UnityEngine.Object)null))
				return;
			Texture2D cachedTexture = GetCachedTexture(sprite.name);
			if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
			{
				Sprite sprite1 = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
				sprite1.name = sprite.name;
				sprite = sprite1;
			}
		}

		private static void ReplaceSpritesInListController(List<Sprite> spriteList)
		{
			for (int index = 0; index < spriteList.Count; ++index)
			{
				Sprite sprite1 = spriteList[index];
				if ((UnityEngine.Object)sprite1 != (UnityEngine.Object)null)
				{
					Texture2D cachedTexture = GetCachedTexture(sprite1.name);
					if ((UnityEngine.Object)cachedTexture != (UnityEngine.Object)null)
					{
						Sprite sprite2 = TextureReplacer.BepInExPlugin.TextureToSprite(cachedTexture);
						sprite2.name = sprite1.name;
						spriteList[index] = sprite2;
					}
				}
			}
		}
	}
}