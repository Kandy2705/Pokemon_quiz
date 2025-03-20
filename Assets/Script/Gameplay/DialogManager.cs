using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] int letterPerSecond;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    private bool isShowing = false;
    private Coroutine typingCoroutine;

    private bool canAdvanceLine = false;
    private bool isDialogCompleted = false;

    public static DialogManager Instance{ get; private set; }
    private void Awake() {
        Instance = this; 
    }

    Dialog dialog;
    int currentLine = 0;
    bool isTyping;

    public IEnumerator ShowDialog(Dialog dialog){
        if (isShowing)
        {
            yield break;
        }
        
        isShowing = true;
        isDialogCompleted = false;
        OnShowDialog?.Invoke();

        this.dialog = dialog;
        currentLine = 0;
        dialogBox.SetActive(true);
        AudioManager.i.PlaySfx(AudioId.UISelect);

        if (typingCoroutine != null) { 
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeDialog(dialog.Lines[currentLine]));

        yield return new WaitUntil(() => isDialogCompleted);
        dialogBox.SetActive(false);
        OnCloseDialog?.Invoke();
        isShowing = false;
    }

    public void HandleUpdate(){

        if (!dialogBox.activeSelf || isTyping) return;

        if (Input.GetKeyUp(KeyCode.Z) && canAdvanceLine)
        {
            if (currentLine < dialog.Lines.Count - 1)
            {
                currentLine++;
                AudioManager.i.PlaySfx(AudioId.UISelect);
                canAdvanceLine = false;
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
            }
            else
            {
                isDialogCompleted = true;
            }
        }
    }

    public IEnumerator TypeDialog(string line)
    {
        isTyping = true;
        dialogText.text = "";
        foreach(var letter in line.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/letterPerSecond);
        }
        isTyping = false;
        canAdvanceLine = true;
        if (currentLine == dialog.Lines.Count) {
            yield return new WaitForSeconds(1f / letterPerSecond);
            isDialogCompleted = true;
        }

    }
}
