using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy, QuestionAnswer};

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit player;
    [SerializeField] BattleUnit enemy;
    [SerializeField] BattleHUD playerHUD;
    [SerializeField] BattleHUD enemyHUD;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] MonsterQuestion monsterQuestion;
    [SerializeField] CharacterShopDatabase characterShopDatabase;
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    [SerializeField] Movement playerMovement;

    public int money = 0;
    [SerializeField] GameObject moneyText;

    BattleState state;
    int currAction;
    int currMove;
    int currAnswer;
    int randomQuestion;
    bool correct;
    public event Action<bool> onBattleOver;
    int escapeAttempts;

    Collider2D Collision;


    public void SetMoney(int newMoney)
    {
        money = newMoney;
        UpdateMoneyUI();
    }
    void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            Text textComponent = moneyText.GetComponent<Text>();
            if (textComponent != null) { 
                textComponent.text = money.ToString();
            }
            else
            {
                Debug.LogWarning("Không tìm thấy component Text trên moneyTextObject.");
            }
        }
        else
        {
            Debug.LogWarning("moneyTextObject chưa được gán trong Inspector.");
        }
    }

    public void StartBattle(MonsterBase Enemy, Monster Player, Collider2D collision){
        Collision = collision;
        enemy._base = Enemy;
        player.Monster = Player;
        StartCoroutine(SetupBattle(new Monster(Enemy, Player.Level <= 5 ? Player.Level + Random.Range(0, 6): (Random.Range(0, 2) == 0 ? Player.Level + Random.Range(0, 6): Player.Level - Random.Range(0, 6))), Player));

        AudioManager.i.PlayMusic(wildBattleMusic);
    }

    public IEnumerator SetupBattle(Monster Enemy, Monster Player){
        player.Setup(Player);
        playerHUD.SetData(player.Monster, characterShopDatabase);
        enemy.Setup(Enemy);
        enemyHUD.SetData(enemy.Monster, null);

        //Debug.Log(player.Monster.HP);

        dialogBox.SetMoveNames(player.Monster.Moves);
        yield return dialogBox.TypeDialog($"Xuất hiện quái vật {enemy.Monster.Base.Name}");
        yield return new WaitForSeconds(1f);

        escapeAttempts = 0;

        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Chọn hành động"));
        dialogBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableAnswerSelector(false);
        dialogBox.EnableMoveSelector(true);
    }

    void QuestionAnswer()
    {
        state = BattleState.QuestionAnswer;
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(false);
        monsterQuestion.gameObject.SetActive(true);
        monsterQuestion.EnableMonsterQuestion(true);
        dialogBox.EnableAnswerSelector(true);
        randomQuestion = Random.Range(0, enemy.Monster.Questions.Count);
        monsterQuestion.setUpMonsterQuestion(enemy.Monster.Questions[randomQuestion].Base.Question);
        dialogBox.SetAnswerDialog(enemy.Monster.Questions[randomQuestion].Base.Answers);
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            handlePlayerAction();
        }
        else if (state == BattleState.PlayerMove)
        {
            handlePlayerMove();
        }
        else if (state == BattleState.QuestionAnswer) {
            handlePlayerQuestionAnswer();
        }
    }

    IEnumerator PerformPlayerMove(bool correct, float bonusDmg)
    {
        state = BattleState.Busy;
        var move = player.Monster.Moves[currMove];
        if (correct)
        {
            yield return dialogBox.TypeDialog($"{player.Monster.Base.Name} sử dụng {move.Base.Name} gây toàn bộ sát thương");
        }
        else
        {
            yield return dialogBox.TypeDialog($"{player.Monster.Base.Name} sử dụng {move.Base.Name}, nhưng trả lời sai nên không sát thương");
        }
        player.PlayerAttackAnimation();
        AudioManager.i.PlaySfx(move.Base.Sound, true);
        Debug.Log(move.Base.Sound);

        if (correct)
        {
            enemy.PlayHitAnimation();
            AudioManager.i.PlaySfx(AudioId.Hit, true);
            Debug.Log(AudioId.Hit);
        }
        yield return new WaitForSeconds(1f);

        bool isFainted = enemy.Monster.TakeDamage(move, player.Monster, correct, bonusDmg);
        StartCoroutine(enemyHUD.UpdateHP(enemy.Monster));
        if (isFainted) {
            yield return dialogBox.TypeDialog($"{enemy.Monster.Base.Name} đã ngất");
            enemy.PlayFaintAnimation();
            if(Collision != null){
                Collision.gameObject.SetActive(false);
            }

            AudioManager.i.PlayMusic(battleVictoryMusic);



            if (enemy.Monster.Level > player.Monster.Level)
            {
                money += enemy.Monster.Money;
            }
            else
            {
                float tempRadio = (float)enemy.Monster.Level / player.Monster.Level;
                money += (Mathf.FloorToInt(enemy.Monster.Money * tempRadio));
            }
            UpdateMoneyUI();

            PlayerPrefs.SetInt("Money", money);

            yield return new WaitForSeconds(2f);
            onBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemy.Monster.GetRandomMove();
        yield return dialogBox.TypeDialog($"{enemy.Monster.Base.Name} đang sử dụng {move.Base.Name}");
        yield return new WaitForSeconds(1f);

        bool lucky = Random.Range(0, 100) <= 70;

        bool isFainted = player.Monster.TakeDamage(move, enemy.Monster, lucky, 1);
        yield return dialogBox.TypeDialog(lucky ? $"{enemy.Monster.Base.Name} đã trúng bạn!!!" : $"{enemy.Monster.Base.Name} đã trượt.");
        yield return new WaitForSeconds(1f);
        enemy.PlayerAttackAnimation();
        AudioManager.i.PlaySfx(move.Base.Sound, true);
        Debug.Log(move.Base.Sound);
        if (lucky)
        {
            player.PlayHitAnimation();
            AudioManager.i.PlaySfx(AudioId.Hit, true);
            Debug.Log(AudioId.Hit);

        }
        yield return new WaitForSeconds(1f);
        StartCoroutine(playerHUD.UpdateHP(player.Monster));
        if (isFainted) {
            yield return dialogBox.TypeDialog($"Bạn đã thua. BẠN SẼ MẤT 1/3 SỐ TIỀN VÀ BYE!!!");
            player.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
            onBattleOver(false);
        }
        else
        {
            PlayerAction();
        }
    }

    void handlePlayerQuestionAnswer()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currAnswer < enemy.Monster.Questions[randomQuestion].Base.Answers.Count - 2)
            {
                currAnswer += 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currAnswer > 1)
            {
                currAnswer -= 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currAnswer < enemy.Monster.Questions[randomQuestion].Base.Answers.Count - 1)
            {
                ++currAnswer;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currAnswer > 0)
            {
                --currAnswer;
            }
        }

        dialogBox.UpdateAnswerSelection(currAnswer);
        if (Input.GetKeyDown(KeyCode.Z))
        {
            AudioManager.i.PlaySfx(AudioId.UISelect);
            float multiplyDame = monsterQuestion.timer.timerValue;
            correct = enemy.Monster.Questions[randomQuestion].Base.Answers[currAnswer].correctAnswer;
            monsterQuestion.EnableMonsterQuestion(false);
            monsterQuestion.gameObject.SetActive(false);
            dialogBox.EnableAnswerSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove(correct, multiplyDame));
        }
    }

    void handlePlayerMove()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(currMove < player.Monster.Moves.Count - 2)
            {
                currMove += 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(currMove > 1)
            {
                currMove -= 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if(currMove < player.Monster.Moves.Count - 1)
            {
                ++currMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if(currMove > 0)
            {
                --currMove;
            }
        }

        dialogBox.UpdateMoveSelection(currMove, player.Monster.Moves[currMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            AudioManager.i.PlaySfx(AudioId.UISelect);
            QuestionAnswer();
        }
    }

    void handlePlayerAction()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            AudioManager.i.PlaySfx(AudioId.UISelect);
            if (currAction < 1)
            {
                ++currAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AudioManager.i.PlaySfx(AudioId.UISelect);
            if (currAction > 0)
            {
                --currAction;
            }
        }
        dialogBox.UpdateActionSelection(currAction);
        if (Input.GetKeyDown(KeyCode.Z))
        {
            AudioManager.i.PlaySfx(AudioId.UISelect);
            if (currAction == 0)
            {
                //Fight
                PlayerMove();
            }
            else if( currAction == 1)
            {
                //Run
                StartCoroutine(TryToEscape());
            }
        }
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        int playerSpeed = player.Monster.Speed;
        int enemySpeed = enemy.Monster.Speed;
        ++escapeAttempts;
        if(enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Chạy trốn an toàn !");
            onBattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if(Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Chạy trốn an toàn !");
                dialogBox.EnableActionSelector(false);
                onBattleOver(true);
                playerMovement.transform.Translate(Vector3.up*0.5f);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Không thể chạy trốn");
                state = BattleState.PlayerAction;
                PlayerAction();
            }
        }
    }
}
