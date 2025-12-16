using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;

[System.Serializable]
public class FavoriteMonsterDtoUnity
{
    public int personnageId;
    public int monsterId;
}

[System.Serializable]
public class FavoriteToggleResponse
{
    public string message;
    public bool isFavorite;
    public int favoriteId;
}

public class MonsterCardUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statusText;
    public Image background;

    [Header("Favorite")]
    public Image favoriteIcon;
    public Sprite favoriteOnSprite;
    public Sprite favoriteOffSprite;

    [Header("Colors")]
    public Color huntedColor = new Color(0.7f, 1f, 0.7f);
    public Color notHuntedColor = new Color(0.9f, 0.9f, 0.9f);

    [HideInInspector] public int monsterId;
    [HideInInspector] public bool isHunted;
    [HideInInspector] public bool isFavorite;

    private PokedexManager pokedexManager;

    public void Init(int monsterId, string name, bool isHunted, bool isFavorite,
                     Sprite icon, PokedexManager manager)
    {
        this.monsterId = monsterId;
        this.isHunted = isHunted;
        this.isFavorite = isFavorite;
        this.pokedexManager = manager;

        if (nameText != null)
            nameText.text = name;

        if (statusText != null)
            statusText.text = isHunted ? "Hunted" : "Not hunted";

        if (iconImage != null && icon != null)
            iconImage.sprite = icon;

        if (background != null)
            background.color = isHunted ? huntedColor : notHuntedColor;

        UpdateFavoriteIcon();
    }

    public void OnFavoriteClicked()
    {
        if (pokedexManager == null)
        {
            Debug.LogError("PokedexManager not set on MonsterCardUI");
            return;
        }

        StartCoroutine(ToggleFavoriteCoroutine());
    }

    IEnumerator ToggleFavoriteCoroutine()
    {
        string url = "https://localhost:7105/api/Monsters/favorite";

        var dto = new FavoriteMonsterDtoUnity
        {
            personnageId = pokedexManager.personnageId,
            monsterId = this.monsterId
        };

        string json = JsonConvert.SerializeObject(dto);

        using (var req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.certificateHandler = new BypassCertificate();

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Favorite API error: " + req.error);
                yield break;
            }

            var respJson = req.downloadHandler.text;
            FavoriteToggleResponse resp;
            try
            {
                resp = JsonConvert.DeserializeObject<FavoriteToggleResponse>(respJson);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Favorite JSON parse error: " + e.Message);
                yield break;
            }

            isFavorite = resp.isFavorite;
            UpdateFavoriteIcon();
        }
    }

    private void UpdateFavoriteIcon()
    {
        if (favoriteIcon == null) return;

        favoriteIcon.sprite = isFavorite ? favoriteOnSprite : favoriteOffSprite;
    }
}
