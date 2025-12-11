using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public GameObject questsScreen;
    public GameObject pokedexScreen;

    void Start()
    {
        ShowQuests();
    }

    public void ShowQuests()
    {
        if (questsScreen != null) questsScreen.SetActive(true);
        if (pokedexScreen != null) pokedexScreen.SetActive(false);
    }

    public void ShowPokedex()
    {
        if (questsScreen != null) questsScreen.SetActive(false);
        if (pokedexScreen != null) pokedexScreen.SetActive(true);
    }
}