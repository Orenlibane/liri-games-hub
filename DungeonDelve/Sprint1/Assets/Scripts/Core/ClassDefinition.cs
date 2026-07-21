using System;
using UnityEngine;

namespace DungeonDelve.Core
{
    /// <summary>
    /// Defines a playable character class — starting stats, starting items,
    /// and the unique class ability. Create one instance per class via
    /// Assets → Create → DungeonDelve → Class Definition.
    /// </summary>
    [CreateAssetMenu(fileName = "New ClassDefinition", menuName = "DungeonDelve/Class Definition")]
    public class ClassDefinition : ScriptableObject
    {
        // ── Identity ──────────────────────────────────────────────────────
        [Header("Identity")]
        [Tooltip("Display name shown in the class select screen.")]
        [SerializeField] private string _className = "Warrior";

        [Tooltip("Short description shown on the class select screen.")]
        [TextArea(2, 4)]
        [SerializeField] private string _classDescription = "A sturdy fighter who excels in close combat.";

        [Tooltip("Sprite shown on the class select card.")]
        [SerializeField] private Sprite _classPortrait;

        // ── Starting Stats ─────────────────────────────────────────────
        [Header("Starting Stats")]
        [Tooltip("Maximum HP at start of run.")]
        [SerializeField] private int _startingMaxHP  = 30;

        [Tooltip("Base attack damage before item bonuses.")]
        [SerializeField] private int _startingAttack = 5;

        [Tooltip("Base defense (damage reduction) before item bonuses.")]
        [SerializeField] private int _startingDefense = 2;

        [Tooltip("Determines action order in a turn — higher acts first.")]
        [SerializeField] private int _startingSpeed  = 5;

        [Tooltip("Starting mana pool (0 for classes that don't use mana).")]
        [SerializeField] private int _startingMaxMana = 0;

        // ── Starting Equipment ─────────────────────────────────────────
        [Header("Starting Items")]
        [Tooltip("Items the player starts each run with. Can be empty.")]
        [SerializeField] private ItemData[] _startingItems = Array.Empty<ItemData>();

        // ── Class Ability ──────────────────────────────────────────────
        [Header("Class Ability")]
        [Tooltip("Name of the unique class ability shown in the UI.")]
        [SerializeField] private string _abilityName = "Block";

        [Tooltip("Description of what the ability does.")]
        [TextArea(2, 4)]
        [SerializeField] private string _abilityDescription = "Once per turn, reduce incoming damage by 50%.";

        [Tooltip("How many turns between uses. 0 = passive (no cooldown).")]
        [SerializeField] private int _abilityCooldownTurns = 3;

        // ── Properties ────────────────────────────────────────────────
        public string     ClassName             => _className;
        public string     ClassDescription      => _classDescription;
        public Sprite     ClassPortrait         => _classPortrait;
        public int        StartingMaxHP         => _startingMaxHP;
        public int        StartingAttack        => _startingAttack;
        public int        StartingDefense       => _startingDefense;
        public int        StartingSpeed         => _startingSpeed;
        public int        StartingMaxMana       => _startingMaxMana;
        public ItemData[] StartingItems         => _startingItems;
        public string     AbilityName           => _abilityName;
        public string     AbilityDescription    => _abilityDescription;
        public int        AbilityCooldownTurns  => _abilityCooldownTurns;
    }
}
