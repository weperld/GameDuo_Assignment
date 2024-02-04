using System;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public abstract class Skill : ISkill
{
    protected virtual string _SpritePath { get; }
    protected virtual string _SpriteName { get; }
    public Sprite _Sprite
    {
        get
        {
            var sprites = Resources.LoadAll<Sprite>(_SpritePath);
            if (sprites == null || sprites.Length == 0) return null;

            foreach (var sprite in sprites)
                if (sprite.name == _SpriteName)
                    return sprite;
            return null;
        }
    }


    private float cooldown = 30f;
    private SkillEffects effect;
    private SkillTargetType targetType;
    private object[] skillValues;

    private float currentCooldown = 0f;

    protected List<object> customCoefficients = new List<object>();
    protected List<Character> list_Targets = new List<Character>();

    /// <summary>
    /// (current, total cooldown)
    /// </summary>
    public ActionTemplate<float, float> _ActionOnCooldownChanged { get; } = new ActionTemplate<float, float>();

    #region 프로퍼티
    public Archer _SkillOwner { get; private set; }

    /// <summary>
    /// 전체 쿨타임
    /// </summary>
    public float _Cooldown => cooldown;
    public SkillEffects _Effect => effect;
    public SkillTargetType _TargetType => targetType;
    public object[] _SkillValues => skillValues;

    /// <summary>
    /// 현재 쿨타임
    /// </summary>
    public float _CurrentCooldown
    {
        get => currentCooldown;
        set
        {
            currentCooldown = Mathf.Clamp(value, 0f, _Cooldown);
            _ActionOnCooldownChanged.Action(currentCooldown, _Cooldown);
        }
    }
    #endregion

    public Skill(Archer owner) { _SkillOwner = owner; }
    public Skill(Archer owner, float cd, SkillEffects eff, SkillTargetType targetType, object[] values) : this(owner)
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

    public virtual void UseSkill()
    {
        if (_CurrentCooldown > 0f) return;
        _CurrentCooldown = _Cooldown;
        if (_SkillOwner != null && _SkillOwner.gameObject.activeInHierarchy) _SkillOwner.StartCoroutine(_SkillOwner.CoroutineReductActiveSkillCooldown(this));

        ApplyEffectToTargets();
    }
    /// <summary>
    /// When use skill, apply skill effect to targets
    /// </summary>
    protected abstract void ApplyEffectToTargets();
    protected abstract void ExpireAppliedEffectOfTargets();
}
