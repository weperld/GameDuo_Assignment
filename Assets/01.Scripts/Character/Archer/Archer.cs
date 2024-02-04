using System;
using UnityEngine;
using Enums;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Collections;

public class Archer : Character, IPointerClickHandler
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
    public virtual ActiveSkill _ActiveSkill { get { if (activeSkill == null) activeSkill = new ActiveSkill(this); return activeSkill; } }
    protected PassiveSkill passiveSkill = null;
    public virtual PassiveSkill _PassiveSkill { get { if (passiveSkill == null) passiveSkill = new PassiveSkill(this, 0f, PassiveActiveCondition.NONE); return passiveSkill; } }
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
                    SetTargetsAndUsePassiveSkill(targets);
                });
                break;
            case PassiveActiveCondition.ACTIVATE_SKILL:
                actionOnSkill.SetAction(targets =>
                {
                    if (_PassiveSkill._TargetType != SkillTargetType.TARGET)
                        targets = Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this);
                    SetTargetsAndUsePassiveSkill(targets);
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
                    SetTargetsAndUsePassiveSkill(targets);
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
                    SetTargetsAndUsePassiveSkill(targets);
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
                    SetTargetsAndUsePassiveSkill(targets);
                });
                break;
            case PassiveActiveCondition.DEATH:
                actionOnDeath.SetAction(atkInfo => SetTargetsAndUsePassiveSkill(Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this)));
                break;
            case PassiveActiveCondition.KILL:
                actionOnKill.SetAction(target =>
                {
                    var targets = _PassiveSkill._TargetType switch
                    {
                        SkillTargetType.TARGET => new Character[] { target },
                        _ => Character.GetTargetsByTargetType(_PassiveSkill._TargetType, this),
                    };
                    SetTargetsAndUsePassiveSkill(targets);
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
                    SetTargetsAndUsePassiveSkill(targets);
                });
                break;
            case PassiveActiveCondition.CLEAR_WAVE:
                if (!WaveManager.IsDestroying) WaveManager.Instance._ActionOnClearWave.RegistAction(ActionOnClearWave);
                break;
        }
    }
    private void SetTargetsAndUsePassiveSkill(params Character[] targets)
    {
        _PassiveSkill.SetTargets(targets);
        _PassiveSkill.UseSkill();
    }

    public void UseActiveSkill()
    {
        var skillTargets = Character.GetTargetsByTargetType(_ActiveSkill._TargetType, this);
        _ActiveSkill.SetTargets(skillTargets);
        _ActiveSkill.UseSkill();
        actionOnSkill.Action(skillTargets);
    }

    #region ���� �׼�
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
    #endregion

    #region Skill Cooldown
    /// <summary>
    /// ��Ƽ�� ��ų�� ��Ÿ�� ����<para/>
    /// ��Ÿ�� ���Ҹ� ��ü ��Ÿ���� ������ ������ ������, ���������� ������ ������ ���� �ʿ�
    /// </summary>
    /// <param name="constValue">true: ���������� ����, false: ������ ����</param>
    /// <param name="reductionValue">�������� ��쿡 ���� 0���� ������ 0���� ���, ���������� ���� ��� 0�� 1 ������ ���� ���</param>
    public void ReductActiveSkillCooldown(bool constValue, float reductionValue)
    {
        if (!constValue) reductionValue = Mathf.Clamp01(reductionValue) * _ActiveSkill._Cooldown;
        _ActiveSkill._CurrentCooldown -= reductionValue;
    }

    public IEnumerator CoroutineReductActiveSkillCooldown(Skill skill)
    {
        if (skill == null) yield break;

        while (skill._CurrentCooldown > 0f)
        {
            yield return null;
            skill._CurrentCooldown -= Time.deltaTime;
        }
    }
    #endregion

    #region ��ų�� �ߵ� ������ ���̺� Ŭ������ �� �ߵ��� �Լ�
    /// <summary>
    /// �нú� ��ų�� �ߵ� ������ ���̺� Ŭ������ �� �ߵ��� �Լ�
    /// </summary>
    /// <param name="stageNum">Ŭ���� ������ �������� ��ȣ</param>
    /// <param name="waveNum">Ŭ���� ������ ���̺� ��ȣ</param>
    private void ActionOnClearWave(int stageNum, int waveNum)
    {
        _PassiveSkill.UseSkill();
    }
    #endregion

    #region ��ġ �̺�Ʈ
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Touch On {name}");
        if (GameManager.IsDestroying) return;
        GameManager.Instance._SelectedArcher = this;
    }
    #endregion
}