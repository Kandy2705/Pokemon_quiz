using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public GameObject uiShop;
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
    }

}
