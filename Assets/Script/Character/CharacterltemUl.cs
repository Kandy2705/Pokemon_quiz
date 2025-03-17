using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.SceneManagement;

public class CharacterltemUl : MonoBehaviour
{
    public Text moneyText;

    [SerializeField] Image characterImg;
    [SerializeField] Text characterNameText;
    [SerializeField] Text characterIntroduction;
    [SerializeField] Text characterPriceText;
    [SerializeField] Button characterPurchaseButton;
    [SerializeField] Button characterLeftButton;
    [SerializeField] Button characterRightButton;

    [SerializeField] CharacterShopDatabase shopDatabase;

    private int currentIndex = 0;

    private void Start()
    {
        characterLeftButton.onClick.AddListener(OnLeftButtonClicked);
        characterRightButton.onClick.AddListener(OnRightButtonClicked);
        characterPurchaseButton.onClick.AddListener(OnPurchaseButtonClicked);

        UpdataCharacterUI();
    }

    private void Update()
    {
        
    }

    private void OnPurchaseButtonClicked()
    {
        int money = PlayerPrefs.GetInt("Money", 0);
        Character character = shopDatabase.GetCharacter(currentIndex);
        if (character.isPurchased)
        {
            DeselectAllCharacters();
            character.isSelected = true;
            characterPriceText.text = "Selected";
            characterPurchaseButton.GetComponent<Image>().color = Color.blue;
        }
        else
        {
            if (money >= character.price && !character.isPurchased)
            {
                money -= character.price;
                PlayerPrefs.SetInt("Money", money);
                int temp = PlayerPrefs.GetInt("Money", 0);
                UpdateMoneyUI(money);
                character.isPurchased = true;
                DeselectAllCharacters();
                character.isSelected = true;
                characterPriceText.text = "Selected";
                characterPurchaseButton.GetComponent<Image>().color = Color.blue;
            }
        }

        UpdataCharacterUI();
    }

    private void DeselectAllCharacters()
    {
        for (int i = 0; i < shopDatabase.CharactersCount; i++)
        {
            shopDatabase.GetCharacter(i).isSelected = false;
        }
    }
    void UpdateMoneyUI(int money)
    {
        if (moneyText != null)
        {
            moneyText.text = money.ToString();
        }
        else
        {
            Debug.LogWarning("Ch?a gán moneyText trong Inspector!");
        }
    }

    private void OnLeftButtonClicked()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = shopDatabase.CharactersCount - 1;
        }

        UpdataCharacterUI();
    }

    private void OnRightButtonClicked()
    {
        currentIndex++;
        if (currentIndex >= shopDatabase.CharactersCount) { 
            currentIndex = 0;
        }

        UpdataCharacterUI();
    }

    private void UpdataCharacterUI()
    {
        Character character = shopDatabase.GetCharacter(currentIndex);
        characterImg.sprite = character.image;
        characterNameText.text = character.name;
        characterIntroduction.text = character.introduction;
        characterPriceText.text = character.price.ToString();
        if (character.isSelected)
        {
            characterPriceText.text = "Selected";
            characterPurchaseButton.GetComponent<Image>().color = Color.blue;
        }
        else
        {
            if (character.isPurchased)
            {
                characterPriceText.text = "Select";
                characterPurchaseButton.GetComponent<Image>().color = Color.green;
            }
            else
            {
                Color newColor;
                if (ColorUtility.TryParseHtmlString("#9C7962", out newColor))
                {
                    characterPurchaseButton.GetComponent<Image>().color = newColor;
                }
            }
        }
    }
}
