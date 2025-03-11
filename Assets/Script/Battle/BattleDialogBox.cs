using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int letterPerSecond;
    [SerializeField] Color highlightColor;
    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject answerSelector;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;
    [SerializeField] List<Text> answerTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text TypeText;

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach(var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/letterPerSecond);
        }
    }

    public void changeText(string dialog)
    {
        dialogText.text = dialog;
    }

    public void EnableDialogText(bool enable)
    {
       dialogText.enabled = enable;
    }

    public void EnableActionSelector(bool enable)
    {
        actionSelector.SetActive(enable);
    }

    public void EnableMoveSelector(bool enable)
    {
        moveSelector.SetActive(enable);
        moveDetails.SetActive(enable);
    }

    public void EnableAnswerSelector(bool enable)
    {
        answerSelector.SetActive(enable);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for(int i = 0; i < actionTexts.Count; i++)
        {
            if(i == selectedAction)
            {
                actionTexts[i].color = highlightColor;
            }
            else
            {
                actionTexts[i].color = Color.black;
            }
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for(int i = 0; i < moveTexts.Count; i++)
        {
            if(i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                moveTexts[i].text = "-";
            }
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for(int i = 0; i < moveTexts.Count; ++i)
        {
            if(i == selectedMove)
                moveTexts[i].color = highlightColor;
            else
                moveTexts[i].color = Color.black;
        }
        ppText.text = $"PP {move.PP}/{move.Base.Pp}";
        TypeText.text = move.Base.Type.ToString();
    }

    public void SetAnswerDialog(List<Answer> answers)
    {
        for(int i = 0; i < answerTexts.Count; ++i)
        {
            answerTexts[i].text = answers[i].answer;
        }
    }

    public void UpdateAnswerSelection(int selectedAnswer)
    {
        for(int i = 0; i < answerTexts.Count; ++i)
        {
            if(i == selectedAnswer)
            {
                answerTexts[i].color = highlightColor;
            }
            else
            {
                answerTexts[i].color = Color.black;
            }
        }
    }
}
