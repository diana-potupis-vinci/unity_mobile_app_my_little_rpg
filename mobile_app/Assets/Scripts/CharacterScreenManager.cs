using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

[System.Serializable]
public class PersonnageDtoUnity
{
    public int id;
    public string nom;
    public int niveau;
    public int experience;
    public int pointsVie;
    public int pointsVieMax;
    public int force;
    public int defense;
    public int positionX;
    public int positionY;
}

public class CharacterScreenManager : MonoBehaviour
{
    public string apiUrl = "https://localhost:7105/api/personnages/email/";

    [Header("UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI positionText;

    public void LoadFromSession()
    {
        if (string.IsNullOrEmpty(GameSession.Email))
        {
            Debug.LogError("GameSession.Email vide");
            return;
        }

        StartCoroutine(LoadCharacter(GameSession.Email));
    }

    IEnumerator LoadCharacter(string email)
    {
        string url = apiUrl + email;
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.certificateHandler = new BypassCertificate();
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Personnage API error: " + req.error);
                yield break;
            }

            var json = req.downloadHandler.text;
            PersonnageDtoUnity data;
            try
            {
                data = JsonConvert.DeserializeObject<PersonnageDtoUnity>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Personnage JSON parse error: " + e.Message);
                yield break;
            }

            if (data == null) yield break;

            if (nameText != null) nameText.text = data.nom;
            if (levelText != null) levelText.text = "Niveau: " + data.niveau;
            if (xpText != null) xpText.text = "XP: " + data.experience;
            if (hpText != null) hpText.text = $"PV: {data.pointsVie}/{data.pointsVieMax}";
            if (attackText != null) attackText.text = "Force: " + data.force;
            if (defenseText != null) defenseText.text = "Défense: " + data.defense;
            if (positionText != null) positionText.text = $"Position: {data.positionX}, {data.positionY}";
        }
    }
}
