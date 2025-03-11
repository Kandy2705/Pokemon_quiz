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
        StartCoroutine(SetupBattle(new Monster(Enemy, Player.Level <= 5 ? Player.Level + Random.Range(0, 6): (Random.Range(0, 2) == 0 ? Player.Level + Random.Range(0, 6): Player.Level - Random.Range(0, 6))), Player));
    }

    public IEnumerator SetupBattle(Monster Enemy, Monster Player){
        player.Setup(Player);
        playerHUD.SetData(player.Monster);
        enemy.Setup(Enemy);
        enemyHUD.SetData(enemy.Monster);

        Debug.Log(player.Monster.HP);

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
            yield return dialogBox.TypeDialog($"You are dead. BYE BYE !!!");
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
