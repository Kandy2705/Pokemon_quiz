using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.Animation;

public enum GameState { FreeRoam, Battle, Dialog }
public class GameController : MonoBehaviour
{
    GameState state;
    [SerializeField] Movement playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] GameObject DeathScene;
    [SerializeField] GameObject MinimAP;
    [SerializeField] private GameObject moneyObject;
    [SerializeField] private GameObject buttonStop;
    [SerializeField] CharacterShopDatabase shopDatabase;
    [SerializeField] GameObject spriteLibraryAsset;
    private SpriteLibrary spriteLibrary;
    public Character player;
    [SerializeField] AudioClip sceneMusic;
    [SerializeField] AudioClip endGameMusic;
    bool musicEndStarted = false;
    bool isTalkNPC = false;


    private void Start()
    {
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
        if (!isTalkNPC)
        {
            AudioManager.i.PlaySfx(AudioId.UISelect);
            state = GameState.Dialog;
            isTalkNPC = true;
        }
    }

    public void ChangeAudio(AudioClip clip)
    {
        AudioManager.i.PlayMusic(clip, true, true);
    }

    private void OnDestroy()
    {
        DialogManager.Instance.OnShowDialog -= HandleShowDialog;
    }

    void UpdateCharacterUI()
    {
        for (int i = 0; i < shopDatabase.CharactersCount; i++)
        {
            Character character = shopDatabase.GetCharacter(i);
            if (character.isSelected)
            {
                SpriteLibrary sl = spriteLibraryAsset.GetComponent<SpriteLibrary>();
                if (sl != null)
                {
                    sl.spriteLibraryAsset = character.spriteLibraryAsset;
                }
                break;
            }
        }

    }

    void EndBattle(bool won)
    {
        if (!won)
        {
            int currentMoney = PlayerPrefs.GetInt("Money", 0);
            int newMoney = currentMoney / 3;
            battleSystem.SetMoney(newMoney);
            PlayerPrefs.SetInt("Money", newMoney);
            MinimAP.gameObject.SetActive(false);
        }
        else
        {
            if (sceneMusic != null)
                AudioManager.i.PlayMusic(sceneMusic, true, true);
        }
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        moneyObject.SetActive(true);
        playerController.enabled = true;
    }

    void StartBattle(MonsterBase Enemy, Monster Player, Collider2D Collision)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        moneyObject.SetActive(false);
        playerController.enabled = false;

        battleSystem.StartBattle(Enemy, Player, Collision);
    }

    private void Update()
    {
        UpdateCharacterUI();
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
                if (!musicEndStarted)
                {
                    AudioManager.i.PlayMusic(endGameMusic, true, true);
                    musicEndStarted = true;
                }
                playerController.enabled = false;
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    musicEndStarted = false;
                    SceneManager.LoadScene("SampleScene");
                }
            }
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
    }
}
