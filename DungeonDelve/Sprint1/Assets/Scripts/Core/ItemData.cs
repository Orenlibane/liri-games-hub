using UnityEngine;

namespace DungeonDelve.Core
{
    /// <summary>
    /// Defines an item — its type, stats, rarity, and appearance.
    /// Create instances via Assets → Create → DungeonDelve → Item Data.
    /// One asset per item (e.g. "Iron Sword", "Leather Armor", "Health Potion").
    /// </summary>
    [CreateAssetMenu(fileName = "New ItemData", menuName = "DungeonDelve/Item Data")]
    public class ItemData : ScriptableObject
    {
        // ── Identity ──────────────────────────────────────────────────────
        [Header("Identity")]
        [Tooltip("Display name shown in inventory and pickup prompts.")]
        [SerializeField] private string _itemName = "Iron Sword";

        [Tooltip("Short description shown when the item is examined.")]
        [TextArea(2, 4)]
        [SerializeField] private string _description = "A reliable sword. Nothing fancy.";

        [SerializeField] private Sprite _icon;

        [Tooltip("What slot this item occupies or what it does when used.")]
        [SerializeField] private ItemType _itemType = ItemType.Weapon;

        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;

        // ── Stat Bonuses ───────────────────────────────────────────────
        [Header("Stat Bonuses (0 = no bonus)")]
        [Tooltip("Attack bonus when equipped.")]
        [SerializeField] private int _attackBonus   = 0;

        [Tooltip("Defense bonus when equipped.")]
        [SerializeField] private int _defenseBonus  = 0;

        [Tooltip("Max HP bonus when equipped.")]
        [SerializeField] private int _maxHPBonus    = 0;

        [Tooltip("Speed bonus when equipped (can be negative).")]
        [SerializeField] private int _speedBonus    = 0;

        // ── Consumable ─────────────────────────────────────────────────
        [Header("Consumable (ignored for non-consumables)")]
        [Tooltip("HP restored when used. Only applies to healing consumables.")]
        [SerializeField] private int _healAmount    = 0;

        [Tooltip("Mana restored when used. Only applies to mana consumables.")]
        [SerializeField] private int _manaRestored  = 0;

        // ── Properties ────────────────────────────────────────────────
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

        public bool IsEquippable  => _itemType == ItemType.Weapon
                                  || _itemType == ItemType.Armor
                                  || _itemType == ItemType.Ring;
        public bool IsConsumable  => _itemType == ItemType.Consumable;
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Ring,
        Consumable
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }
}
