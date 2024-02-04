using System;
using UnityEngine;
using Enums;
using UnityEditor;

public class Archer : Character
{
    #region Inspector
    [Header("Archer")]
    [SerializeField] private Transform tf_FireArrowPos;
    #endregion

    #region Variables
    public override float _Power
    {
        get
        {
            var ret = base._Power;
            // ���� Ŭ���� �ۼ� �Ϸ��
            // �⺻ ���ݷ� ��� ���۵� ������ ���ݷ��� ���̵��� �⺻ ���ݷ� ���� �����ϰ�
            // ���� ���ݷ� �ջ� �ڵ� �ۼ�

            return ret;
        }
    }

    public override float _AttackSpeed => base._AttackSpeed;

    private AttackInformation basicAtkInfo;
    #endregion

    #region Skills
    protected ActiveSkill activeSkill = null;
    protected virtual ActiveSkill _ActiveSkill { get { if (activeSkill == null) activeSkill = new ActiveSkill(this); return activeSkill; } }
    protected PassiveSkill passiveSkill = null;
    protected virtual PassiveSkill _PassiveSkill { get { if (passiveSkill == null) passiveSkill = new PassiveSkill(this, 0f, PassiveActiveCondition.NONE); return passiveSkill; } }
    #endregion

    #region Actions
    private ActionTemplate<AttackInformation> actionOnAttack = new ActionTemplate<AttackInformation>();
    private ActionTemplate<Character[]> actionOnSkill = new ActionTemplate<Character[]>();
    private ActionTemplate<AttackInformation> actionOnHitAll = new ActionTemplate<AttackInformation>();
    private ActionTemplate<AttackInformation> actionOnHitBasicAtk = new ActionTemplate<AttackInformation>();
    private ActionTemplate<AttackInformation> actionOnHitSkill = new ActionTemplate<AttackInformation>();
    private ActionTemplate<AttackInformation> actionOnDamage = new ActionTemplate<AttackInformation>();
    private ActionTemplate<AttackInformation> actionOnDeath = new ActionTemplate<AttackInformation>();
    private ActionTemplate<Character> actionOnKill = new ActionTemplate<Character>();
    #endregion

    private void Awake()
    {
        // �нú� �ߵ� ���ǿ� ����
        switch (_PassiveSkill._ActiveCond)
        {
            case PassiveActiveCondition.BASIC_ATK:
                actionOnAttack.SetAction(atkInfo =>
                {
                    var targets = _PassiveSkill._TargetType switch
                    {
                        SkillTargetType.TARGET => new Character[] { atkInfo.target },
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
            case PassiveActiveCondition.HIT_ALL:
                actionOnHitAll.SetAction(atkInfo =>
                {
                    var targets = _PassiveSkill._TargetType switch
                    {
                        SkillTargetType.TARGET => new Character[] { atkInfo.target },
                        _ => Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this),
                    };
                    _PassiveSkill.SetCustomCoefficients(atkInfo);
                    SetTargetsAndApplyEffectOfPassive(targets);
                });
                break;
            case PassiveActiveCondition.HIT_BASIC_ATK:
                actionOnHitBasicAtk.SetAction(atkInfo =>
                {
                    var targets = _PassiveSkill._TargetType switch
                    {
                        SkillTargetType.TARGET => new Character[] { atkInfo.target },
                        _ => Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this),
                    };
                    _PassiveSkill.SetCustomCoefficients(atkInfo);
                    SetTargetsAndApplyEffectOfPassive(targets);
                });
                break;
            case PassiveActiveCondition.HIT_SKILL:
                actionOnHitSkill.SetAction(atkInfo =>
                {
                    var targets = _PassiveSkill._TargetType switch
                    {
                        SkillTargetType.TARGET => new Character[] { atkInfo.target },
                        _ => Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this),
                    };
                    _PassiveSkill.SetCustomCoefficients(atkInfo);
                    SetTargetsAndApplyEffectOfPassive(targets);
                });
                break;
            case PassiveActiveCondition.DEATH:
                actionOnDeath.SetAction(atkInfo => SetTargetsAndApplyEffectOfPassive(Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this)));
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
                actionOnDamage.SetAction(atkInfo =>
                {
                    var targets = _PassiveSkill._TargetType switch
                    {
                        SkillTargetType.TARGET => new Character[] { atkInfo.attacker },
                        _ => Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this),
                    };
                    _PassiveSkill.SetCustomCoefficients(atkInfo);
                    SetTargetsAndApplyEffectOfPassive(targets);
                });
                break;
            case PassiveActiveCondition.CLEAR_WAVE:
                if (!WaveManager.IsDestroying) WaveManager.Instance._ActionOnClearWave.RegistAction(ActionOnClearWave);
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

    protected override void ActionOnBasicAttack(AttackInformation attackInfo)
    {
        basicAtkInfo = attackInfo;
        actionOnAttack.Action(attackInfo);
    }

    protected override void ActionOnHit(AttackInformation attackInfo)
    {
        switch (attackInfo.attackType)
        {
            case AttackType.BASIC: actionOnHitBasicAtk.Action(attackInfo); break;
            case AttackType.SKILL: actionOnHitSkill.Action(attackInfo); break;
        }
        actionOnHitAll.Action(attackInfo);
    }

    protected override void ActionOnDamage(AttackInformation attackInfo)
    {
        actionOnDamage.Action(attackInfo);
    }

    protected override void ActionOnDeath(AttackInformation attackInfo)
    {
        actionOnDeath.Action(attackInfo);
    }

    public override void BeNoticedDeathOfTarget(Character targetCharacter)
    {
        actionOnKill.Action(targetCharacter);
    }

    public override void AttackOnHitFrameOfAnimation()
    {
        if (SpawnManager.IsDestroying) return;

        var arrow = SpawnManager.Instance.Spawn<Arrow>(PathOfResources.Prefabs.Arrow);

        if (arrow == null) return;
        arrow.SetAttackInformationAndFire(tf_FireArrowPos.position, basicAtkInfo);
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