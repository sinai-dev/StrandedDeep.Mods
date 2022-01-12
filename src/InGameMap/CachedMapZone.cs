using Beam.Terrain;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModPack.InGameMap
{
	public enum MapType
    {
		Island,
		Start,
		Eel,
		Squid,
		Shark,
		Aircraft
    }

    public class CachedMapZone : MarkerBase, IPoolable
	{
		public Map RefMap { get; private set; }
		public Zone RefZone { get; private set; }
		public MapType Type { get; private set; }

		string MapId => RefMap.EditorData.Id;
		bool IsMission => MapId.Contains("MISSION");

		private bool? discoveredState;
		private string discoveredKey;
		private string undiscoveredKey;
		private Vector2 worldPosition;

		private readonly Dictionary<string, Image> icons = new();
		private Text label;

		public void OnBorrowed(object[] data)
		{
			RefMap = (Map)data[0];
			RefZone = (Zone)data[1];
			worldPosition = (Vector2)data[2];

			if (RefZone.IsStartingIsland)
            {
				Type = MapType.Start;
				discoveredKey = "heightmap";
				label.text = "Start";
				label.color = Color.black;
            }
			else if (IsMission)
			{
				undiscoveredKey = InGameMap.revealMissions.Value
					? "unknown_mission"
					: "unknown";

				if (MapId.Contains("MISSION_3"))
                {
					Type = MapType.Aircraft;
					discoveredKey = "heightmap";
				}
				else
				{
					discoveredKey = "boss";

					if (MapId.Contains("MISSION_0"))
						Type = MapType.Eel;
					else if (MapId.Contains("MISSION_1"))
						Type = MapType.Shark;
					else if (MapId.Contains("MISSION_2"))
						Type = MapType.Squid;
				}
            }
			else
            {
				Type = MapType.Island;
				label.gameObject.SetActive(false);
				undiscoveredKey = "unknown";
				discoveredKey = "heightmap";
			}
		}

		public void OnReturnToPool()
		{
			RefMap = null;
			RefZone = null;
			discoveredState = null;
			discoveredKey = null;
			undiscoveredKey = null;

			label.gameObject.SetActive(false);

			foreach (var entry in icons)
				entry.Value.gameObject.SetActive(false);
		}

        public override void SetPosition()
        {
			UiRoot.transform.localPosition = this.TransformToScreenCoordinates(new Vector3(worldPosition.x, 0, worldPosition.y));
		}

        public void UpdateState(bool discovered)
        {
			if (discoveredState == discovered)
				return;
			discoveredState = discovered;

			if (!string.IsNullOrEmpty(discoveredKey))
				icons[discoveredKey].gameObject.SetActive(discovered);

			if (!string.IsNullOrEmpty(undiscoveredKey))
				icons[undiscoveredKey].gameObject.SetActive(!discovered);

			label.gameObject.SetActive(discovered && Type != MapType.Island);

			if (IsMission)
            {
				if (!discovered)
				{
					if (InGameMap.revealMissions.Value)
					{
						label.gameObject.SetActive(true);
						label.text = "???";
						label.color = Color.yellow;
					}
				}
				else
				{
					if (Type == MapType.Aircraft)
					{
						label.color = new(0.05f, 0.2f, 0.04f);
						label.text = "Aircraft";
					}
					else
					{
						label.color = Color.red;

						if (Type == MapType.Eel)
							label.text = "Abaia (Eel)";
						else if (Type == MapType.Shark)
							label.text = "The Meg (Shark)";
						else if (Type == MapType.Squid)
							label.text = "Lusca (Squid)";
					}
				}
			}
		}

		private static Color shallowsColor = new(0.5f, 0.89f, 1.0f);
		private static Color sandColor = new(1f, 0.9f, 0.5f);
		private static Color grassColor = new(0.5f, 0.9f, 0.2f);

		public void SetHeightmap(Map map)
		{
			var texture2D = icons["heightmap"].sprite.texture;

			for (int y = 0; y < map.HeightmapData.GetLength(0); y++)
			{
				for (int x = 0; x < map.HeightmapData.GetLength(1); x++)
				{
					Color color;
					float height = map.HeightmapData[y, x];

					if (height < 0.660f)
						color = Color.clear;
					else if (height < 0.675f)
						color = shallowsColor;
					else if (height < 0.680f)
						color = sandColor;
					else
						color = grassColor;

					texture2D.SetPixel(x, y, color);
				}
			}

			texture2D.Apply();
		}

		public void InitialCreation()
		{
			UiRoot = new GameObject("CachedEvent");
			UiRoot.transform.SetParent(InGameMap.Instance.canvasRoot.transform);

			foreach (var entry in iconTemplates)
			{
				if (entry.Key == "player")
					continue;

				Image icon;
				if (entry.Key == "heightmap")
					icon = CreateHeightmap();
				else
					icon = InstantiateIcon(entry.Key);

				icon.gameObject.SetActive(false);
				icon.transform.SetParent(UiRoot.transform);
				icons.Add(entry.Key, icon);
			}

			CheckFontAsset();

			label = new GameObject("label").AddComponent<Text>();
			label.transform.SetParent(UiRoot.transform);
			label.fontSize = 10;
			label.color = Color.white;
			label.font = fontAsset;
			label.horizontalOverflow = HorizontalWrapMode.Overflow;
			label.alignment = TextAnchor.MiddleCenter;
			label.rectTransform.pivot = new Vector2(0.5f, 0f);
			label.rectTransform.sizeDelta = new(36, 20);
			label.rectTransform.anchoredPosition = new Vector2(0, 5);
		}

		public Image CreateHeightmap()
		{
			GameObject heightmapObj = new("heightmap");
			heightmapObj.hideFlags = HideFlags.HideAndDontSave;
			GameObject.DontDestroyOnLoad(heightmapObj);
			Image heightmapImg = heightmapObj.AddComponent<Image>();
			Texture2D heightmapTex = new(256, 256, TextureFormat.ARGB32, false);
			heightmapImg.sprite = Sprite.Create(heightmapTex,
				new Rect(0, 0, heightmapTex.width, heightmapTex.height),
				new Vector2(heightmapTex.width * 0.5f, heightmapTex.height * 0.5f));
			heightmapImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ICON_SIZE);
			heightmapImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ICON_SIZE);
			return heightmapImg;
		}
	}
}
