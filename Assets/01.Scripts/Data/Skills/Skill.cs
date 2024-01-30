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

    #region 프로퍼티
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
            Debug.LogWarning("Skill - SetTargets의 characters 입력 값이 비어있음");
            return;
        }
        list_Targets.AddRange(characters);
    }
    /// <summary>
    /// 스킬의 계수로, skillValues 등과 같은 멤버 변수와의 계산에 사용할 값들을 저장<para/>
    /// 어떻게 사용할 지는 상속받은 클래스에서 상세 구현<para/>
    /// 예시: skillValues[0] * customCoefficients[0]
    /// </summary>
    /// <param name="values"></param>
    public void SetCustomCoefficients(params object[] values)
    {
        customCoefficients.Clear();
        if (values == null || values.Length == 0)
        {
            Debug.LogWarning("Skill - SetCustomCoefficients의 values 입력 값이 비어있음");
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
