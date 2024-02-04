using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillInfo : UIBase
{
    public GameObject go_SkillButtonRoot;
    public Image img_SkillIcon;
    public Image img_Cooldown;

    private void Awake()
    {
        if (!GameManager.IsDestroying)
        {
            GameManager.Instance._ActionOnSelectArcher.RegistAction(OnSelectArhcer);
            OnSelectArhcer(null, GameManager.Instance._SelectedArcher);
        }
        else OnSelectArhcer(null, null);
    }
    private void OnDestroy()
    {
        if (!GameManager.IsDestroying)
        {
            OnSelectArhcer(GameManager.Instance._SelectedArcher, null);
            GameManager.Instance._ActionOnSelectArcher.RemoveAction(OnSelectArhcer);
        }
    }

    private void OnSelectArhcer(Archer prev, Archer current)
    {
        if (go_SkillButtonRoot != null) go_SkillButtonRoot.SetActive(current != null);

        if (prev != null && prev._ActiveSkill != null) prev._ActiveSkill._ActionOnCooldownChanged.RemoveAction(OnChangeActiveSkillCooldown);
        if (current != null && current._ActiveSkill != null)
        {
            current._ActiveSkill._ActionOnCooldownChanged.RegistAction(OnChangeActiveSkillCooldown);
            OnChangeActiveSkillCooldown(current._ActiveSkill._CurrentCooldown, current._ActiveSkill._Cooldown);
            img_SkillIcon.sprite = current._ActiveSkill._Sprite;
        }
    }
    private void OnChangeActiveSkillCooldown(float current, float total)
    {
        if (img_Cooldown == null) return;

        img_Cooldown.gameObject.SetActive(current > 0f);
        img_Cooldown.fillAmount = total <= 0f ? 0f : current / total;
    }

    public void OnClickActiveSkillButton()
    {
        if (GameManager.IsDestroying) return;

        GameManager.Instance._SelectedArcher.UseActiveSkill();
    }
}
