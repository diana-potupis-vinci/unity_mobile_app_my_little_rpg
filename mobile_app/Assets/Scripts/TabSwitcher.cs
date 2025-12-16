using UnityEngine;

public class TabSwitcher : MonoBehaviour
{
    public GameObject questsScreen;
    public GameObject pokedexScreen;
    public GameObject characterScreen;
    public CharacterScreenManager characterScreenManager;

    public void ShowQuests()
    {
        questsScreen.SetActive(true);
        pokedexScreen.SetActive(false);
        characterScreen.SetActive(false);
    }

    public void ShowPokedex()
    {
        questsScreen.SetActive(false);
        pokedexScreen.SetActive(true);
        characterScreen.SetActive(false);
    }

    public void ShowCharacter()
    {
        questsScreen.SetActive(false);
        pokedexScreen.SetActive(false);
        characterScreen.SetActive(true);

        if (characterScreenManager != null)
            characterScreenManager.LoadFromSession();
    }
}
