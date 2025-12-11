using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterCardUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statusText;

    public void Setup(string name, bool isHunted /*, string type, Sprite icon */)
    {
        if (nameText != null)
            nameText.text = name;

        if (statusText != null)
            statusText.text = isHunted ? "Hunted" : "Not hunted";
    }
}
