using System;
using UnityEngine;
using Enums;

public class Archer : Character
{
    #region Variables
    public override float _Power
    {
        get
        {
            // �⺻ ���ݷ� ��� ���۵� ������ ���ݷ��� ���̵��� �⺻ ���ݷ� ���� ����
            var ret = base._Power - power;
            // ���� Ŭ���� �ۼ� �Ϸ�� ���⿡ ���� ���ݷ� �ջ� �ڵ� �ۼ�

            return ret;
        }
    }

    public override float _AttackSpeed => base._AttackSpeed;
    #endregion

    #region Skills
    protected ActiveSkill activeSkill = null;
    protected virtual ActiveSkill _ActiveSkill { get { if (activeSkill == null) activeSkill = new ActiveSkill(this); return activeSkill; } }
    protected PassiveSkill passiveSkill = null;
    protected virtual PassiveSkill _PassiveSkill { get { if (passiveSkill == null) passiveSkill = new PassiveSkill(this); return passiveSkill; } }
    #endregion

    #region Actions
    private ActionTemplate<Character> actionOnAttack = new ActionTemplate<Character>();
    private ActionTemplate<Character[]> actionOnSkill = new ActionTemplate<Character[]>();
    private ActionTemplate<float, Character> actionOnHit = new ActionTemplate<float, Character>();
    private ActionTemplate<float, Character> actionOnDamage = new ActionTemplate<float, Character>();
    private Action actionOnDeath;
    private ActionTemplate<Character> actionOnKill = new ActionTemplate<Character>();
    #endregion

    private void Awake()
    {
        Debug.Log(_ActiveSkill.GetType());
        Debug.Log(_PassiveSkill.GetType());

        // �нú� �ߵ� ���ǿ� ����
        switch (_PassiveSkill._ActiveCond)
        {
            case PassiveActiveCondition.BASIC_ATK:
                actionOnAttack.SetAction(target =>
                {
                    var targets = _PassiveSkill._TargetType switch
                    {
                        SkillTargetType.TARGET => new Character[] { target },
                        _ => Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this)
                    };
                    SetTargetsAndApplyEffectOfPassive(targets);
                });
                break;
            case PassiveActiveCondition.ACTIVATE_SKILL:
                actionOnSkill.SetAction(targets =>
                {
                    if (_PassiveSkill._TargetType != SkillTargetType.TARGET)
                        targets = Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this);
                    SetTargetsAndApplyEffectOfPassive(targets);
                });
                break;
            case PassiveActiveCondition.HIT:
                actionOnHit.SetAction((dmg, target) =>
                {
                    var targets = _PassiveSkill._TargetType switch
                    {
                        SkillTargetType.TARGET => new Character[] { target },
                        _ => Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this),
                    };
                    _PassiveSkill.SetCustomCoefficients(dmg);
                    SetTargetsAndApplyEffectOfPassive(targets);
                });
                break;
            case PassiveActiveCondition.DEATH:
                actionOnDeath = () => SetTargetsAndApplyEffectOfPassive(Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this));
                break;
            case PassiveActiveCondition.KILL:
                actionOnKill.SetAction(target =>
                {
                    var targets = _PassiveSkill._TargetType switch
                    {
                        SkillTargetType.TARGET => new Character[] { target },
                        _ => Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this),
                    };
                    SetTargetsAndApplyEffectOfPassive(targets);
                });
                break;
            case PassiveActiveCondition.DAMAGE:
                actionOnDamage.SetAction((dmg, attacker) =>
                {
                    var targets = _PassiveSkill._TargetType switch
                    {
                        SkillTargetType.TARGET => new Character[] { attacker },
                        _ => Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this),
                    };
                    _PassiveSkill.SetCustomCoefficients(dmg);
                    SetTargetsAndApplyEffectOfPassive(targets);
                });
                break;
            case PassiveActiveCondition.CLEAR_WAVE:
                WaveManager.Instance._ActionOnClearWave.RegistAction(ActionOnClearWave);
                break;
        }
    }
    private void SetTargetsAndApplyEffectOfPassive(params Character[] targets)
    {
        _PassiveSkill.SetTargets(targets);
        _PassiveSkill.ApplyEffectToTargets();
    }

    public void UseActiveSkill()
    {
        var skillTargets = Character.GetTargetsByTargetType(_ActiveSkill._TargetType, this);
        _ActiveSkill.SetTargets(skillTargets);
        _ActiveSkill.ApplyEffectToTargets();
        actionOnSkill.Action(skillTargets);
    }

    protected override void ActionOnAttack(Character targetCharacter)
    {
        actionOnAttack.Action(targetCharacter);
    }

    protected override void ActionOnHit(float dmg, Character damagedCharacter)
    {
        actionOnHit.Action(dmg, damagedCharacter);
    }

    protected override void ActionOnDamage(float dmg, Character attackCharacter)
    {
        actionOnDamage.Action(dmg, attackCharacter);
    }

    protected override void ActionOnDeath()
    {
        actionOnDeath?.Invoke();
    }

    public override void NoticedDeathFromTarget(Character targetCharacter)
    {
        actionOnKill.Action(targetCharacter);
    }

    #region Skill Cooldown Function
    /// <summary>
    /// ��Ƽ�� ��ų�� ��Ÿ�� ����<para/>
    /// ��Ÿ�� ���Ҹ� ��ü ��Ÿ���� ������ ������ ������, ���������� ������ ������ ���� �ʿ�
    /// </summary>
    /// <param name="constValue">true: ���������� ����, false: ������ ����</param>
    /// <param name="reductionValue">�������� ��쿡 ���� 0���� ������ 0���� ���, ���������� ���� ��� 0�� 1 ������ ���� ���</param>
    public void ReductActiveSkillCooldown(bool constValue, float reductionValue)
    {
        float cd = _ActiveSkill._Cooldown;
        if (!constValue) reductionValue = Mathf.Clamp01(reductionValue) * cd;
        _ActiveSkill._CurrentCooldown -= reductionValue;
    }

    public void RegistCooldownAction(Action<float> action) => _ActiveSkill.actionOnCooldownChanged.RegistAction(action);
    public void RemoveCooldownAction(Action<float> action) => _ActiveSkill.actionOnCooldownChanged.RemoveAction(action);
    #endregion

    #region ��ų�� �ߵ� ������ ���̺� Ŭ������ �� �ߵ��� �Լ�
    /// <summary>
    /// �нú� ��ų�� �ߵ� ������ ���̺� Ŭ������ �� �ߵ��� �Լ�
    /// </summary>
    /// <param name="stageNum">Ŭ���� ������ �������� ��ȣ</param>
    /// <param name="waveNum">Ŭ���� ������ ���̺� ��ȣ</param>
    private void ActionOnClearWave(int stageNum, int waveNum)
    {
        _PassiveSkill.ApplyEffectToTargets();
    }
    #endregion
}