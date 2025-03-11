using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] public MonsterBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayer;

    Image image;
    Vector3 originalPos;
    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public Monster Monster {get; set;}

    public void Setup(Monster monster){
        Monster = monster;
        if(isPlayer){
            image.sprite = Monster.Base.BackSprite;
        }
        else{
            image.sprite = Monster.Base.FrontSprite;
            
        }
        image.color = originalColor;
        PlayeEnterAnimation();
    }

    public void PlayeEnterAnimation()
    {
        if (!isPlayer)
        {
            image.transform.localPosition = new Vector3(-216f, originalPos.y);
        }
        else
        {
            image.transform.localPosition = new Vector3(217f, originalPos.y);
        }
        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    public void PlayerAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayer)
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        }
        else
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));
        }
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }
}
