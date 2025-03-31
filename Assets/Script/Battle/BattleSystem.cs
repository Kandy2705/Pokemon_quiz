using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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

    [SerializeField] Movement playerMovement;

<<<<<<< Updated upstream
=======
    public int money = 0;
    public int experience = 0;
    [SerializeField] GameObject moneyText;

>>>>>>> Stashed changes
    BattleState state;
    int currAction;
    int currMove;
    int currAnswer;
    int randomQuestion;
    bool correct;
    public event Action<bool> onBattleOver;
    int escapeAttempts;

    Collider2D Collision;

    public void StartBattle(MonsterBase Enemy, Monster Player, Collider2D collision){
        Collision = collision;
        enemy._base = Enemy;
        player.Monster = Player;
<<<<<<< Updated upstream
        StartCoroutine(SetupBattle(new Monster(Enemy, Player.Level <= 5 ? Player.Level + Random.Range(0, 6): (Random.Range(0, 2) == 0 ? Player.Level + Random.Range(0, 6): Player.Level - Random.Range(0, 6))), Player));
=======
        int enemyLevel;

        if (Player.Level <= 5)
        {
            enemyLevel = Player.Level + Random.Range(0, 2); 
        }
        else
        {
            enemyLevel = Player.Level + (Random.Range(0, 2) == 0 ? 0 : 1);
        }

        enemyLevel = Mathf.Max(1, enemyLevel);

        Monster enemyMonster = new Monster(Enemy, enemyLevel);
        
        StartCoroutine(SetupBattle(enemyMonster, Player));

        AudioManager.i.PlayMusic(wildBattleMusic);
>>>>>>> Stashed changes
    }

    public IEnumerator SetupBattle(Monster Enemy, Monster Player){
        player.Setup(Player);
<<<<<<< Updated upstream
        playerHUD.SetData(player.Monster);
        enemy.Setup(Enemy);
        enemyHUD.SetData(enemy.Monster);

        Debug.Log(player.Monster.HP);

=======
        playerHUD.SetData(player.Monster, characterShopDatabase, true);
        enemy.Setup(Enemy);
        enemyHUD.SetData(enemy.Monster,null, false);

        //Debug.Log(player.Monster.HP);
        Debug.Log("Setting up battle");
>>>>>>> Stashed changes
        dialogBox.SetMoveNames(player.Monster.Moves);
        yield return dialogBox.TypeDialog($"A monster {enemy.Monster.Base.Name} appear");
        yield return new WaitForSeconds(1f);

        escapeAttempts = 0;

        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
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
            yield return dialogBox.TypeDialog($"{player.Monster.Base.Name} used {move.Base.Name} with full damage");
        }
        else
        {
            yield return dialogBox.TypeDialog($"{player.Monster.Base.Name} used {move.Base.Name}, but you answered wrong so no damage");
        }
        player.PlayerAttackAnimation();
        if (correct) enemy.PlayHitAnimation();
        yield return new WaitForSeconds(1f);

        bool isFainted = enemy.Monster.TakeDamage(move, player.Monster, correct, bonusDmg);
        StartCoroutine(enemyHUD.UpdateHP(enemy.Monster));
        if (isFainted) {
            yield return dialogBox.TypeDialog($"{enemy.Monster.Base.Name} is fainted");
            enemy.PlayFaintAnimation();
            if(Collision != null){
                Collision.gameObject.SetActive(false);
            }
<<<<<<< Updated upstream
=======
            
            //gaining exp and level up
            //-------------------------------------------------
            int expYield = player.Monster.Base.ExperienceYield;
            Debug.Log($"Player expYield {expYield}");
            int enemyLv = enemy.Monster.Level;
            Debug.Log($"enemy level{enemyLv}");
            float expGain = Mathf.FloorToInt((expYield * enemyLv * 10) / (enemyLv - 3));
            Debug.Log($"Exp gained {expGain}");
            Debug.Log($"Currently exp {player.Monster.EXP}");
            float remainingEXP = player.Monster.MaxEXP - player.Monster.EXP;
            Debug.Log($"Remaining EXP: {remainingEXP}");
            float temp = 0;
            if (remainingEXP < expGain)
            {
                temp = (player.Monster.EXP + expGain) - player.Monster.MaxEXP;
                Debug.Log($"Temp EXP: {temp}");
                expGain -= temp;
                Debug.Log($"Remaining EXP: {remainingEXP}");
            }
            player.Monster.EXP += expGain;
            Debug.Log($"After get exp {player.Monster.EXP}");
            yield return dialogBox.TypeDialog($"Người chơi được cộng {expGain} kinh nghiệm");
            StartCoroutine(playerHUD.UpdateEXP(player.Monster));
            if (player.Monster.EXP == player.Monster.MaxEXP)
            {
                player.Monster.Level += 1;
                Debug.Log($"PLayer level {player.Monster.Level}");
                yield return dialogBox.TypeDialog($"Người chơi được tăng lên level {player.Monster.Level}");
                player.Monster.EXP = 0;
                if (temp != 0)
                {
                    player.Monster.EXP += temp;
                }
                StartCoroutine(playerHUD.UpdateEXP(player.Monster));
                if (player.Monster.HP < (player.Monster.MaxHP / 2))
                {
                    player.Monster.HP += (player.Monster.MaxHP / 4);
                    yield return dialogBox.TypeDialog($"Người chơi hiện tại có số máu dưới 50% nên được cộng thêm {(player.Monster.Level/4)} HP");
                    StartCoroutine(playerHUD.UpdateHP(player.Monster));
                }
            }
            Debug.Log($"Player exp {player.Monster.EXP}");
            //-------------------------------------------------
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
            
>>>>>>> Stashed changes
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
        yield return dialogBox.TypeDialog($"{enemy.Monster.Base.Name} is using {move.Base.Name}");
        yield return new WaitForSeconds(1f);

        bool lucky = Random.Range(0, 100) <= 70;

        bool isFainted = player.Monster.TakeDamage(move, enemy.Monster, lucky, 1);
        yield return dialogBox.TypeDialog(lucky ? $"{enemy.Monster.Base.Name} hit you !!!": $"{enemy.Monster.Base.Name} is miss.");
        yield return new WaitForSeconds(1f);
        enemy.PlayerAttackAnimation();
        if (lucky) player.PlayHitAnimation();
        yield return new WaitForSeconds(1f);
        StartCoroutine(playerHUD.UpdateHP(player.Monster));
        if (isFainted) {
<<<<<<< Updated upstream
            yield return dialogBox.TypeDialog($"You are dead. BYE BYE !!!");
=======
            yield return dialogBox.TypeDialog($"Bạn đã thua. BẠN SẼ MẤT 1/3 SỐ TIỀN, TRỪ 1 CẤP VÀ BYE!!!");
>>>>>>> Stashed changes
            player.PlayFaintAnimation();
            player.Monster.Level -= 1;
            player.Monster.EXP = 0;
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
            QuestionAnswer();
        }
    }

    void handlePlayerAction()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(currAction < 1)
            {
                ++currAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(currAction > 0)
            {
                --currAction;
            }
        }
        dialogBox.UpdateActionSelection(currAction);
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if(currAction == 0)
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
            yield return dialogBox.TypeDialog($"Ran away safely !");
            onBattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if(Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely !");
                dialogBox.EnableActionSelector(false);
                onBattleOver(true);
                playerMovement.transform.Translate(Vector3.up*0.5f);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape");
                state = BattleState.PlayerAction;
                PlayerAction();
            }
        }
    }
}
