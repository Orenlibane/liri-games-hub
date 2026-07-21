using UnityEngine;

namespace DungeonDelve.Core
{
    [CreateAssetMenu(fileName = "New EnemyConfig", menuName = "DungeonDelve/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _enemyName = "Enemy";
        [SerializeField] private Sprite _sprite;

        [Header("Base Stats")]
        [SerializeField] private int   _baseMaxHP           = 10;
        [SerializeField] private int   _baseAttack          = 3;
        [SerializeField] private int   _baseDefense         = 0;
        [SerializeField] private int   _baseSpeed           = 4;
        [SerializeField] private int   _goldDrop            = 2;

        [Header("Floor Scaling")]
        [SerializeField] private bool  _scalesWithFloor     = true;
        [SerializeField] private float _hpScalePerFloor     = 0.15f;
        [SerializeField] private float _attackScalePerFloor = 0.10f;

        [Header("AI")]
        [SerializeField] private EnemyBehaviourType _behaviour       = EnemyBehaviourType.ChaseAndAttack;
        [SerializeField] private int                _detectionRange  = 5;
        [SerializeField] private int                _attackRange     = 1;

        [Header("Loot")]
        [SerializeField] private LootEntry[] _lootTable = System.Array.Empty<LootEntry>();

        public string             EnemyName       => _enemyName;
        public Sprite             Sprite          => _sprite;
        public int                BaseMaxHP       => _baseMaxHP;
        public int                BaseAttack      => _baseAttack;
        public int                BaseDefense     => _baseDefense;
        public int                BaseSpeed       => _baseSpeed;
        public int                GoldDrop        => _goldDrop;
        public EnemyBehaviourType Behaviour       => _behaviour;
        public int                DetectionRange  => _detectionRange;
        public int                AttackRange     => _attackRange;
        public LootEntry[]        LootTable       => _lootTable;

        public int GetScaledHP(int floor)     => _scalesWithFloor
            ? Mathf.RoundToInt(_baseMaxHP     * (1f + _hpScalePerFloor     * (floor - 1))) : _baseMaxHP;
        public int GetScaledAttack(int floor) => _scalesWithFloor
            ? Mathf.RoundToInt(_baseAttack    * (1f + _attackScalePerFloor * (floor - 1))) : _baseAttack;

        public void SetEditorValues(string enemyName, int maxHP, int attack, int defense,
            int speed, int goldDrop, EnemyBehaviourType behaviour, int detectionRange, int attackRange)
        {
            _enemyName       = enemyName;
            _baseMaxHP       = maxHP;
            _baseAttack      = attack;
            _baseDefense     = defense;
            _baseSpeed       = speed;
            _goldDrop        = goldDrop;
            _behaviour       = behaviour;
            _detectionRange  = detectionRange;
            _attackRange     = attackRange;
        }
    }

    public enum EnemyBehaviourType { Patrol, ChaseAndAttack, Ranged, Boss }

    [System.Serializable]
    public class LootEntry
    {
        public ItemData Item;
        [Range(1, 100)] public int Weight = 10;
    }
}
