using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class MonsterQuestion : MonoBehaviour
{
    [SerializeField] Text questionText;
    [SerializeField] Image backgroundImage;
    [SerializeField] public Timer timer;

    public IEnumerator TypeDialog(string dialog)
    {
        questionText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            questionText.text += letter;
            yield return new WaitForSeconds(1f / 30);
        }
    }

    public void setQuestion(string question)
    {
        questionText.text = question;
    }

    public void EnableMonsterQuestion(bool enable)
    {
        questionText.enabled = enable;
        backgroundImage.enabled = enable;
        timer.gameObject.SetActive(enable);
    }

    public void setUpMonsterQuestion(string questionText)
    {
        setQuestion(questionText);
    }
}
