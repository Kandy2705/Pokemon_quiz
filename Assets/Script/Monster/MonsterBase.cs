using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster/Create new monster")]
public class MonsterBase : ScriptableObject
{
    //Monster info
    [SerializeField] string monsterName;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    [SerializeField] MonsterType type;

    //Monster base stat
    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;
    [SerializeField] List<LearnAbleMove> learnAbleMoves;
    [SerializeField] List<LearnAbleQuestion> learnAbleQuestions;

    //Function for access variable
    public string Name { get { return monsterName; } }
    public string Description { get { return description; } }
    public Sprite FrontSprite { get { return frontSprite; } }
    public Sprite BackSprite { get { return backSprite; } }
    public MonsterType Type { get { return type;} }
    public int MaxHP { get { return maxHP; } }
    public int Attack { get { return attack;} }
    public int Defense { get { return defense;} }
    public int SpAttack { get { return spAttack; } }
    public int SpDefense { get { return spDefense;} }
    public int Speed { get { return speed;} }
    public List<LearnAbleMove> LearnAbleMoves { get { return learnAbleMoves;} }
    
    public List<LearnAbleQuestion> LearnAbleQuestions { get { return learnAbleQuestions; } }
}

[System.Serializable]
public class LearnAbleMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base { get { return moveBase;} }
    public int Level { get { return level; } }
}

[System.Serializable]
public class LearnAbleQuestion
{
    [SerializeField] QuestionBase question;
    [SerializeField] int level;

    public QuestionBase Base { get { return question; } }
    public int Level { get { return level; } }
}

public enum MonsterType{
    None,
    List,
    Tree,
    Sort,
    Graph
};
