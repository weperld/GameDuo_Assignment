using System;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public abstract class Skill : ISkill
{
    private float cooldown = 30f;
    private SkillEffects effect;
    private SkillTargetType targetType;
    private object[] skillValues;

    private float currentCooldown = 0f;

    protected List<object> customCoefficients = new List<object>();
    protected List<Character> list_Targets = new List<Character>();

    public ActionTemplate<float> actionOnCooldownChanged = new ActionTemplate<float>();

    #region ������Ƽ
    public Character _SkillOwner { get; private set; }

    public float _Cooldown => cooldown;
    public SkillEffects _Effect => effect;
    public SkillTargetType _TargetType => targetType;
    public object[] _SkillValues => skillValues;

    public float _CurrentCooldown
    {
        get => currentCooldown;
        set
        {
            float decrementValue = Mathf.Clamp(value, 0f, _Cooldown);
            currentCooldown = Mathf.Max(currentCooldown - decrementValue, 0f);
            actionOnCooldownChanged.Action(currentCooldown);
        }
    }
    #endregion

    public Skill(Character owner) { _SkillOwner = owner; }
    public Skill(Character owner, float cd, SkillEffects eff, SkillTargetType targetType, object[] values) : this(owner)
    {
        cooldown = cd;
        effect = eff;
        this.targetType = targetType;
        skillValues = values;
    }

    public virtual void SetTargets(params Character[] characters)
    {
        list_Targets.Clear();
        if (characters == null || characters.Length == 0)
        {
            Debug.LogWarning("Skill - SetTargets�� characters �Է� ���� �������");
            return;
        }
        list_Targets.AddRange(characters);
    }
    /// <summary>
    /// ��ų�� �����, skillValues ��� ���� ��� �������� ��꿡 ����� ������ ����<para/>
    /// ��� ����� ���� ��ӹ��� Ŭ�������� �� ����<para/>
    /// ����: skillValues[0] * customCoefficients[0]
    /// </summary>
    /// <param name="values"></param>
    public void SetCustomCoefficients(params object[] values)
    {
        customCoefficients.Clear();
        if (values == null || values.Length == 0)
        {
            Debug.LogWarning("Skill - SetCustomCoefficients�� values �Է� ���� �������");
            return;
        }
        customCoefficients.AddRange(values);
    }

    /// <summary>
    /// When use skill, apply skill effect to targets
    /// </summary>
    public abstract void ApplyEffectToTargets();
    public abstract void ExpireAppliedEffectOfTargets();
}
