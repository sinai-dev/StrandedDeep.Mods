using Beam;
using UnityEngine;
using UnityEngine.UI;

namespace ModPack.InGameMap
{
    public class CachedPlayer : MarkerBase, IPoolable
    {
        public Image image;
        public Text text;
        public IPlayer refPlayer;

        public void OnBorrowed(object[] data)
        {
            refPlayer = (IPlayer)data[0];
            text.text = $"P{refPlayer.Peer.Id + 1}";
            UiRoot.gameObject.SetActive(true);
        }

        public void OnReturnToPool()
        {
            refPlayer = null;
            UiRoot.gameObject.SetActive(false);
        }

        public override void SetPosition()
        {
            UiRoot.transform.localPosition = TransformToScreenCoordinates(refPlayer.transform.localPosition);
        }

        public void InitialCreation()
        {
            UiRoot = new GameObject("CachedPlayer");
            UiRoot.transform.SetParent(InGameMap.Instance.canvasRoot.transform);

            image = InstantiateIcon("player");
            image.transform.SetParent(UiRoot.transform);

            CheckFontAsset();
            if (!fontAsset)
            {
                InGameMap.Instance.LogWarning($"FontAsset is null!");
                fontAsset = Font.GetDefault();
            }

            text = new GameObject("username").AddComponent<Text>();
            text.gameObject.AddComponent<Outline>();
            text.transform.SetParent(UiRoot.transform);
            text.fontSize = 10;
            text.color = Color.white;
            text.font = fontAsset;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.alignment = TextAnchor.MiddleCenter;
            text.rectTransform.sizeDelta = new(25, 25);
            text.rectTransform.anchoredPosition = new(10, 0);
        }
    }
}
