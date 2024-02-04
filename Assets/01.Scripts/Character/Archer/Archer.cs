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
            // 무기 클래스 작성 완료시
            // 기본 공격력 대신 제작된 무기의 공격력이 쓰이도록 기본 공격력 값을 제거하고
            // 무기 공격력 합산 코드 작성

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
        // 패시브 발동 조건에 따라
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

    #region 전투 액션
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
    /// 액티브 스킬의 쿨타임 감소<para/>
    /// 쿨타임 감소를 전체 쿨타임의 비율로 진행할 것인지, 고정값으로 진행할 것인지 결정 필요
    /// </summary>
    /// <param name="constValue">true: 고정값으로 감소, false: 비율로 감소</param>
    /// <param name="reductionValue">고정값일 경우에 값이 0보다 작으면 0으로 계산, 비율값으로 사용될 경우 0과 1 사이의 값만 사용</param>
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

    #region 스킬의 발동 조건이 웨이브 클리어일 때 발동될 함수
    /// <summary>
    /// 패시브 스킬의 발동 조건이 웨이브 클리어일 때 발동될 함수
    /// </summary>
    /// <param name="stageNum">클리어 시점의 스테이지 번호</param>
    /// <param name="waveNum">클리어 시점의 웨이브 번호</param>
    private void ActionOnClearWave(int stageNum, int waveNum)
    {
        _PassiveSkill.UseSkill();
    }
    #endregion

    #region 터치 이벤트
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Touch On {name}");
        if (GameManager.IsDestroying) return;
        GameManager.Instance._SelectedArcher = this;
    }
    #endregion
}