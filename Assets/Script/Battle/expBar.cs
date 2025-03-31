using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EXPBar : MonoBehaviour
{
    [SerializeField] GameObject expBar;

    public void SetExp(float expNormalized){
        if (expBar == null) return;
        expBar.transform.localScale = new Vector3(expNormalized, 1, 1);
    }

    public IEnumerator SetExpSmooth(float newExpNormalized)
    {
        if (expBar == null) yield break;
        yield return expBar.transform.DOScaleX(newExpNormalized, 1.5f).WaitForCompletion();
    }
}