using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
public class QuestProgressDto
{
    public int questId;
    public string title;
    public string description;
    public int progression;
    public bool isCompleted;
    public string message;

    public int awardValue = 0;
    public string awardType = "XP";
}

public class QuestListManager : MonoBehaviour
{
    [Header("API Settings")]
    public string apiBaseUrl = "http://localhost:5087/api/PersonnageQuests/Active/";
    public string email = "test@example.com";

    [Header("UI")]
    public Transform contentParent;      // drag -> ScrollView/Content
    public GameObject questCardPrefab;   // drag -> Prefab
    public TextMeshProUGUI questCountBadge; // optional

    private void Start()
    {
        StartCoroutine(LoadQuestsFromAPI());
    }

    public void Refresh()
    {
        StartCoroutine(LoadQuestsFromAPI());
    }

    IEnumerator LoadQuestsFromAPI()
    {
        if (string.IsNullOrEmpty(apiBaseUrl) || string.IsNullOrEmpty(email))
        {
            Debug.LogError("API base URL or email not set on QuestListManager.");
            yield break;
        }

        string url = apiBaseUrl.TrimEnd('/') + "/" + email;

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API error: " + req.error + " | Response: " + req.downloadHandler.text);
                yield break;
            }

            string json = req.downloadHandler.text;

            List<QuestProgressDto> quests = JsonUtilityExtended.FromJsonList<QuestProgressDto>(json);

            if (contentParent != null)
            {
                foreach (Transform child in contentParent)
                    Destroy(child.gameObject);
            }

            if (questCountBadge != null)
                questCountBadge.text = $"{quests.Count}/3";

            foreach (var quest in quests)
            {
                GameObject card = Instantiate(questCardPrefab, contentParent);
                QuestCardUI ui = card.GetComponent<QuestCardUI>();
                if (ui != null)
                {
                    ui.Setup(
                        quest.title ?? "No title",
                        quest.description ?? "",
                        quest.progression,
                        quest.awardValue,
                        string.IsNullOrEmpty(quest.awardType) ? "XP" : quest.awardType
                    );
                }
            }
        }
    }
}
