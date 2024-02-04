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

    #region �⺻ �ɷ�ġ ����
    [Header("Stats")]
    [SerializeField, Tooltip("�⺻ ���ݷ�")] protected float power = 1f;
    [SerializeField, Tooltip("�ʴ� ���� �⺻ Ƚ��")] protected float baseAttackSpeed = 1f;
    [SerializeField] protected float baseMaxHP = 100f;
    [SerializeField] protected float moveSpeed = 100f;
    [SerializeField, Range(0f, 1000f)] protected float attackRange = 100f;

    protected float attackTimer = 0f;
    protected float currentHP;

    /// <summary>
    /// ���ݷ�
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
    /// �ʴ� ���� Ƚ��
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
    /// ü��
    /// </summary>
    public virtual float _MaxHP => baseMaxHP;
    /// <summary>
    /// �ִ� ���� ��Ÿ�
    /// </summary>
    public float _AttackRange => attackRange;
    #endregion

    #region ���� ���� ����
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

    #region �ִϸ��̼� ���� ����
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

    #region ĳ���� �ൿ ���� ���� ����
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
    /// �ൿ ��� ��Ʈ
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

    #region ���� �׼� �Լ�
    #region ��� ���
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

    #region �̵�
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
        // ���������� �ɾ���� �ϴ� �ڷ�ƾ
        // ��, �̵��� ���صǴ� �ൿ�� �ϴ� ��� ����
        // ���� ��� ĳ���Ͱ� ����� ��� ���������� �̵��� �Ұ����ϱ� ������ �ڷ�ƾ Ż���� �ʿ���
        // ����, ���� ���� ����� ȿ���� ���۵Ǿ� ����� ��� ����� �ð���ŭ�� �̵��� ���ߵ��� �ؾ���
        do
        {
            yield return null;
            // �̵��� ���� ȿ���� ���� ��� ���� �Լ� ���� �߰� �ʿ�
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
    /// x�� �Ÿ����� ���
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

    #region �⺻ ����
    /// <summary>
    /// �⺻ ���� ����
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
    /// �⺻ ���� �� ó���� ����
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

    #region ���� ����
    /// <summary>
    /// ���� ����
    /// </summary>
    /// <param name="damagedCharacter"></param>
    public void Hit(AttackInformation attackInfo)
    {
        ActionOnHit(attackInfo);
        Debug.Log($"{gameObject.name}({gameObject.GetInstanceID()}) Hit!!");
    }

    /// <summary>
    /// ���� ���� �� ó���� ����
    /// </summary>
    /// <param name="attackInfo"></param>
    protected abstract void ActionOnHit(AttackInformation attackInfo);
    #endregion

    #region �ǰ�
    /// <summary>
    /// ĳ���Ͱ� ���ظ� ���� ���
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
    /// �ǰ� �� ó���� ����
    /// </summary>
    /// <param name="attackInfo"></param>
    protected abstract void ActionOnDamage(AttackInformation attackInfo);

    /// <summary>
    /// ������ ������ ����� ��� ������ ������ ĳ���Ϳ��� �˸��� ���� 
    /// </summary>
    /// <param name="attackCharacter"></param>
    private void NoticeToAttackCharacterWhenBeHit(AttackInformation attackInfo)
    {
        if (attackInfo == null || attackInfo.attacker == null) return;
        attackInfo.attacker.Hit(attackInfo);
    }
    #endregion

    #region ���
    /// <summary>
    /// ĳ���� ���
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
    /// ��� �� ó���� ����
    /// </summary>
    /// <param name="attackInfo"></param>
    protected abstract void ActionOnDeath(AttackInformation attackInfo);

    /// <summary>
    /// �� ��ü�� ���� ĳ���Ϳ��� �׿��ٴ� ���� �˸�
    /// </summary>
    /// <param name="attackCharacter"></param>
    protected void NoticeDeathToAttackCharacter(AttackInformation attackInfo)
    {
        if (attackInfo == null || attackInfo.attacker == null) return;

        attackInfo.attacker.BeNoticedDeathOfTarget(this);
    }
    /// <summary>
    /// �� ��ü�� �������� ��� ĳ���Ͱ� ������ �޴� �˸�
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
    /// ���� ȿ�� ����
    /// ���� ������ ȿ���� �̹� ����Ǿ��ٸ� ȿ�� ����
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
                // ���� �ð� ���� �� �۵� �ڵ� �ۼ�(ex: ���־� ȿ�� ����)
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

            // ����� ���� üũ
            foreach (var kvp in dict_AppliedBuffEffects)
            {
                if (kvp.Value == null) continue;

                kvp.Value.appliedDuration += Time.deltaTime;
                if (kvp.Value.appliedDuration < kvp.Value.appliedEffect._Duration) applyingEffCount++;
                else expireKey.Add(kvp.Key);
            }

            // ���� ���� ����
            foreach (var key in expireKey)
                dict_AppliedBuffEffects[key].ExpireBuff();

        } while (applyingEffCount > 0);

        expiredBuffCoroutine = null;
    }
    #endregion

    #region �ൿ ���
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
