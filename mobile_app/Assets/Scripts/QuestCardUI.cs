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
        if (progressFill != null)
        {
            float normalized = progress / 100f;
            float minVisible = 0.15f; // 5% always visible

            if (normalized < minVisible)
                normalized = minVisible;   // don't let the bar become completely invisible

            progressFill.fillAmount = Mathf.Clamp01(normalized);
            Debug.Log($"Progress raw: {progress}, normalized: {normalized}");
        }


        if (rewardText != null) rewardText.text = $"+{rewardValue} {rewardType}";
    }
}
