using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class ClassementEntryDto
{
    public int rang;
    public string nom;
    public int niveau;
    public int experience;
    public int force;
    public int defense;
    public int nombreMonstres;
}

public class ClassementScreenManager : MonoBehaviour
{
    [Header("API")]
    public string apiUrl = "https://localhost:7105/api/personnages/classement";
    public string critere = "niveau"; // "niveau" | "force" | "monstres"

    [Header("UI")]
    public Transform contentParent;
    public GameObject ligneClassementPrefab;
    public TextMeshProUGUI errorText;

    void OnEnable()
    {
        StartCoroutine(LoadClassement());
    }

    IEnumerator LoadClassement()
    {
        if (errorText != null) errorText.text = "";

        string url = apiUrl + "?critere=" + critere;
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.certificateHandler = new BypassCertificate();
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Classement API error: " + req.error);
                if (errorText != null) errorText.text = "Erreur de connexion au serveur.";
                yield break;
            }

            var json = req.downloadHandler.text;
            List<ClassementEntryDto> data;
            try
            {
                data = JsonConvert.DeserializeObject<List<ClassementEntryDto>>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Classement JSON parse error: " + e.Message);
                if (errorText != null) errorText.text = "Données du classement invalides.";
                yield break;
            }

            if (data == null)
                yield break;

            foreach (Transform child in contentParent)
                Destroy(child.gameObject);

            foreach (var e in data)
            {
                var row = GameObject.Instantiate(ligneClassementPrefab, contentParent);

                var txtRang = row.transform.Find("Rang")?.GetComponent<TextMeshProUGUI>();
                var txtName = row.transform.Find("UserName")?.GetComponent<TextMeshProUGUI>();
                var txtValue = row.transform.Find("valeurClassement")?.GetComponent<TextMeshProUGUI>();

                if (txtRang != null) txtRang.text = e.rang.ToString();
                if (txtName != null) txtName.text = e.nom;

                if (txtValue != null)
                {
                    switch (critere.ToLower())
                    {
                        case "force":
                            txtValue.text = e.force.ToString();
                            break;
                        case "monstres":
                            txtValue.text = e.nombreMonstres.ToString();
                            break;
                        default: // "niveau"
                            txtValue.text = e.niveau.ToString();
                            break;
                    }
                }
            }
        }
    }

    public void SetCritereNiveau()
    {
        critere = "niveau";
        StartCoroutine(LoadClassement());
    }

    public void SetCritereForce()
    {
        critere = "force";
        StartCoroutine(LoadClassement());
    }

    public void SetCritereMonstres()
    {
        critere = "monstres";
        StartCoroutine(LoadClassement());
    }
}