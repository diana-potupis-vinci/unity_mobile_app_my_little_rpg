using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class QuestCardUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    public TextMeshProUGUI progressLabel;
    public Image progressFill; // Image -> Type = Filled

    public TextMeshProUGUI rewardText;

    public void Setup(string title, string description, int progress, int rewardValue, string rewardType)
    {
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;

        if (progressLabel != null) progressLabel.text = $"Progression: {progress}%";
        if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01(progress / 100f);

        if (rewardText != null) rewardText.text = $"+{rewardValue} {rewardType}";
    }
}
