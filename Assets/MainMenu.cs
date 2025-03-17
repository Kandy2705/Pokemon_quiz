using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject uiShop;
    public Text moneyText;

    [SerializeField] Text characterNameText;
    [SerializeField] Image characterImg;

    [SerializeField] CharacterShopDatabase shopDatabase;

    private int selectedIndex = 0;
    private void Start()
    {
        int money = PlayerPrefs.GetInt("Money", 0);
        UpdateMoneyUI(money);

        for (int i = 0; i < shopDatabase.CharactersCount; i++) {
            Character character = shopDatabase.GetCharacter(i);
            if (character.isSelected)
            {
                selectedIndex = i;
                break;
            }
        }

        UpdateCharacterUI();
    }

    private void Update()
    {
        int money = PlayerPrefs.GetInt("Money", 0);
        UpdateMoneyUI(money);

        for (int i = 0; i < shopDatabase.CharactersCount; i++)
        {
            Character character = shopDatabase.GetCharacter(i);
            if (character.isSelected)
            {
                selectedIndex = i;
                break;
            }
        }

        UpdateCharacterUI();
    }

    public void PlayGame(){
        SceneManager.LoadSceneAsync(1);

    }
    public void ExitGame(){
        Application.Quit();
    }

    public void ShowUIShop()
    {
        uiShop.SetActive(true);
    }

    public void HideUIShop()
    {
        uiShop.SetActive(false);
        UpdateCharacterUI();
    }

    void UpdateMoneyUI(int money)
    {
        if (moneyText != null)
        {
            moneyText.text = money.ToString();
        }
        else
        {
            Debug.LogWarning("Chua gán moneyText trong Inspector!");
        }
    }

    void UpdateCharacterUI()
    {
        Character selectedCharacter = shopDatabase.GetCharacter(selectedIndex);

        if (characterNameText != null)
        {
            characterNameText.text = selectedCharacter.name;
        }

        if (characterImg != null)
        {
            characterImg.sprite = selectedCharacter.image;
        }
    }

}
