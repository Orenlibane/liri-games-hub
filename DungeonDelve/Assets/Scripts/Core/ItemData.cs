using UnityEngine;

namespace DungeonDelve.Core
{
    [CreateAssetMenu(fileName = "New ItemData", menuName = "DungeonDelve/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string     _itemName    = "Item";
        [TextArea(2, 4)]
        [SerializeField] private string     _description = "";
        [SerializeField] private Sprite     _icon;
        [SerializeField] private ItemType   _itemType    = ItemType.Weapon;
        [SerializeField] private ItemRarity _rarity      = ItemRarity.Common;

        [Header("Stat Bonuses")]
        [SerializeField] private int _attackBonus  = 0;
        [SerializeField] private int _defenseBonus = 0;
        [SerializeField] private int _maxHPBonus   = 0;
        [SerializeField] private int _speedBonus   = 0;

        [Header("Consumable")]
        [SerializeField] private int _healAmount   = 0;
        [SerializeField] private int _manaRestored = 0;

        public string     ItemName      => _itemName;
        public string     Description   => _description;
        public Sprite     Icon          => _icon;
        public ItemType   ItemType      => _itemType;
        public ItemRarity Rarity        => _rarity;
        public int        AttackBonus   => _attackBonus;
        public int        DefenseBonus  => _defenseBonus;
        public int        MaxHPBonus    => _maxHPBonus;
        public int        SpeedBonus    => _speedBonus;
        public int        HealAmount    => _healAmount;
        public int        ManaRestored  => _manaRestored;

        public bool IsEquippable => _itemType != ItemType.Consumable;
        public bool IsConsumable => _itemType == ItemType.Consumable;

        public void SetEditorValues(string itemName, string description,
            ItemType type, ItemRarity rarity,
            int attackBonus = 0, int defenseBonus = 0, int maxHPBonus = 0,
            int speedBonus = 0, int healAmount = 0, int manaRestored = 0)
        {
            _itemName     = itemName;
            _description  = description;
            _itemType     = type;
            _rarity       = rarity;
            _attackBonus  = attackBonus;
            _defenseBonus = defenseBonus;
            _maxHPBonus   = maxHPBonus;
            _speedBonus   = speedBonus;
            _healAmount   = healAmount;
            _manaRestored = manaRestored;
        }
    }

    public enum ItemType   { Weapon, Armor, Ring, Consumable }
    public enum ItemRarity { Common, Uncommon, Rare, Legendary }
}
