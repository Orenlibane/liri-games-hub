using UnityEngine;

namespace DungeonDelve.Core
{
    /// <summary>
    /// Defines a meta-progression unlock — a persistent reward the player earns
    /// between runs using Essence (the meta currency). Unlocks persist across runs
    /// and are loaded by GameManager from a save file.
    ///
    /// Create instances via Assets → Create → DungeonDelve → Meta Unlock.
    /// </summary>
    [CreateAssetMenu(fileName = "New MetaUnlock", menuName = "DungeonDelve/Meta Unlock")]
    public class MetaUnlock : ScriptableObject
    {
        // ── Identity ──────────────────────────────────────────────────────
        [Header("Identity")]
        [SerializeField] private string _unlockName = "Veteran's Training";

        [Tooltip("Short description shown in the meta-progression screen.")]
        [TextArea(2, 4)]
        [SerializeField] private string _description = "All classes start with +2 Attack.";

        [Tooltip("Icon shown in the meta-progression screen.")]
        [SerializeField] private Sprite _icon;

        // ── Unlock Condition ───────────────────────────────────────────
        [Header("Unlock Condition")]
        [Tooltip("Essence cost to unlock this upgrade.")]
        [SerializeField] private int _essenceCost = 10;

        [Tooltip("Which condition must be met before this unlock becomes available to purchase.")]
        [SerializeField] private UnlockConditionType _condition = UnlockConditionType.Always;

        [Tooltip("Floor number required (used when Condition = ReachFloor).")]
        [SerializeField] private int _requiredFloor = 1;

        [Tooltip("Number of runs required (used when Condition = CompleteRuns).")]
        [SerializeField] private int _requiredRuns = 0;

        // ── Unlock Effect ──────────────────────────────────────────────
        [Header("Unlock Effect")]
        [Tooltip("What this unlock does when purchased.")]
        [SerializeField] private UnlockEffectType _effectType = UnlockEffectType.StatBonus;

        [Tooltip("Attack bonus granted to all future runs (+0 = no bonus).")]
        [SerializeField] private int _attackBonus   = 0;

        [Tooltip("Defense bonus granted to all future runs.")]
        [SerializeField] private int _defenseBonus  = 0;

        [Tooltip("Max HP bonus granted to all future runs.")]
        [SerializeField] private int _maxHPBonus    = 0;

        [Tooltip("Item that becomes available in the starting item pool.")]
        [SerializeField] private ItemData _unlockedStartingItem;

        // ── Properties ────────────────────────────────────────────────
        public string             UnlockName           => _unlockName;
        public string             Description          => _description;
        public Sprite             Icon                 => _icon;
        public int                EssenceCost          => _essenceCost;
        public UnlockConditionType Condition           => _condition;
        public int                RequiredFloor        => _requiredFloor;
        public int                RequiredRuns         => _requiredRuns;
        public UnlockEffectType   EffectType           => _effectType;
        public int                AttackBonus          => _attackBonus;
        public int                DefenseBonus         => _defenseBonus;
        public int                MaxHPBonus           => _maxHPBonus;
        public ItemData           UnlockedStartingItem => _unlockedStartingItem;

        /// <summary>
        /// Check whether the condition for this unlock is met given the player's
        /// current meta-stats.
        /// </summary>
        public bool IsConditionMet(int totalRunsCompleted, int deepestFloorReached)
        {
            return _condition switch
            {
                UnlockConditionType.Always       => true,
                UnlockConditionType.ReachFloor   => deepestFloorReached >= _requiredFloor,
                UnlockConditionType.CompleteRuns => totalRunsCompleted  >= _requiredRuns,
                _                                => false
            };
        }
    }

    public enum UnlockConditionType
    {
        Always,        // always available to purchase (if you have the Essence)
        ReachFloor,    // must reach a certain floor first
        CompleteRuns   // must complete a certain number of runs
    }

    public enum UnlockEffectType
    {
        StatBonus,        // adds permanent stats to all future runs
        StartingItem,     // adds an item to the starting item pool
        NewClass,         // unlocks a new playable class (⚠️ Assumption: handled in Sprint 5)
        DungeonModifier   // changes dungeon generation rules
    }
}
