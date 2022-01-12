using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ModPack.InGameMap
{
    public abstract class MarkerBase
	{
		public GameObject UiRoot { get; protected set; }

		protected static Font fontAsset;

		public abstract void SetPosition();

		protected Vector3 TransformToScreenCoordinates(Vector3 pos)
		{
			// The in-game world is rotated 45deg to the right.
			pos = Quaternion.Euler(0, -45f, 0) * pos;

			// Screen dimensions can change. Could be cached, but this isn't very expensive anyway.
			float mapScaleX = Screen.width / InGameMap.REFERENCE_SCREEN_WIDTH;
			float mapScaleY = Screen.height / InGameMap.REFERENCE_SCREEN_HEIGHT;
			// Scale the width and height for the map.
			float adjustedWidth = pos.x * ((InGameMap.MAP_IMAGE_DIMENSIONS - 250f) / (InGameMap.INGAME_DIMENSIONS * mapScaleX));
			float adjustedHeight = pos.z * ((InGameMap.MAP_IMAGE_DIMENSIONS - 250f) / (InGameMap.INGAME_DIMENSIONS * mapScaleY));
			return new(adjustedWidth, adjustedHeight);
		}

		public Image InstantiateIcon(string key) => Object.Instantiate(iconTemplates[key]);

		// --------- Static ---------

		public const float ICON_SIZE = 30f;

		internal static readonly Dictionary<string, Image> iconTemplates = new();

		public static void InitTemplates()
		{
			GameObject cacheHolder = new("InGameMapMod_CacheHolder");
			GameObject.DontDestroyOnLoad(cacheHolder);
			cacheHolder.hideFlags = HideFlags.HideAndDontSave;
			cacheHolder.SetActive(false);

			var playerImage = CreateTemplate(cacheHolder, "player", 7f, 350, "ingamemap.player.png");
			playerImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
			iconTemplates.Add("player", playerImage);

			iconTemplates.Add("unknown", CreateTemplate(cacheHolder, "unknown", ICON_SIZE, 350, "ingamemap.unknown.png"));
			iconTemplates.Add("unknown_mission", CreateTemplate(cacheHolder, "unknown_mission", ICON_SIZE, 350, "ingamemap.unknown_mission.png"));
			iconTemplates.Add("boss", CreateTemplate(cacheHolder, "boss", ICON_SIZE, 350, "ingamemap.boss.png"));

			iconTemplates.Add("heightmap", null);
		}

		protected static void CheckFontAsset()
		{
			if (!fontAsset)
			{
				foreach (var font in Resources.FindObjectsOfTypeAll<Font>())
				{
					if (font.name == "Metropolis-MediumItalic")
					{
						fontAsset = Object.Instantiate(font);
						GameObject.DontDestroyOnLoad(fontAsset);
						break;
					}
				}
			}
		}

		public static Image CreateTemplate(GameObject parent, string name, float imageSize, int texSize, string fileName)
		{
			GameObject gameObject = new(name);
			GameObject.DontDestroyOnLoad(gameObject);
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			gameObject.transform.SetParent(parent.transform);

			Image image = gameObject.AddComponent<Image>();
			image.rectTransform.pivot = new Vector2(0.5f, 0.5f);
			image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageSize);
			image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageSize);

			Texture2D texture = new(texSize, texSize, TextureFormat.ARGB32, false);
			string path = Path.Combine(Path.GetDirectoryName(typeof(InGameMap).Assembly.Location), "Resources", fileName);
			ImageConversion.LoadImage(texture, File.ReadAllBytes(path));

			Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texSize, texSize), new Vector2(texSize * 0.5f, texSize * 0.5f));
			image.sprite = sprite;

			return image;
		}
	}
}
