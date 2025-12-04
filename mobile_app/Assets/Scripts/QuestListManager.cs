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
    
    [Header("UI")]
    public Transform contentParent;
    public GameObject questCardPrefab;
    public TextMeshProUGUI questCountBadge;

    private void Start()
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
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

            // Nettoyer anciennes cartes
            if (contentParent != null)
            {
                foreach (Transform child in contentParent)
                    Destroy(child.gameObject);
            }

            // Mise à jour badge
            if (questCountBadge != null)
                questCountBadge.text = $"{quests.Count}/3";

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
}

// Class to bypass SSL certificates (for development only!)
public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
