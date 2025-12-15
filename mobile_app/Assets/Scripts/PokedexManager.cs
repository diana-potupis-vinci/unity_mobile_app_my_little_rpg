using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;

public class PokedexManager : MonoBehaviour
{
    [Header("API")]
    public string apiBaseUrl = "https://localhost:7105/api/Monsters/Pokedex";
    public int personnageId;
    public int pageSize = 9;

    [Header("UI")]
    public Transform contentParent;
    public GameObject monsterCardPrefab;
    public TextMeshProUGUI pageInfoText;

    [Header("Icons")]
    public Sprite defaultIcon;

    public enum HuntFilter { All, Hunted, NotHunted }

    [Header("Filters")]
    public HuntFilter huntFilter = HuntFilter.All;
    public string typeFilter = null;

    [Header("Search")]
    public TMP_InputField searchInput;
    public string nameFilter = null;

    private int currentPage = 0;
    private int totalCount = 0;

    void Start()
    {
        Debug.Log("PokedexManager Start");
        //StartCoroutine(LoadPage(0));
    }
    public void InitFromSession()
    {
        if (GameSession.PersonnageId == 0)
        {
            Debug.LogWarning("GameSession.PersonnageId is empty");
            return;
        }

        personnageId = GameSession.PersonnageId;
        StartCoroutine(LoadPage(0));
    }

    public void OnNextPage()
    {
        int maxPage = Mathf.Max(0, (totalCount - 1) / pageSize);
        if (currentPage < maxPage)
            StartCoroutine(LoadPage(currentPage + 1));
    }

    public void OnPreviousPage()
    {
        if (currentPage > 0)
            StartCoroutine(LoadPage(currentPage - 1));
    }

    IEnumerator LoadIconAndSetupCard(MonsterPokedexDtoUnity m, MonsterCardUI ui)
    {
        if (string.IsNullOrEmpty(m.spriteUrl))
        {
            ui.Setup(m.name, m.isHunted, defaultIcon);
            yield break;
        }

        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(m.spriteUrl))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Image load error for " + m.name + ": " + req.error);
                ui.Setup(m.name, m.isHunted, defaultIcon);
                yield break;
            }

            var tex = DownloadHandlerTexture.GetContent(req);
            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );

            ui.Setup(m.name, m.isHunted, sprite);
        }
    }

    IEnumerator LoadPage(int pageIndex)
    {
        int offset = pageIndex * pageSize;
        string url = $"{apiBaseUrl}?personnageId={personnageId}&offset={offset}&limit={pageSize}";

        if (huntFilter == HuntFilter.Hunted)
            url += "&hunted=true";
        else if (huntFilter == HuntFilter.NotHunted)
            url += "&hunted=false";

        if (!string.IsNullOrEmpty(typeFilter))
            url += $"&type={typeFilter}";

        if (!string.IsNullOrEmpty(nameFilter))
            url += $"&name={UnityWebRequest.EscapeURL(nameFilter)}";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.certificateHandler = new BypassCertificate();

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Pokedex API error: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;

            PagedMonsterDtoUnity data;
            try
            {
                data = JsonConvert.DeserializeObject<PagedMonsterDtoUnity>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Pokedex JSON parse error: " + e.Message);
                yield break;
            }

            if (data == null || data.items == null)
                yield break;

            totalCount = data.totalCount;
            currentPage = pageIndex;

            foreach (Transform child in contentParent)
                Destroy(child.gameObject);

            foreach (var m in data.items)
            {
                GameObject card = Instantiate(monsterCardPrefab, contentParent);
                var ui = card.GetComponent<MonsterCardUI>();
                if (ui != null)
                    StartCoroutine(LoadIconAndSetupCard(m, ui));
            }

            if (pageInfoText != null)
            {
                int maxPage = Mathf.Max(0, (totalCount - 1) / pageSize);
                pageInfoText.text = $"Page {currentPage + 1} / {maxPage + 1}";
            }
        }
    }

    public void SetTypeAll()
    {
        typeFilter = null;
        StartCoroutine(LoadPage(0));
    }

    public void SetTypeHunted()
    {
        typeFilter = null;
        huntFilter = HuntFilter.Hunted;
        StartCoroutine(LoadPage(0));
    }

    public void SetTypeFire()
    {
        typeFilter = "Fire";
        StartCoroutine(LoadPage(0));
    }
    public void SetTypeElectric()
    {
        typeFilter = "Electric";
        StartCoroutine(LoadPage(0));
    }

    public void SetTypeFairy()
    {
        typeFilter = "Fairy";
        StartCoroutine(LoadPage(0));
    }

    public void SetTypeIce()
    {
        typeFilter = "Ice";
        StartCoroutine(LoadPage(0));
    }

    public void SetTypeFighting()
    {
        typeFilter = "Fighting";
        StartCoroutine(LoadPage(0));
    }
    public void OnSearchClicked()
    {
        if (searchInput != null)
            nameFilter = searchInput.text;
        else
            nameFilter = null;

        StartCoroutine(LoadPage(0));
    }

}
