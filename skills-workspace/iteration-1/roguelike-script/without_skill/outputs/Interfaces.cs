using UnityEngine;

namespace RoguelikeDungeonCrawler.TurnSystem
{
    /// <summary>
    /// Represents an action that can be performed by an entity (player or enemy).
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Executes the action.
        /// </summary>
        void Execute();

        /// <summary>
        /// Gets the action type for identification and logging.
        /// </summary>
        string ActionType { get; }
    }

    /// <summary>
    /// Interface for entities that can take turns in the turn-based system.
    /// Must be implemented by Player, Enemy, and other entities that participate in combat.
    /// </summary>
    public interface IActable
    {
        /// <summary>
        /// Gets the speed stat of this entity, used to determine turn order.
        /// Higher speed = acts earlier in the turn queue.
        /// </summary>
        int Speed { get; }

        /// <summary>
        /// Gets the unique identifier for this entity.
        /// </summary>
        int EntityID { get; }

        /// <summary>
        /// Executes the entity's turn with the given action.
        /// </summary>
        /// <param name="action">The action to perform this turn.</param>
        void ExecuteTurn(IAction action);

        /// <summary>
        /// Called when it becomes this entity's turn.
        /// </summary>
        void OnTurnStarted();

        /// <summary>
        /// Called when this entity's turn has ended.
        /// </summary>
        void OnTurnEnded();

        /// <summary>
        /// Checks if this entity is still able to act (e.g., not dead, stunned, etc.).
        /// </summary>
        bool CanAct();
    }
}
