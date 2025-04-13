using UnityEngine;

public class PokedexManager : MonoBehaviour
{
    public GameObject pokedexPanel;

    public void TogglePokedex()
    {
        bool isActive = pokedexPanel.activeSelf;
        pokedexPanel.SetActive(!isActive);
    }
}
