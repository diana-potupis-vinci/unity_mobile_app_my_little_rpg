using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;

public class PokedexManager : MonoBehaviour
{
    [Header("API")]
    public string apiBaseUrl = "https://localhost:7105/api/Monsters/Pokedex";
    public int personnageId = 7;
    public int pageSize = 9;

    [Header("UI")]
    public Transform contentParent;
    public GameObject monsterCardPrefab;
    public TextMeshProUGUI pageInfoText;

    private int currentPage = 0;
    private int totalCount = 0;

    void Start()
    {
        Debug.Log("PokedexManager Start");
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

    IEnumerator LoadPage(int pageIndex)
    {
        int offset = pageIndex * pageSize;
        string url = $"{apiBaseUrl}?personnageId={personnageId}&offset={offset}&limit={pageSize}";

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
                    ui.Setup(m.name, m.isHunted);
            }

            if (pageInfoText != null)
            {
                int maxPage = Mathf.Max(0, (totalCount - 1) / pageSize);
                pageInfoText.text = $"Page {currentPage + 1} / {maxPage + 1}";
            }
        }
    }
}
