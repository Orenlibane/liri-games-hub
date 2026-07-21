using System;
using UnityEngine;

namespace DungeonDelve.Core
{
    [CreateAssetMenu(fileName = "New ClassDefinition", menuName = "DungeonDelve/Class Definition")]
    public class ClassDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _className        = "Warrior";
        [TextArea(2, 4)]
        [SerializeField] private string _classDescription = "A sturdy fighter.";
        [SerializeField] private Sprite _classPortrait;

        [Header("Starting Stats")]
        [SerializeField] private int _startingMaxHP    = 30;
        [SerializeField] private int _startingAttack   = 5;
        [SerializeField] private int _startingDefense  = 2;
        [SerializeField] private int _startingSpeed    = 5;
        [SerializeField] private int _startingMaxMana  = 0;

        [Header("Starting Items")]
        [SerializeField] private ItemData[] _startingItems = Array.Empty<ItemData>();

        [Header("Class Ability")]
        [SerializeField] private string _abilityName           = "Block";
        [TextArea(2, 4)]
        [SerializeField] private string _abilityDescription    = "Reduce incoming damage by 50% once per turn.";
        [SerializeField] private int    _abilityCooldownTurns  = 3;

        public string     ClassName            => _className;
        public string     ClassDescription     => _classDescription;
        public Sprite     ClassPortrait        => _classPortrait;
        public int        StartingMaxHP        => _startingMaxHP;
        public int        StartingAttack       => _startingAttack;
        public int        StartingDefense      => _startingDefense;
        public int        StartingSpeed        => _startingSpeed;
        public int        StartingMaxMana      => _startingMaxMana;
        public ItemData[] StartingItems        => _startingItems;
        public string     AbilityName          => _abilityName;
        public string     AbilityDescription   => _abilityDescription;
        public int        AbilityCooldownTurns => _abilityCooldownTurns;

        /// <summary>Called by the Editor setup script to configure this SO programmatically.</summary>
        public void SetEditorValues(string className, string description,
            int maxHP, int attack, int defense, int speed, int mana,
            string abilityName, string abilityDesc, int abilityCooldown)
        {
            _className             = className;
            _classDescription      = description;
            _startingMaxHP         = maxHP;
            _startingAttack        = attack;
            _startingDefense       = defense;
            _startingSpeed         = speed;
            _startingMaxMana       = mana;
            _abilityName           = abilityName;
            _abilityDescription    = abilityDesc;
            _abilityCooldownTurns  = abilityCooldown;
        }
    }
}
