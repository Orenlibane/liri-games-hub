using System;
using UnityEngine;

namespace DungeonDelve.Core
{
    /// <summary>
    /// Implemented by any entity that participates in the turn system —
    /// the player, enemies, and any future summons or traps.
    /// </summary>
    public interface IActable
    {
        /// <summary>Display name used in debug logs and future combat log UI.</summary>
        string ActableName { get; }

        /// <summary>
        /// Speed determines action order within a turn cycle.
        /// Higher speed = acts earlier in the round.
        /// </summary>
        int Speed { get; }

        /// <summary>
        /// Whether this entity can currently act. False if stunned, dead, or
        /// waiting for async input.
        /// </summary>
        bool CanAct { get; }

        /// <summary>
        /// Request this entity to take its turn. The entity is responsible for
        /// submitting an action to TurnManager via SubmitPlayerAction (if player)
        /// or executing immediately (if AI).
        /// </summary>
        void RequestTurn();

        /// <summary>Called by TurnManager at the start of this entity's turn.</summary>
        event Action OnTurnStarted;

        /// <summary>Called by TurnManager when this entity's turn is complete.</summary>
        event Action OnTurnEnded;
    }

    /// <summary>
    /// Represents a single action taken during a turn — movement, attack, wait, etc.
    /// </summary>
    public interface IAction
    {
        ActionType ActionType    { get; }
        Vector2Int TargetTile    { get; }  // grid position this action targets
        IActable   Actor         { get; }  // who is performing the action
    }

    /// <summary>All possible turn actions in the game.</summary>
    public enum ActionType
    {
        Move,    // move to an adjacent tile
        Attack,  // attack an entity on the target tile
        Wait,    // skip turn, regenerate a small amount of HP/mana
        UseItem, // consume an item from inventory
        Interact // open chest, door, use stairs
    }
}
