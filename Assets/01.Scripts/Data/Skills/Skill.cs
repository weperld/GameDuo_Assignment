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

    #region ������Ƽ
    public Archer _SkillOwner { get; private set; }

    /// <summary>
    /// ��ü ��Ÿ��
    /// </summary>
    public float _Cooldown => cooldown;
    public SkillEffects _Effect => effect;
    public SkillTargetType _TargetType => targetType;
    public object[] _SkillValues => skillValues;

    /// <summary>
    /// ���� ��Ÿ��
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
