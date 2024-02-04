using System.Linq;
using UnityEngine;

public class SetterOnStartingScene : MonoBehaviour
{
    [SerializeField] private Transform tf_BarricadePos;
    [SerializeField] private Transform[] tf_ArcherGenPos;
    [SerializeField] private Transform[] tf_MonsterGenPos;

    private void Awake()
    {
        if (!GameManager.IsDestroying)
        {
            GameManager.Instance.SetRootGeneratePosition(transform);
            GameManager.Instance.RegistArcherGenPositions(tf_ArcherGenPos.Select(s => s.position).ToArray());
            GameManager.Instance.CreateArcher(1);
            GameManager.Instance.CreateArcher(2);
        }
        if (!WaveManager.IsDestroying)
        {
            WaveManager.Instance.RegistBarricadePosition(tf_BarricadePos.position);
            WaveManager.Instance.CreateBarricade();
            WaveManager.Instance.RegistMonsterGenPositions(tf_MonsterGenPos.Select(s => s.position).ToArray());
        }
    }
    private void Start()
    {
        if (WaveManager.IsDestroying || GameManager.IsDestroying) return;

        var archers = GameManager.Instance.GetActivatedArchers();
        WaveManager.Instance.StartNextWave();
        var monsters = WaveManager.Instance.GetActivatedMonsters();
        if (monsters == null || monsters.Length == 0) return;

        foreach (var archer in archers)
        {
            var target = monsters[Random.Range(0, monsters.Length)];
            archer.AddNewOrderActions(new Params.CharacterActionParam(Enums.CharacterActionState.ATTACK, Enums.OrderOnEndCharacterAction.REPEAT, target));
            archer.SetActionOnOrderListClear(ActionOnArcherOrderListClear);
        }
    }

    private void ActionOnArcherOrderListClear(Character archer)
    {
        if (archer == null || WaveManager.IsDestroying) return;

        var monsters = WaveManager.Instance.GetActivatedMonsters();
        if (monsters == null || monsters.Length == 0) return;

        var target = monsters[Random.Range(0, monsters.Length)];
        archer.AddNewOrderActions(new Params.CharacterActionParam(Enums.CharacterActionState.ATTACK, Enums.OrderOnEndCharacterAction.REPEAT, target));
    }
}
