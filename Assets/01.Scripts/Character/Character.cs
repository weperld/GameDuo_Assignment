using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Enums;

public abstract class Character : MonoBehaviour
{
    public string _ResourcePath { get; private set; }

    #region Inspector
    [SerializeField] private Transform[] tf_DmgPositions;
    #endregion

    #region 기본 능력치 변수
    [Header("Stats")]
    [SerializeField, Tooltip("기본 공격력")] protected float power = 1f;
    [SerializeField, Tooltip("초당 공격 기본 횟수")] protected float baseAttackSpeed = 1f;
    [SerializeField] protected float baseMaxHP = 100f;
    [SerializeField] protected float moveSpeed = 100f;
    [SerializeField, Range(0f, 1000f)] protected float attackRange = 100f;

    protected float attackTimer = 0f;
    protected float currentHP;

    /// <summary>
    /// 공격력
    /// </summary>
    public virtual float _Power
    {
        get
        {
            var ret = power;
            if (dict_AppliedBuffEffects.TryGetValue(BuffStat.Stat.ATK_BOOST, out var v) && !v._IsExpired) ret += v.appliedEffect._BoostValue;
            return ret;
        }
    }
    /// <summary>
    /// 초당 공격 횟수
    /// </summary>
    public virtual float _AttackSpeed
    {
        get
        {
            var ret = baseAttackSpeed;
            if (dict_AppliedBuffEffects.TryGetValue(BuffStat.Stat.ATK_SPD_BOOST, out var v) && v != null)
            {
                if (!v._IsExpired) ret += v.appliedEffect._BoostValue;
            }
            return ret;
        }
    }
    /// <summary>
    /// 체력
    /// </summary>
    public virtual float _MaxHP => baseMaxHP;
    /// <summary>
    /// 최대 공격 사거리
    /// </summary>
    public float _AttackRange => attackRange;
    #endregion

    #region 버프 관련 변수
    protected class AppliedBuffEffect
    {
        public BuffEffect appliedEffect = null;
        public float appliedDuration;
        public bool _IsExpired => appliedEffect == null || appliedEffect._Duration <= appliedDuration;

        private ActionTemplate<AppliedBuffEffect> onExpire = new ActionTemplate<AppliedBuffEffect>();

        public AppliedBuffEffect(BuffEffect effect, Action<AppliedBuffEffect> onExpire)
        {
            appliedEffect = effect;
            appliedDuration = 0f;
            this.onExpire.SetAction(onExpire);
        }

        public void ExpireBuff()
        {
            onExpire.Action(this);
        }
    }

    protected Dictionary<BuffStat.Stat, AppliedBuffEffect> dict_AppliedBuffEffects = new Dictionary<BuffStat.Stat, AppliedBuffEffect>();
    private IEnumerator expiredBuffCoroutine = null;
    #endregion

    #region 애니메이션 관련 변수
    protected const string AnimParam_Attack = "Attack";
    protected const string AnimParam_Hit = "Hit";
    protected const string AnimParam_Death = "Death";
    protected const string AnimParam_Walk = "Walk";

    private Animator animator;
    protected Animator _Animator
    {
        get
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null) animator = gameObject.AddComponent<Animator>();
            }
            return animator;
        }
    }
    #endregion

    #region 캐릭터 행동 상태 관련 변수
    private CharacterActionState currentActionState;
    public CharacterActionState _CurrentActionState => currentActionState;

    private class SetOfActionOrders
    {
        private List<Params.CharacterActionParam> list_ActionOrders = new List<Params.CharacterActionParam>();
        private ActionTemplate actionOnClearList = new ActionTemplate();

        public bool IsEmpty => list_ActionOrders.Count == 0;

        public void Clear()
        {
            list_ActionOrders.Clear();
            actionOnClearList.Action();
        }
        public void AddOrders(params Params.CharacterActionParam[] actionOrders)
        {
            list_ActionOrders.AddRange(actionOrders);
        }
        public Params.CharacterActionParam GetFirstOrder()
        {
            if (IsEmpty) return null;
            return list_ActionOrders[0];
        }
        public void RemoveFirstOrder()
        {
            if (IsEmpty) return;
            list_ActionOrders.RemoveAt(0);
            if (IsEmpty) actionOnClearList.Action();
        }

        public void SetActionOnListClear(Action action)
        {
            actionOnClearList.SetAction(action);
        }
    }
    /// <summary>
    /// 행동 명령 세트
    /// </summary>
    private SetOfActionOrders setOfActionOrders = new SetOfActionOrders();

    private ActionTemplate<Character> alarmOnCharacterIsDead = new ActionTemplate<Character>();
    public ActionTemplate<Character> _ActionOnCharacterIsDead => alarmOnCharacterIsDead;
    #endregion

    public bool _IsDead => currentHP <= 0f;
    protected virtual bool _IsAttackable => !_IsDead && attackTimer <= 0f;

    protected virtual void OnEnable()
    {
        currentActionState = CharacterActionState.IDLE;
        currentHP = _MaxHP;
    }
    protected virtual void OnDisable()
    {
        if (_Animator != null)
        {
            _Animator.SetBool(AnimParam_Death, false);
            _Animator.SetBool(AnimParam_Walk, false);
        }
    }

    protected virtual void Update()
    {
        if (WaveManager.IsDestroying || WaveManager.Instance._WaveSequence != WaveSequence.START) return;

        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        if (!_IsDead && !setOfActionOrders.IsEmpty && currentActionState == CharacterActionState.IDLE)
        {
            var firstAction = setOfActionOrders.GetFirstOrder();
            switch (firstAction.state)
            {
                case CharacterActionState.WAIT:
                    float waitTime = (float)firstAction.values[0];
                    WaitOrders(waitTime, () =>
                    {
                        currentActionState = CharacterActionState.IDLE;
                        setOfActionOrders.RemoveFirstOrder();
                    });
                    break;
                case CharacterActionState.WALK:
                    var target = (Character)firstAction.values[0];
                    var closest = target.FindClosestDmgPosition(transform.position);
                    WalkTo(closest.position, () =>
                        {
                            currentActionState = CharacterActionState.IDLE;
                            _Animator.SetBool(AnimParam_Walk, false);
                            if (firstAction.endOrder == OrderOnEndCharacterAction.REMOVE_ACTION) setOfActionOrders.RemoveFirstOrder();
                        });
                    break;
                case CharacterActionState.ATTACK:
                    target = (Character)firstAction.values[0];
                    BasicAttack(target, () =>
                        {
                            currentActionState = CharacterActionState.IDLE;
                            if (firstAction.endOrder == OrderOnEndCharacterAction.REMOVE_ACTION) setOfActionOrders.RemoveFirstOrder();
                            if (target._IsDead) setOfActionOrders.RemoveFirstOrder();
                        });
                    break;
                default:
                    break;
            }
        }
    }

    public void SetResourcePath(string path)
    {
        _ResourcePath = path;
    }

    #region 전투 액션 함수
    #region 명령 대기
    private void WaitOrders(float waitTime, Action onEnd)
    {
        currentActionState = CharacterActionState.WAIT;
        waitTime = Mathf.Max(waitTime, 0f);
        StartCoroutine(CoroutineWaitOrders(waitTime, onEnd));
    }
    private IEnumerator CoroutineWaitOrders(float waitTime, Action onEnd)
    {
        yield return new WaitForSeconds(waitTime);
        if (onEnd != null) onEnd();
    }
    #endregion

    #region 이동
    public void WalkTo(Vector3 destination, Action onEnd)
    {
        currentActionState = CharacterActionState.WALK;
        _Animator.SetBool(AnimParam_Walk, true);
        if (walkCoroutine != null) StopCoroutine(walkCoroutine);
        walkCoroutine = CoroutineWalkTo(destination, onEnd);
        StartCoroutine(walkCoroutine);
    }
    private IEnumerator walkCoroutine = null;
    private IEnumerator CoroutineWalkTo(Vector3 destination, Action onEnd)
    {
        // 도착지까지 걸어가도록 하는 코루틴
        // 단, 이동에 방해되는 행동을 하는 경우 멈춤
        // 예를 들어 캐릭터가 사망할 경우 영구적으로 이동이 불가능하기 때문에 코루틴 탈출이 필요함
        // 만약, 기절 등의 디버프 효과가 제작되어 적용될 경우 적용된 시간만큼만 이동이 멈추도록 해야함
        do
        {
            yield return null;
            // 이동을 막는 효과가 있을 경우 관련 함수 내용 추가 필요
            if (IsNotWalkable())
            {
                _Animator.SetBool(AnimParam_Walk, false);

                if (!IsTemporaryUnmovable())
                {
                    onEnd?.Invoke();
                    walkCoroutine = null;
                    yield break;
                }
            }
            _Animator.SetBool(AnimParam_Walk, true);

            var currentPos = transform.position;
            float nextPosX = Mathf.MoveTowards(currentPos.x, destination.x, Time.deltaTime * moveSpeed);
            var nextPos = new Vector3(nextPosX, currentPos.y, currentPos.z);
            transform.position = nextPos;
        } while (!IsArrivedToDestination(destination, transform.position));

        //transform.position = destination;
        onEnd?.Invoke();

        walkCoroutine = null;
    }
    /// <summary>
    /// x축 거리만을 계산
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="currentPos"></param>
    /// <param name="margin"></param>
    /// <returns></returns>
    private bool IsArrivedToDestination(Vector3 destination, Vector3 currentPos, float margin = 0.1f)
    {
        var distance = Mathf.Abs(destination.x - currentPos.x);
        if (margin < 0f) margin *= -1f;
        return distance <= margin + attackRange;
    }
    private bool IsNotWalkable()
    {
        return _IsDead || IsTemporaryUnmovable();
    }
    private bool IsTemporaryUnmovable()
    {
        return false;
    }
    #endregion

    #region 기본 공격
    /// <summary>
    /// 기본 공격 시전
    /// </summary>
    protected void BasicAttack(Character targetCharacter, Action onEnd)
    {
        if (!_IsAttackable) return;

        attackTimer = 1f / _AttackSpeed;
        currentActionState = CharacterActionState.ATTACK;
        _Animator.SetTrigger(AnimParam_Attack);
        endActionOfBasicAttack = onEnd;

        var atkInfo = new AttackInformation(this, targetCharacter, AttackType.BASIC, _Power);
        ActionOnBasicAttack(atkInfo);

        Debug.Log($"{gameObject.name}({gameObject.GetInstanceID()}) ATTACK!!");
    }

    /// <summary>
    /// 기본 공격 시 처리할 동작
    /// </summary>
    /// <param name="atkInfo"></param>
    protected abstract void ActionOnBasicAttack(AttackInformation atkInfo);

    private Action endActionOfBasicAttack;
    public void EndOfBasicAttack()
    {
        endActionOfBasicAttack?.Invoke();
    }
    public abstract void AttackOnHitFrameOfAnimation();
    #endregion

    #region 공격 적중
    /// <summary>
    /// 공격 적중
    /// </summary>
    /// <param name="damagedCharacter"></param>
    public void Hit(AttackInformation attackInfo)
    {
        ActionOnHit(attackInfo);
        Debug.Log($"{gameObject.name}({gameObject.GetInstanceID()}) Hit!!");
    }

    /// <summary>
    /// 공격 적중 시 처리할 동작
    /// </summary>
    /// <param name="attackInfo"></param>
    protected abstract void ActionOnHit(AttackInformation attackInfo);
    #endregion

    #region 피격
    /// <summary>
    /// 캐릭터가 피해를 입을 경우
    /// </summary>
    /// <param name="dmg"></param>
    /// <param name="attackCharacter"></param>
    public void Damage(AttackInformation attackInfo)
    {
        currentHP = Mathf.Max(0f, currentHP - attackInfo.dmg);
        DamageEffect(attackInfo.dmg, attackInfo.tf_DmgPos.position, attackInfo.dmgFontOffset); NoticeToAttackCharacterWhenBeHit(attackInfo);
        if (_IsDead) { Death(attackInfo); NoticeDeathToAttackCharacter(attackInfo); }
        else ActionOnDamage(attackInfo);
    }
    private void DamageEffect(float dmg, Vector3 dmgWorldPosition, Vector2 dmgFontOffset)
    {
        if (!UIManager.IsDestroying) UIManager.Instance.SpawnDamageEffectUI(dmg, dmgWorldPosition, dmgFontOffset,
            UIManager.Instance._ColorSwatches[this.GetType() == typeof(Monster) ? 0 : 1]);
    }

    /// <summary>
    /// 피격 시 처리될 동작
    /// </summary>
    /// <param name="attackInfo"></param>
    protected abstract void ActionOnDamage(AttackInformation attackInfo);

    /// <summary>
    /// 나에게 공격이 닿았을 경우 공격을 시전한 캐릭터에게 알리기 위한 
    /// </summary>
    /// <param name="attackCharacter"></param>
    private void NoticeToAttackCharacterWhenBeHit(AttackInformation attackInfo)
    {
        if (attackInfo == null || attackInfo.attacker == null) return;
        attackInfo.attacker.Hit(attackInfo);
    }
    #endregion

    #region 사망
    /// <summary>
    /// 캐릭터 사망
    /// </summary>
    protected void Death(AttackInformation attackInfo)
    {
        _Animator.SetBool(AnimParam_Death, true);
        currentActionState = CharacterActionState.DEATH;
        setOfActionOrders.Clear();
        ActionOnDeath(attackInfo);
        _ActionOnCharacterIsDead.Action(this);
        Debug.Log($"{gameObject.name}({gameObject.GetInstanceID()}) DEAD..");
    }

    /// <summary>
    /// 사망 시 처리될 동작
    /// </summary>
    /// <param name="attackInfo"></param>
    protected abstract void ActionOnDeath(AttackInformation attackInfo);

    /// <summary>
    /// 이 개체를 죽인 캐릭터에게 죽였다는 정보 알림
    /// </summary>
    /// <param name="attackCharacter"></param>
    protected void NoticeDeathToAttackCharacter(AttackInformation attackInfo)
    {
        if (attackInfo == null || attackInfo.attacker == null) return;

        attackInfo.attacker.BeNoticedDeathOfTarget(this);
    }
    /// <summary>
    /// 이 개체의 공격으로 대상 캐릭터가 죽으면 받는 알림
    /// </summary>
    /// <param name="targetCharacter"></param>
    public abstract void BeNoticedDeathOfTarget(Character targetCharacter);
    public void OnEndDeathAnimation()
    {
        if (SpawnManager.IsDestroying) return;
        SpawnManager.Instance.Despawn(_ResourcePath, gameObject);
    }
    #endregion
    #endregion

    #region Buff Methods
    /// <summary>
    /// 버프 효과 적용
    /// 같은 종류의 효과가 이미 적용되었다면 효과 갱신
    /// </summary>
    /// <param name="buffEffects"></param>
    public virtual void ApplyBuffEffects(params BuffEffect[] buffEffects)
    {
        if (buffEffects == null || buffEffects.Length == 0) return;

        foreach (var buffEffect in buffEffects)
        {
            if (buffEffect == null) continue;

            var key = buffEffect._BuffStat;
            var appliedEffectInfo = new AppliedBuffEffect(buffEffect, b =>
            {
                // 버프 시간 만료 시 작동 코드 작성(ex: 비주얼 효과 제거)
                if (dict_AppliedBuffEffects[key] == b) dict_AppliedBuffEffects.Remove(key);
            });
            if (dict_AppliedBuffEffects.ContainsKey(key))
            {
                dict_AppliedBuffEffects[key].ExpireBuff();
                dict_AppliedBuffEffects[key] = appliedEffectInfo;
            }
            else dict_AppliedBuffEffects.Add(key, appliedEffectInfo);
        }

        if (expiredBuffCoroutine != null) return;
        expiredBuffCoroutine = RemoveExpiredBuffs();
        StartCoroutine(expiredBuffCoroutine);
    }
    private IEnumerator RemoveExpiredBuffs()
    {

        int applyingEffCount;
        do
        {
            yield return null;

            applyingEffCount = 0;
            List<BuffStat.Stat> expireKey = new List<BuffStat.Stat>();

            // 만료된 버프 체크
            foreach (var kvp in dict_AppliedBuffEffects)
            {
                if (kvp.Value == null) continue;

                kvp.Value.appliedDuration += Time.deltaTime;
                if (kvp.Value.appliedDuration < kvp.Value.appliedEffect._Duration) applyingEffCount++;
                else expireKey.Add(kvp.Key);
            }

            // 만료 버프 제거
            foreach (var key in expireKey)
                dict_AppliedBuffEffects[key].ExpireBuff();

        } while (applyingEffCount > 0);

        expiredBuffCoroutine = null;
    }
    #endregion

    #region 행동 명령
    public void AddNewOrderActions(params Params.CharacterActionParam[] actions)
    {
        setOfActionOrders.AddOrders(actions);
    }

    public void SetActionOnOrderListClear(Action<Character> action)
    {
        setOfActionOrders.SetActionOnListClear(() => { action?.Invoke(this); });
    }
    #endregion

    public static Character[] GetTargetsByTargetType(SkillTargetType targetType, Character self)
    {
        var all_Ally = GameManager.IsDestroying ? null : GameManager.Instance.GetActivatedArchers();
        var all_Ally_Except_Self = all_Ally.Except(new Character[] { self }).ToArray();
        var all_Enemy = WaveManager.IsDestroying ? null : WaveManager.Instance.GetActivatedMonsters();

        int randVal = UnityEngine.Random.Range(0, 10000);

        return targetType switch
        {
            SkillTargetType.SELF => new Character[] { self },
            SkillTargetType.ALL => all_Ally.Concat(all_Enemy).ToArray(),
            SkillTargetType.ALL_EXCEPT_SELF => all_Ally.Concat(all_Enemy).Except(new Character[] { self }).ToArray(),
            SkillTargetType.ALL_ALLY => all_Ally,
            SkillTargetType.ALL_ALLY_EXCEPT_SELF => all_Ally_Except_Self,
            SkillTargetType.ONE_ALLY => all_Ally.Where(w => randVal % all_Ally.Length == Array.IndexOf(all_Ally, w)).ToArray(),
            SkillTargetType.ONE_ALLY_EXCEPT_SELF => all_Ally_Except_Self.Where(w => randVal % all_Ally_Except_Self.Length == Array.IndexOf(all_Ally_Except_Self, w)).ToArray(),
            SkillTargetType.ALL_ENEMY => all_Enemy,
            SkillTargetType.ONE_ENEMY => all_Enemy.Where(w => randVal % all_Enemy.Length == Array.IndexOf(all_Enemy, w)).ToArray(),
            _ => null,
        };
    }

    public Transform FindClosestDmgPosition(Vector3 pivot)
    {
        if (tf_DmgPositions == null || tf_DmgPositions.Length == 0) return transform;

        var closestPos = tf_DmgPositions[0];
        for (int i = 1; i < tf_DmgPositions.Length; i++)
        {
            var curr = tf_DmgPositions[i];
            var prevDist = (closestPos.position - pivot).magnitude;
            var currDist = (curr.position - pivot).magnitude;

            if (currDist < prevDist)
                closestPos = curr;
        }

        return closestPos;
    }
}
