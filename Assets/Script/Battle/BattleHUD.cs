using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] EXPBar expBar;
    private float previousExp = -1;

<<<<<<< Updated upstream
    public void SetData(Monster monster){
        nameText.text = monster.Base.Name;
        levelText.text = "Lvl " + monster.Level;
        hpBar.SetHP((float)monster.HP / monster.MaxHP);
    }
=======
    public void SetData(Monster monster, CharacterShopDatabase shopDatabase, bool isPlayer)
    {
        if (shopDatabase == null)
        {
            Debug.Log("if shop database is null");
            nameText.text = monster.Base.Name;
        }
        else
        {
            Debug.Log("if shop database is not null");
            Debug.Log(shopDatabase.characters);
            nameText.text = shopDatabase.GetNamePlayer();
        }
>>>>>>> Stashed changes

        Debug.Log($"Player Level: {monster.Level}");
        Debug.Log($"Monster level: {monster.Level}");
        levelText.text = "Lvl " + monster.Level;
        Debug.Log($"Setting monster level: Lvl {monster.Level}");

        float hpPercentage = (float)monster.HP / monster.MaxHP;
        hpBar.SetHP(hpPercentage);
        Debug.Log($"Setting HP bar: {hpPercentage * 100}%");
        if (isPlayer && expBar != null)
        {
            float newExp = GetNormalizedExp(monster);
            Debug.Log($"New exp: {newExp}");
            if (previousExp < 0) previousExp = newExp; // Gán lần đầu
            if (Mathf.Abs(previousExp - newExp) > 0.01f)
            {
                expBar.SetExp(newExp);
                previousExp = newExp; // Cập nhật giá trị mới
            }
            Debug.Log($"Current exp: {previousExp}");
        }
    }
    float GetNormalizedExp(Monster monster)
    {
        int currLevelExp = monster.Base.getExperienceForLevel(monster.Level);
        float nextLevelExp = monster.Base. getExperienceForLevel(monster.Level + 1);
        float normalizedExp = (float) (monster.EXP - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }
    public IEnumerator UpdateHP(Monster monster)
    {
        float hpPercentage = (float)monster.HP / monster.MaxHP;
        Debug.Log($"Updating HP bar to: {hpPercentage * 100}%");
        yield return hpBar.SetHPSmooth(hpPercentage);
    }
    public IEnumerator UpdateEXP(Monster monster)
    {

        float expPercent = (float)monster.EXP / 100;
        float remainingExp = monster.MaxEXP - monster.EXP;
        Debug.Log($"Remaining EXP: {remainingExp}");
        Debug.Log($"Updating EXP bar to: {expPercent * 100}%");
        yield return expBar.SetExpSmooth(expPercent);

        // monster.EXP -= monster.MaxEXP;
        // Debug.Log($"Updating EXP bar to: {monster.EXP * 100}%");
        // yield return expBar.SetExpSmooth(monster.EXP);
    }
}

