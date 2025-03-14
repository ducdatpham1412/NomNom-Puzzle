using UnityEngine;
using UnityEngine.UI;

public class SkillShop : MonoBehaviour {
    [Header("UI Element")]
    [SerializeField] Image SkillSprite;
    [SerializeField] GameObject Indicator;

    [Header("Stats")]
    public PlayerShopping Manager;
    public BaseSkill Skill;

    public void SetSkill(BaseSkill skill) {
        Skill = skill;
        SkillSprite.sprite = skill.SkillSprite;
        SkillSprite.preserveAspect = true;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void UnFocus() {
        Indicator.SetActive(false);
    }

    void OnClick() {
        Indicator.SetActive(true);
        Manager.SelectSkill(this);
        // TODO: Add sound effect
    }
}
