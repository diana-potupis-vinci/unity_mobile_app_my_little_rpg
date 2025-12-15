using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using Newtonsoft.Json;

[System.Serializable]
public class LoginRequestDto
{
    public string email;
    public string motDePasse;
}

[System.Serializable]
public class LoginResponseDto
{
    public string email;
    public bool estConnecte;
    public int personnageId;
}

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI errorText;
    public string loginUrl = "https://localhost:7105/api/auth/login";

    public GameObject loginScreen;
    public GameObject questsScreen;
    public GameObject pokedexScreen;

    public QuestListManager questListManager;
    public PokedexManager pokedexManager;

    public void OnLoginClicked()
    {
        if (errorText != null) errorText.text = "";

        var req = new LoginRequestDto
        {
            email = emailInput.text,
            motDePasse = passwordInput.text
        };

        StartCoroutine(LoginCoroutine(req));
    }

    IEnumerator LoginCoroutine(LoginRequestDto data)
    {
        string json = JsonConvert.SerializeObject(data);

        using (var req = new UnityWebRequest(loginUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.certificateHandler = new BypassCertificate();

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Login error: " + req.error);
                if (errorText != null) errorText.text = "Email ou mot de passe incorrect.";
                yield break;
            }

            var respJson = req.downloadHandler.text;
            LoginResponseDto resp;
            try
            {
                resp = JsonConvert.DeserializeObject<LoginResponseDto>(respJson);
            }
            catch
            {
                Debug.LogError("Login parse error: " + respJson);
                if (errorText != null) errorText.text = "Réponse du serveur invalide.";
                yield break;
            }

            if (!resp.estConnecte)
            {
                Debug.LogError("Login failed (estConnecte == false)");
                if (errorText != null) errorText.text = "Email ou mot de passe incorrect.";
                yield break;
            }

            GameSession.Email = resp.email;
            GameSession.PersonnageId = resp.personnageId;

            if (questListManager != null)
            {
                questListManager.email = GameSession.Email;
                questListManager.Refresh();
            }

            if (pokedexManager != null)
            {
                pokedexManager.InitFromSession();
            }

            loginScreen.SetActive(false);
            questsScreen.SetActive(true);
            pokedexScreen.SetActive(false);
        }
    }
}
