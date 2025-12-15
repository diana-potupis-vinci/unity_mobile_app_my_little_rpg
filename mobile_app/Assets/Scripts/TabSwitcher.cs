using UnityEngine;

public class TabSwitcher : MonoBehaviour
{
    public GameObject questsScreen;
    public GameObject pokedexScreen;

    public void ShowQuests()
    {
        questsScreen.SetActive(true);
        pokedexScreen.SetActive(false);
    }

    public void ShowPokedex()
    {
        questsScreen.SetActive(false);
        pokedexScreen.SetActive(true);
    }
}
