using System;
using UnityEngine;

namespace RogueLikeDungeon.Combat
{
    /// <summary>Represents an action that can be submitted by an actable entity.</summary>
    public interface IAction
    {
        /// <summary>Gets the type of action being performed.</summary>
        ActionType ActionType { get; }

        /// <summary>Gets the target position for this action (if applicable).</summary>
        Vector2 TargetPosition { get; }

        /// <summary>Gets any additional data needed for action resolution.</summary>
        object ActionData { get; }
    }

    /// <summary>Represents an entity that can act during a turn (player or enemy).</summary>
    public interface IActable
    {
        /// <summary>Gets the unique identifier for this actable entity.</summary>
        int ActableID { get; }

        /// <summary>Gets the speed/priority value for turn ordering (higher = acts first).</summary>
        float Speed { get; }

        /// <summary>Called when it becomes this entity's turn to act.</summary>
        void OnTurnStart();

        /// <summary>Called when this entity's turn ends.</summary>
        void OnTurnEnd();

        /// <summary>Executes the submitted action during turn resolution.</summary>
        /// <param name="action">The action to execute.</param>
        void ExecuteAction(IAction action);
    }

    /// <summary>Defines the types of actions available in combat.</summary>
    public enum ActionType
    {
        Move,
        Attack,
        Ability,
        Defend,
        Item,
        Idle
    }
}
