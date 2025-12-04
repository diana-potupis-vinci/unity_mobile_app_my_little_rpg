using UnityEngine;
using TMPro;

public class NextQuestTimer : MonoBehaviour
{
    public TextMeshProUGUI timerLabel;
    public QuestListManager questList;
    public float intervalSeconds = 60f;
    private float timeRemaining;

    private void Start()
    {
        timeRemaining = intervalSeconds;
        UpdateLabel();
    }

    private void Update()
    {
        if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f) timeRemaining = 0f;
            UpdateLabel();

            if (timeRemaining <= 0f)
            {
                if (questList != null)
                    questList.Refresh();

                timeRemaining = intervalSeconds;
            }
        }
    }

    private void UpdateLabel()
    {
        if (timerLabel == null) return;

        int seconds = Mathf.CeilToInt(timeRemaining);
        int minutes = seconds / 60;
        int sec = seconds % 60;

        timerLabel.text = $"{minutes:00}m {sec:00}s";
    }
}
