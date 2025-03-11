using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] float timeToBonusDamage = 30f;
    public float timerValue;
    
    void OnEnable()
    {
        timerValue = timeToBonusDamage;
    }
    void Update()
    {
        UpdateTimer();
    }

    void UpdateTimer()
    {
        timerValue -= Time.deltaTime;
        if (timerValue <= 0)
        {
            timerValue = 0;
        }
        this.GetComponent<Image>().fillAmount = timerValue / timeToBonusDamage;
    }
}
