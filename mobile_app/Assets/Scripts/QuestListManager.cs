using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;

[System.Serializable]
public class QuestProgressDto
{
    public int questId;
    public string title;
    public string description;
    public int progression;
    public bool isCompleted;
    public string message;
    public int awardValue;
    public string awardType;
}

public class QuestListManager : MonoBehaviour
{
    [Header("API Settings")]
    public string apiBaseUrl = "https://localhost:7105/api/PersonnageQuests/Active/";
    public string email = "test@gmail.com";
    public string apiNextGenUrl = "https://localhost:7105/api/PersonnageQuests/NextGenerationInfo";
    
    [Header("UI")]
    public Transform contentParent;
    public GameObject questCardPrefab;
    public TextMeshProUGUI questCountBadge;
    public TextMeshProUGUI timerText;
    public float refreshIntervalSeconds = 600f;
    private float timeRemaining;

    [Header("Notifications")]
    public NotificationManager notificationManager;

    private HashSet<int> _knownQuestIds = new HashSet<int>();

    private void Start()
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
        StartCoroutine(InitAndLoad());
    }
    IEnumerator InitAndLoad()
    {
        yield return LoadQuestsFromAPI();

        yield return SyncTimerWithServer();
    }

    private void Update()
    {
        if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f) timeRemaining = 0f;
            UpdateTimerLabel();

            if (timeRemaining <= 0f)
            {
                StartCoroutine(LoadQuestsFromAPI());
                StartCoroutine(SyncTimerWithServer());
            }
        }
    }

    private void UpdateTimerLabel()
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(timeRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}m {seconds:00}s";
    }


    public void Refresh()
    {
        StartCoroutine(LoadQuestsFromAPI());
        StartCoroutine(SyncTimerWithServer());
    }

    IEnumerator LoadQuestsFromAPI()
    {
        if (string.IsNullOrEmpty(apiBaseUrl) || string.IsNullOrEmpty(email))
        {
            Debug.LogError("❌ API base URL or email not set");
            yield break;
        }

        string url = apiBaseUrl.TrimEnd('/') + "/" + email;
        Debug.Log("🌐 Requête vers: " + url);

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            // Bypass SSL certificate
            req.certificateHandler = new BypassCertificate();
            //req.SetRequestHeader("Content-Type", "application/json");
            
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Erreur API: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            Debug.Log("📩 JSON reçu: " + json);

            // IMPORTANT: Using Newtonsoft.Json for simple array
            List<QuestProgressDto> quests = null;
            
            try
            {
                quests = JsonConvert.DeserializeObject<List<QuestProgressDto>>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ Erreur parsing JSON: " + e.Message);
                yield break;
            }

            if (quests == null || quests.Count == 0)
            {
                Debug.LogWarning("⚠️ Aucune quête trouvée");
                if (questCountBadge != null)
                    questCountBadge.text = "0/3";
                yield break;
            }

            Debug.Log($"🟢 {quests.Count} quêtes chargées");

            List<QuestProgressDto> newQuests = new List<QuestProgressDto>();

            foreach (var q in quests)
            {
                if (!_knownQuestIds.Contains(q.questId))
                {
                    newQuests.Add(q);
                    _knownQuestIds.Add(q.questId);
                }
            }

            if (newQuests.Count > 0 && notificationManager != null)
            {
                if (newQuests.Count == 1)
                {
                    var q = newQuests[0];
                    notificationManager.SendQuestNotification(
                        "Nouvelle quête disponible",
                        q.title ?? "Une nouvelle quête vient d’être ajoutée."
                    );
                }
                else
                {
                    notificationManager.SendQuestNotification(
                        "Nouvelles quêtes disponibles",
                        $"{newQuests.Count} nouvelles quêtes ont été ajoutées."
                    );
                }
            }

            // Nettoyer anciennes cartes
            if (contentParent != null)
            {
                foreach (Transform child in contentParent)
                    Destroy(child.gameObject);
            }

            // Mise à jour badge
            if (questCountBadge != null)
                questCountBadge.text = $"{quests.Count}/{quests.Count}";

            // Créer les cartes UI
            foreach (var quest in quests)
            {
                GameObject card = Instantiate(questCardPrefab, contentParent);
                QuestCardUI ui = card.GetComponent<QuestCardUI>();
                
                if (ui == null)
                {
                    Debug.LogError("❌ QuestCardUI manquant sur le prefab!");
                    continue;
                }

                Debug.Log($"🟦 Carte créée: {quest.title}");

                ui.Setup(
                    quest.title ?? "Sans titre",
                    quest.description ?? "",
                    quest.progression,
                    quest.awardValue,
                    "XP"
                );
            }
        }
    }

    IEnumerator SyncTimerWithServer()
{
    if (string.IsNullOrEmpty(apiNextGenUrl))
        yield break;

    using (UnityWebRequest req = UnityWebRequest.Get(apiNextGenUrl))
    {
        req.certificateHandler = new BypassCertificate();
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erreur NextGenerationInfo: " + req.error);
            yield break;
        }

        var json = req.downloadHandler.text;
        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        if (dict != null && dict.TryGetValue("secondsUntilNext", out var secondsObj)
                        && dict.TryGetValue("intervalSeconds", out var intervalObj))
        {
            int secondsUntil = System.Convert.ToInt32(secondsObj);
            int interval = System.Convert.ToInt32(intervalObj);

            refreshIntervalSeconds = interval;
            timeRemaining = secondsUntil;
            if (timeRemaining < 0f) timeRemaining = 0f;
            UpdateTimerLabel();
        }
    }
}

}

// Class to bypass SSL certificates (for development only!)
public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
