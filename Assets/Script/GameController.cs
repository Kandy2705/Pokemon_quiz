using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle, Dialog}
public class GameController : MonoBehaviour
{
    GameState state;
    [SerializeField] Movement playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] GameObject DeathScene;

    private void Start()
    {
<<<<<<< Updated upstream
        playerController.onEncountered += StartBattle;
        battleSystem.onBattleOver += EndBattle;

        DialogManager.Instance.OnShowDialog += () =>
=======
        Debug.Log("GameController::Start()");
        if (sceneMusic != null)
            AudioManager.i.PlayMusic(sceneMusic, true, true);
        int savedMoney = PlayerPrefs.GetInt("Money", 0);
        battleSystem.SetMoney(savedMoney);
        moneyObject.SetActive(true);
        buttonStop.SetActive(true); 
        
        playerController.onEncountered += StartBattle;
        battleSystem.onBattleOver += EndBattle;

        DialogManager.Instance.OnShowDialog += HandleShowDialog;

    }

    private void HandleShowDialog()
    {
        Debug.Log("GameController::HandleShowDialog()");
        if (!isTalkNPC)
>>>>>>> Stashed changes
        {
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
                state = GameState.FreeRoam;
        };
    }

    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    void StartBattle(MonsterBase Enemy, Monster Player, Collider2D Collision)
    {
        Debug.Log("GameController::StartBattle()");
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        battleSystem.StartBattle(Enemy, Player, Collision);
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            if (playerController.player.HP == 0)
            {
                DeathScene.SetActive(true);
                Destroy(playerController);
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    SceneManager.LoadScene("SampleScene");
                }
            }
            playerController.HandleUpdate();
        }
        else if(state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if(state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
    }
}
