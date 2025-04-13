using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PokedexUI : MonoBehaviour
{
    public GameObject pokemonSlotPrefab;
    public Transform contentParent;
    public Image detailIcon;
    public TextMeshProUGUI detailDescription;
    public GameObject borderDescription;
    private PokemonSlotUI selectedSlot;

    public void PopulatePokedex()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        MonsterBase[] allMonsters = Resources.LoadAll<MonsterBase>("Monsters");

        foreach (var monster in allMonsters)
        {
            GameObject slotGO = Instantiate(pokemonSlotPrefab, contentParent);
            PokemonSlotUI slotUI = slotGO.GetComponent<PokemonSlotUI>();

            if (slotUI != null)
            {
                slotUI.Setup(monster, this);
            }
        }
    }

    public void DisplayPokemon(MonsterBase monster)
    {
        if (monster == null)
            return;

        if (detailIcon != null)
        {
            detailIcon.enabled = true;
            detailIcon.sprite = monster.FrontSprite;
        }

        if (detailDescription != null)
        {
            detailDescription.text =
                $"<b>Name:</b> {monster.Name}\n" +
                $"<b>Type:</b> {monster.Type}\n" +
                $"<b>Stats:</b>\n" +
                $"HP: {monster.MaxHP}\n" +
                $"Attack: {monster.Attack}\n" +
                $"Defense: {monster.Defense}\n" +
                $"Sp. Attack: {monster.SpAttack}\n" +
                $"Sp. Defense: {monster.SpDefense}\n" +
                $"Speed: {monster.Speed}\n\n";
        }

        if (borderDescription != null)
            borderDescription.SetActive(true);
    }

    public void SetSelectedSlot(PokemonSlotUI newSelected)
    {
        if (selectedSlot != null)
            selectedSlot.SetSelected(false);

        selectedSlot = newSelected;

        if (selectedSlot != null)
            selectedSlot.SetSelected(true);
    }
}
