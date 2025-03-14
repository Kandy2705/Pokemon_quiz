using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    public void SetData(Monster monster){
        nameText.text = monster.Base.Name;
        levelText.text = "Lvl " + monster.Level;
        hpBar.SetHP((float)monster.HP / monster.MaxHP);
    }

    public IEnumerator UpdateHP(Monster monster)
    {
        yield return hpBar.SetHPSmooth((float)monster.HP / monster.MaxHP);
    }
}