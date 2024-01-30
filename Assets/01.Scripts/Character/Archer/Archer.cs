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
            // 기본 공격력 대신 제작된 무기의 공격력이 쓰이도록 기본 공격력 값을 제거
            var ret = base._Power - power;
            // 무기 클래스 작성 완료시 여기에 무기 공격력 합산 코드 작성

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

        // 패시브 발동 조건에 따라
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
    /// 액티브 스킬의 쿨타임 감소<para/>
    /// 쿨타임 감소를 전체 쿨타임의 비율로 진행할 것인지, 고정값으로 진행할 것인지 결정 필요
    /// </summary>
    /// <param name="constValue">true: 고정값으로 감소, false: 비율로 감소</param>
    /// <param name="reductionValue">고정값일 경우에 값이 0보다 작으면 0으로 계산, 비율값으로 사용될 경우 0과 1 사이의 값만 사용</param>
    public void ReductActiveSkillCooldown(bool constValue, float reductionValue)
    {
        float cd = _ActiveSkill._Cooldown;
        if (!constValue) reductionValue = Mathf.Clamp01(reductionValue) * cd;
        _ActiveSkill._CurrentCooldown -= reductionValue;
    }

    public void RegistCooldownAction(Action<float> action) => _ActiveSkill.actionOnCooldownChanged.RegistAction(action);
    public void RemoveCooldownAction(Action<float> action) => _ActiveSkill.actionOnCooldownChanged.RemoveAction(action);
    #endregion

    #region 스킬의 발동 조건이 웨이브 클리어일 때 발동될 함수
    /// <summary>
    /// 패시브 스킬의 발동 조건이 웨이브 클리어일 때 발동될 함수
    /// </summary>
    /// <param name="stageNum">클리어 시점의 스테이지 번호</param>
    /// <param name="waveNum">클리어 시점의 웨이브 번호</param>
    private void ActionOnClearWave(int stageNum, int waveNum)
    {
        _PassiveSkill.ApplyEffectToTargets();
    }
    #endregion
}