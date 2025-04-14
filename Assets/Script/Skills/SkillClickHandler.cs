using UnityEngine;
using UnityEngine.EventSystems;

public class SkillClickHandler : MonoBehaviour, IPointerClickHandler
{
    public SkillType skillType;
    private BattleSystem battleSystem;

    void Start()
    {
        battleSystem = FindObjectOfType<BattleSystem>();
        if (battleSystem == null)
        {
            Debug.LogError("⚠ Không tìm thấy BattleSystem trong Scene!");
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        // Bỏ focus khỏi UI
        

        if (battleSystem != null)
        {
            battleSystem.UseSupportSkill(skillType);
        }
        
        EventSystem.current.SetSelectedGameObject(null);
    }
}
