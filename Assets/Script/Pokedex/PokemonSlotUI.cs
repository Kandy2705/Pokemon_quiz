using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PokemonSlotUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    private MonsterBase currentMonster;
    private PokedexUI pokedexUI;

    public void Setup(MonsterBase monster, PokedexUI ui)
    {
        currentMonster = monster;
        pokedexUI = ui;
        
        if (icon != null)
            icon.sprite = monster.FrontSprite;
        if (nameText != null)
        {
            nameText.text = monster.Name;
            SetSelected(false);
        }
    }

    public void OnSlotClicked()
    {
        if (pokedexUI != null && currentMonster != null)
        {
            pokedexUI.DisplayPokemon(currentMonster);
            pokedexUI.SetSelectedSlot(this);
        }
    }

    public void SetSelected(bool selected)
    {
        if (nameText != null)
        {
            nameText.color = selected ? Color.green : Color.black;
        }
    }
}
