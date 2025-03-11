using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "Monster/Create new question")]
public class QuestionBase : ScriptableObject
{
    [TextArea]
    [SerializeField] string question;
    [SerializeField] MonsterType questionType;
    [SerializeField] List<Answer> answers;

    public string Question { get { return question; } }
    public MonsterType QuestionType { get { return questionType; } }
    public List<Answer> Answers { get { return answers; } }
}

[System.Serializable]
public class Answer
{
    [SerializeField] public bool correctAnswer;
    [TextArea]
    [SerializeField] public string answer;
}
