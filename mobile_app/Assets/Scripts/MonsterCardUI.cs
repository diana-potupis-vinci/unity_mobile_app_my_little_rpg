using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterCardUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statusText;
    public Image background;

    public Color huntedColor = new Color(0.7f, 1f, 0.7f);
    public Color notHuntedColor = new Color(0.9f, 0.9f, 0.9f);

    public void Setup(string name, bool isHunted, Sprite icon/*, string type, Sprite icon */)
    {
        if (nameText != null)
            nameText.text = name;

        if (statusText != null)
            statusText.text = isHunted ? "Hunted" : "Not hunted";
        if (iconImage != null && icon != null)
            iconImage.sprite = icon;

        if (background != null)
            background.color = isHunted ? huntedColor : notHuntedColor;
    }
}
