using UnityEngine;

namespace DungeonDelve.Core
{
    /// <summary>
    /// Defines an enemy type — its stats, AI behaviour, loot, and appearance.
    /// Create instances via Assets → Create → DungeonDelve → Enemy Config.
    /// One asset per enemy type (e.g. "Goblin", "Skeleton Warrior", "Cave Troll").
    /// </summary>
    [CreateAssetMenu(fileName = "New EnemyConfig", menuName = "DungeonDelve/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        // ── Identity ──────────────────────────────────────────────────────
        [Header("Identity")]
        [SerializeField] private string _enemyName = "Goblin";

        [Tooltip("Sprite used for the enemy in the dungeon.")]
        [SerializeField] private Sprite _sprite;

        // ── Base Stats ─────────────────────────────────────────────────
        [Header("Base Stats")]
        [Tooltip("Max HP. Scales by floor if _scalesWithFloor is true.")]
        [SerializeField] private int _baseMaxHP     = 8;

        [SerializeField] private int _baseAttack    = 3;
        [SerializeField] private int _baseDefense   = 0;

        [Tooltip("Determines action order — higher Speed acts earlier each round.")]
        [SerializeField] private int _baseSpeed     = 4;

        [Tooltip("Gold dropped when this enemy is killed.")]
        [SerializeField] private int _goldDrop      = 2;

        [Header("Floor Scaling")]
        [Tooltip("If true, stats increase slightly each floor to keep challenge.")]
        [SerializeField] private bool _scalesWithFloor = true;

        [Tooltip("Multiplier applied per floor for HP scaling.")]
        [SerializeField] private float _hpScalePerFloor     = 0.15f;

        [Tooltip("Multiplier applied per floor for Attack scaling.")]
        [SerializeField] private float _attackScalePerFloor = 0.10f;

        // ── AI Behaviour ───────────────────────────────────────────────
        [Header("AI Behaviour")]
        [Tooltip("How this enemy moves and makes decisions.")]
        [SerializeField] private EnemyBehaviourType _behaviour = EnemyBehaviourType.ChaseAndAttack;

        [Tooltip("How many tiles away the enemy can detect the player.")]
        [SerializeField] private int _detectionRange = 5;

        [Tooltip("How many tiles away the enemy can attack.")]
        [SerializeField] private int _attackRange    = 1;

        // ── Loot Table ─────────────────────────────────────────────────
        [Header("Loot")]
        [Tooltip("Items that can drop when this enemy dies. Each has a drop weight.")]
        [SerializeField] private LootEntry[] _lootTable = System.Array.Empty<LootEntry>();

        // ── Properties ────────────────────────────────────────────────
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

        /// <summary>Returns HP scaled for the given floor number.</summary>
        public int GetScaledHP(int floor)
        {
            if (!_scalesWithFloor) return _baseMaxHP;
            return Mathf.RoundToInt(_baseMaxHP * (1f + _hpScalePerFloor * (floor - 1)));
        }

        /// <summary>Returns Attack scaled for the given floor number.</summary>
        public int GetScaledAttack(int floor)
        {
            if (!_scalesWithFloor) return _baseAttack;
            return Mathf.RoundToInt(_baseAttack * (1f + _attackScalePerFloor * (floor - 1)));
        }
    }

    public enum EnemyBehaviourType
    {
        Patrol,          // walks a set path, ignores player unless attacked
        ChaseAndAttack,  // chases player when in detection range, attacks when adjacent
        Ranged,          // keeps distance, attacks from range
        Boss             // custom boss logic (handled by dedicated BossAI script)
    }

    /// <summary>A single entry in an enemy's loot table.</summary>
    [System.Serializable]
    public class LootEntry
    {
        [Tooltip("The item that can drop.")]
        public ItemData Item;

        [Tooltip("Relative drop weight. Higher = more likely. e.g. Common=10, Rare=2")]
        [Range(1, 100)]
        public int Weight = 10;
    }
}
