using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum SkillType { Block, DoubleDamage, Heal }

[System.Serializable]
public class SupportSkill
{
    public string skillName;
    public SkillType skillType;
    public string description;
    public int uses; // Số lần sử dụng còn lại
    public int cost; // Giá bán trong cửa hàng
    public float cooldownTime; // Thời gian hồi chiêu (giây)
    public bool isOnCooldown = false; // Kiểm tra trạng thái cooldown
    public Image skillImage; // Ảnh UI hiển thị cooldown

    public SupportSkill(string name, SkillType type, string desc, int maxUses, int skillCost, float cdTime, Image img)
    {
        skillName = name;
        skillType = type;
        description = desc;
        uses = maxUses;
        cost = skillCost;
        cooldownTime = cdTime;
        skillImage = img;

        if (skillImage != null)
        {
            skillImage.fillAmount = 1; // Ban đầu kỹ năng chưa bị cooldown
        }
        else
        {
            Debug.LogError($"Skill Image bị null cho kỹ năng {name}");
        }
    }


    public bool CanUse()
    {
        return uses > 0 && !isOnCooldown;
    }

    public void UseSkill(Action onSkillUsed, MonoBehaviour owner)
    {
        if (CanUse())
        {
            uses--;
            isOnCooldown = true;
            skillImage.fillAmount = 0; // Bắt đầu cooldown
            owner.StartCoroutine(CooldownRoutine());

            onSkillUsed?.Invoke();
        }
        else
        {
            Debug.Log($"{skillName} không thể sử dụng lúc này!");
        }
    }

    private IEnumerator CooldownRoutine()
    {
        float elapsedTime = 0;

        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime;
            skillImage.fillAmount = elapsedTime / cooldownTime;
            yield return null;
        }

        skillImage.fillAmount = 1;
        isOnCooldown = false;
    }
}
