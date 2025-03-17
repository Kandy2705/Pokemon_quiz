using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
    [SerializeField] private GameObject moneyObject;
    [SerializeField] private GameObject buttonStop;

    private void Start()
    {
        int savedMoney = PlayerPrefs.GetInt("Money", 0);
        battleSystem.SetMoney(savedMoney);
        moneyObject.SetActive(true);
        buttonStop.SetActive(true);

        playerController.onEncountered += StartBattle;
        battleSystem.onBattleOver += EndBattle;

        DialogManager.Instance.OnShowDialog += () =>
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
        moneyObject.SetActive(true);
    }

    void StartBattle(MonsterBase Enemy, Monster Player, Collider2D Collision)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        moneyObject.SetActive(false);

        battleSystem.StartBattle(Enemy, Player, Collision);
    }

    private void Update()
    {
        if (battleSystem.isActiveAndEnabled)
        {
            moneyObject.SetActive(false);
            buttonStop.SetActive(false);
        }
        else
        {
            moneyObject.SetActive(true);
            buttonStop.SetActive(true);
        }
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
