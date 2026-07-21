using UnityEngine;

namespace DungeonDelve.Core
{
    [CreateAssetMenu(fileName = "New MetaUnlock", menuName = "DungeonDelve/Meta Unlock")]
    public class MetaUnlock : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _unlockName  = "Unlock";
        [TextArea(2,4)]
        [SerializeField] private string _description = "";
        [SerializeField] private Sprite _icon;

        [Header("Unlock Condition")]
        [SerializeField] private int                  _essenceCost   = 10;
        [SerializeField] private UnlockConditionType  _condition     = UnlockConditionType.Always;
        [SerializeField] private int                  _requiredFloor = 1;
        [SerializeField] private int                  _requiredRuns  = 0;

        [Header("Effect")]
        [SerializeField] private UnlockEffectType _effectType          = UnlockEffectType.StatBonus;
        [SerializeField] private int              _attackBonus         = 0;
        [SerializeField] private int              _defenseBonus        = 0;
        [SerializeField] private int              _maxHPBonus          = 0;
        [SerializeField] private ItemData         _unlockedStartingItem;

        public string             UnlockName           => _unlockName;
        public string             Description          => _description;
        public int                EssenceCost          => _essenceCost;
        public UnlockConditionType Condition           => _condition;
        public UnlockEffectType   EffectType           => _effectType;
        public int                AttackBonus          => _attackBonus;
        public int                DefenseBonus         => _defenseBonus;
        public int                MaxHPBonus           => _maxHPBonus;
        public ItemData           UnlockedStartingItem => _unlockedStartingItem;

        public bool IsConditionMet(int runsCompleted, int deepestFloor) => _condition switch
        {
            UnlockConditionType.Always       => true,
            UnlockConditionType.ReachFloor   => deepestFloor  >= _requiredFloor,
            UnlockConditionType.CompleteRuns => runsCompleted >= _requiredRuns,
            _                                => false
        };
    }

    public enum UnlockConditionType { Always, ReachFloor, CompleteRuns }
    public enum UnlockEffectType    { StatBonus, StartingItem, NewClass, DungeonModifier }
}
