using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
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

    [SerializeField] private Image skillImageBlock;
    [SerializeField] private Image skillImageDoubleDamage;
    [SerializeField] private Image skillImageHeal;
    
    [SerializeField] private List<SupportSkill> supportSkills = new List<SupportSkill>();
    private bool isBlocking = false;
    private bool doubleDamageNextAttack = false;
    public Text levelPlayerText;

    private SkillType currentPromptedSkill;
    private bool isWaitingForBuyInput = false;

    public SkillType blockSkill;
    public SkillType doubleDameSkill;
    public SkillType healSkill;


    public int money = 0;
    public int experience = 0;
    [SerializeField] GameObject moneyText;

    public Text textNumberBlocksk;
    public Text textNumberDoublesk;
    public Text textNumberHealsk;

    BattleState state;
    int currAction;
    int currMove;
    int currAnswer;
    int randomQuestion;
    bool correct;
    public event Action<bool> onBattleOver;
    int escapeAttempts;

    Collider2D Collision;

    private void Start()
    {
        if (player != null && player.Monster != null)
        {
            int playerLevel = player.level;
            Debug.Log($"Level của người chơi: {playerLevel}");
            levelPlayerText.text = "Lvl " + playerLevel.ToString();
        }
        else
        {
            Debug.LogWarning("Player hoặc player.Monster chưa được khởi tạo!");
        }
    }

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
    // public void AddExperience(int amount)
    // {
    //     if (player != null && player.Monster != null)
    //     {
    //         player.Monster.EXP += amount;
    //         if (playerHUD != null)
    //         {
    //             StartCoroutine(playerHUD.UpdateEXP(player.Monster));
    //         }
    //     }
    // }

    

    public void StartBattle(MonsterBase Enemy, Monster Player, Collider2D collision){
        Collision = collision;
        enemy._base = Enemy;
        player.Monster = Player;
        int enemyLevel;


        if (player.level <= 5)
        {
            enemyLevel = player.level + Random.Range(0, 2); 
        }
        else
        {
            enemyLevel = player.level + (Random.Range(0, 2) == 0 ? 0 : 1);
        }

        enemyLevel = Mathf.Max(1, enemyLevel);

        Monster enemyMonster = new Monster(Enemy, enemyLevel);


        GameObject blockSkillObj = GameObject.Find("BlockSkill");
        if (blockSkillObj != null)
            skillImageBlock = blockSkillObj.GetComponent<Image>();

        GameObject doubleDamageSkillObj = GameObject.Find("DoubleDamageSkill");
        if (doubleDamageSkillObj != null)
            skillImageDoubleDamage = doubleDamageSkillObj.GetComponent<Image>();

        GameObject healSkillObj = GameObject.Find("HealSkill");
        if (healSkillObj != null)
            skillImageHeal = healSkillObj.GetComponent<Image>();

        // Thêm kỹ năng vào danh sách
        supportSkills.Add(new SupportSkill("Phòng Thủ", SkillType.Block, "Chặn 1 đòn tấn công từ enemy", 3, 50, 5f, skillImageBlock));
        supportSkills.Add(new SupportSkill("Nhân đôi sát thương", SkillType.DoubleDamage, "X2 sát thương cho lượt kế tiếp", 3, 100, 10f, skillImageDoubleDamage));
        supportSkills.Add(new SupportSkill("Hồi HP", SkillType.Heal, "Hồi lại HP hiện tại x2", 3, 75, 8f, skillImageHeal));
        
        StartCoroutine(SetupBattle(enemyMonster, Player));

        AudioManager.i.PlayMusic(wildBattleMusic);
    }

    public IEnumerator SetupBattle(Monster Enemy, Monster Player){
        player.Setup(Player);
        playerHUD.SetData(player.Monster, characterShopDatabase, true);
        enemy.Setup(Enemy);
        enemyHUD.SetData(enemy.Monster,null, false);

        int playerLevel = player.level;
        Debug.Log($"Player level sau khi Setup: {playerLevel}");
        levelPlayerText.text = "Lvl " + playerLevel.ToString();

        //Debug.Log(player.Monster.HP);
        Debug.Log("Setting up battle");
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

    public float GetTypeMul(MonsterType monsterType)
    {
        switch (monsterType)
        {
            case MonsterType.Tree or MonsterType.Sort or MonsterType.Graph:
                return 10.0f;
            case MonsterType.DynamicProgramming:
                return 8.0f;
            default:
                return 4.0f;
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

        //bool isFainted = enemy.Monster.TakeDamage(move, player.Monster, correct, bonusDmg);

        float finalDamage = doubleDamageNextAttack ? bonusDmg * 2 : bonusDmg;
        doubleDamageNextAttack = false;
        bool isFainted = enemy.Monster.TakeDamage(move, player.Monster, correct, finalDamage);

        StartCoroutine(enemyHUD.UpdateHP(enemy.Monster));
        if (isFainted) {
            yield return dialogBox.TypeDialog($"{enemy.Monster.Base.Name} đã ngất");
            enemy.PlayFaintAnimation();
            if(Collision != null){
                Collision.gameObject.SetActive(false);
            }
            
            //gaining exp and level up
            //-------------------------------------------------
            int expYield = player.Monster.Base.ExperienceYield;
            Debug.Log($"Player expYield {expYield}");
            int enemyLv = enemy.Monster.Level;
            Debug.Log($"enemy level{enemyLv}");
            //float expGain = Mathf.FloorToInt((expYield * enemyLv * 10) / (enemyLv - 3));
            float typeMultiplier = GetTypeMul(enemy.Monster.Base.Type);
            float balanceFactor = 1.0f;
            float expGain = Mathf.FloorToInt(expYield * enemyLv * typeMultiplier * balanceFactor * 4);
            //Debug.Log($"Exp gained {expGain}");
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
                Debug.Log("truoc " + player.Monster.Attack);
                player.level += 1;
                player.Monster.LevelUp();
                Debug.Log("sau " + player.Monster.Attack);

                Debug.Log($"PLayer level {player.level}");
                levelPlayerText.text = "Lvl " + player.level.ToString();

                yield return dialogBox.TypeDialog($"Người chơi được tăng lên level {player.level}");
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
            
            
            if (enemy.Monster.Level > player.level)
            {
                money += enemy.Monster.Money;
            }
            else
            {
                float tempRadio = (float)enemy.Monster.Level / player.level;
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

        //bool isFainted = player.Monster.TakeDamage(move, enemy.Monster, lucky, 1);
        //yield return dialogBox.TypeDialog(lucky ? $"{enemy.Monster.Base.Name} đã trúng bạn!!!" : $"{enemy.Monster.Base.Name} đã trượt.");
        bool isFainted = false; 

        if (lucky)
        {
            if (isBlocking)
            {
                yield return dialogBox.TypeDialog("Bạn đã chặn thành công đòn tấn công!");
                isBlocking = false; // Reset trạng thái blocking sau khi chặn thành công
            }
            else
            {
                isFainted = player.Monster.TakeDamage(move, enemy.Monster, lucky, 1);
                yield return dialogBox.TypeDialog($"{enemy.Monster.Base.Name} đã trúng bạn!!!");
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{enemy.Monster.Base.Name} đã trượt.");
        }
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
            yield return dialogBox.TypeDialog($"Bạn đã thua. BẠN SẼ MẤT 1/3 SỐ TIỀN, TRỪ 1 CẤP VÀ BYE!!!");
            player.PlayFaintAnimation();
            player.level -= 1;
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
        if (EventSystem.current.currentSelectedGameObject != null) return; // UI đang được focus, không xử lý input

        
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

    // public void UseSupportSkill(SkillType skillType)
    // {
    //     SupportSkill skill = supportSkills.Find(s => s.skillType == skillType);

    //     if (skill != null && skill.CanUse())
    //     {
    //         skill.UseSkill(() =>
    //         {
    //             switch (skillType)
    //             {
    //                 case SkillType.Block:
    //                     isBlocking = true;
    //                     StartCoroutine(dialogBox.TypeDialog("Bạn đã kích hoạt phòng thủ!"));
    //                     break;

    //                 case SkillType.DoubleDamage:
    //                     doubleDamageNextAttack = true;
    //                     StartCoroutine(dialogBox.TypeDialog("Lần tấn công kế tiếp sẽ x2 sát thương!"));
    //                     break;

    //                 case SkillType.Heal:
    //                     int healAmount = player.Monster.HP * 2;
    //                     player.Monster.HP = Mathf.Min(player.Monster.MaxHP, healAmount);
    //                     StartCoroutine(playerHUD.UpdateHP(player.Monster));
    //                     StartCoroutine(dialogBox.TypeDialog("Bạn đã hồi HP!"));
    //                     break;
    //             }
    //         }, this); // Thêm `this` nếu UseSkill yêu cầu tham số MonoBehaviour
    //     }
    //     else
    //     {
    //         StartCoroutine(dialogBox.TypeDialog("Không thể sử dụng kỹ năng này!"));
    //     }
    // }

    public void GetUpdateNumber(Text textNumber, SkillType skillType)
    {
        SupportSkill skill = supportSkills.Find(s => s.skillType == skillType);
        textNumber.text = $"{skill.uses}";
    }
    public void UseSupportSkill(SkillType skillType)
    {
        SupportSkill skill = supportSkills.Find(s => s.skillType == skillType);

        if (skill != null && skill.CanUse())
        {
            skill.UseSkill(() =>
            {
                string alertMessage = "";

                switch (skillType)
                {
                    case SkillType.Block:
                        isBlocking = true;
                        alertMessage = $"Bạn đã kích hoạt phòng thủ!\n(Số lần còn lại: {skill.uses})";
                        break;

                    case SkillType.DoubleDamage:
                        doubleDamageNextAttack = true;
                        alertMessage = $"Lần tấn công kế tiếp sẽ x2 sát thương!\n(Số lần còn lại: {skill.uses})";
                        break;

                    case SkillType.Heal:
                        int healAmount = player.Monster.HP * 2;
                        player.Monster.HP = Mathf.Min(player.Monster.MaxHP, healAmount);
                        StartCoroutine(playerHUD.UpdateHP(player.Monster));
                        alertMessage = $"Bạn đã hồi HP!\n(Số lần còn lại: {skill.uses})";
                        break;
                }

                AlertManager.Instance.ShowAlert(alertMessage);
            }, this);
        }
        else
        {
            AlertManager.Instance.ShowAlert(
                $"\n{skill?.skillName} đã hết lượt hoặc đang hồi chiêu.\nNhấn [X] để mua ({skill?.cost} vàng).");

            // Gợi ý: xử lý phím X để mua kỹ năng này
            currentPromptedSkill = skillType; // Lưu lại kỹ năng để mua sau nếu người dùng bấm X
            isWaitingForBuyInput = true;
        }
    }

    public void BuySkill(SkillType skillType)
    {
        SupportSkill skill = supportSkills.Find(s => s.skillType == skillType);

        if (skill != null && money >= skill.cost)
        {
            money -= skill.cost;
            skill.uses++;
            UpdateMoneyUI();
            AlertManager.Instance.ShowAlert($"Đã mua {skill.skillName}!\n(Số lần còn lại: {skill.uses})");
        }
        else
        {
            AlertManager.Instance.ShowAlert("Không đủ tiền để mua!");
        }
    }

    // public void RewardSkill(SkillType skillType)
    // {
    //     SupportSkill skill = supportSkills.Find(s => s.skillType == skillType);
    //     skill.uses++;
    //     AlertManager.Instance.ShowAlert($"Đã nhận được kỹ năng {skill.skillName}!\n(Số lần hiện tại: {skill.uses})");
    // }


    void handlePlayerMove()
    {
        if (EventSystem.current.currentSelectedGameObject != null) return; // UI đang được focus, không xử lý input

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
        if (EventSystem.current.currentSelectedGameObject != null) return; 

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
    void Update()
    {
        GetUpdateNumber(textNumberBlocksk, blockSkill);
        //Debug.Log("1" + currentPromptedSkill);
        GetUpdateNumber(textNumberHealsk, healSkill);
        //Debug.Log("2" +  currentPromptedSkill);
        GetUpdateNumber(textNumberDoublesk, doubleDameSkill);
        //Debug.Log("3" + currentPromptedSkill);

        if (state == BattleState.PlayerAction || state == BattleState.PlayerMove || state == BattleState.QuestionAnswer)
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        HandleUpdate();

        if (isWaitingForBuyInput && Input.GetKeyDown(KeyCode.X))
        {
            BuySkill(currentPromptedSkill);
            isWaitingForBuyInput = false;
        }

    }

}
