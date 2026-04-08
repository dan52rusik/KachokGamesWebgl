using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private GameObject[] hearts;
        [SerializeField] private TextMeshProUGUI treeCountText;

        [Header("Roblox Style")]
        [SerializeField] private bool applyRobloxStyle = true;
        [SerializeField] private Color cardColor = new Color32(255, 196, 39, 250);
        [SerializeField] private Color cardTextColor = new Color32(24, 18, 10, 255);
        [SerializeField] private Color cardShadowColor = new Color32(0, 0, 0, 160);

        private int _treeCount;

        private void Awake()
        {
            if (applyRobloxStyle)
                ApplyRobloxStyle();
        }

        public void SetHealth(int health)
        {
            if (health > hearts.Length)
                return;

            for (int i = 0; i < hearts.Length; i++)
                hearts[i].SetActive(health > i);
        }

        public int TreeCount
        {
            get => _treeCount;
            set
            {
                _treeCount = value;

                if (treeCountText != null)
                    treeCountText.SetText(_treeCount.ToString());
            }
        }

        private void ApplyRobloxStyle()
        {
            StyleTreeCounter();
            StyleHearts();
        }

        private void StyleTreeCounter()
        {
            if (treeCountText == null)
                return;

            RectTransform rect = treeCountText.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(24f, -18f);
            rect.sizeDelta = new Vector2(240f, 56f);

            treeCountText.fontSize = 28f;
            treeCountText.fontStyle = FontStyles.Bold;
            treeCountText.color = cardTextColor;
            treeCountText.alignment = TextAlignmentOptions.Left;
            treeCountText.textWrappingMode = TextWrappingModes.NoWrap;
            treeCountText.margin = new Vector4(18f, 10f, 18f, 10f);

            Image image = EnsureBackground(treeCountText.gameObject);
            image.color = cardColor;

            Outline outline = treeCountText.gameObject.GetComponent<Outline>();
            if (outline == null)
                outline = treeCountText.gameObject.AddComponent<Outline>();

            outline.effectColor = cardShadowColor;
            outline.effectDistance = new Vector2(4f, -4f);
            outline.useGraphicAlpha = true;
        }

        private void StyleHearts()
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] == null)
                    continue;

                RectTransform rect = hearts[i].GetComponent<RectTransform>();
                if (rect == null)
                    continue;

                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(24f + i * 54f, -86f);
                rect.sizeDelta = new Vector2(44f, 44f);

                Image image = hearts[i].GetComponent<Image>();
                if (image != null)
                    image.color = new Color32(255, 86, 86, 255);
            }
        }

        private static Image EnsureBackground(GameObject target)
        {
            Image image = target.GetComponent<Image>();
            if (image == null)
                image = target.AddComponent<Image>();

            return image;
        }
    }
}
