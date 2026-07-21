using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeDungeonCrawler.TurnSystem
{
    /// <summary>
    /// Manages the turn-based system for the roguelike dungeon crawler.
    /// Handles turn queue, entity registration, player action submission, and turn resolution.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private bool debugLogging = true;

        /// <summary>
        /// Fired when turn resolution begins.
        /// </summary>
        public event Action OnTurnResolutionStart;

        /// <summary>
        /// Fired when turn resolution completes.
        /// </summary>
        public event Action OnTurnResolutionEnd;

        private List<IActable> registeredEntities = new List<IActable>();
        private Queue<IActable> turnQueue = new Queue<IActable>();
        private Dictionary<int, IActable> entityRegistry = new Dictionary<int, IActable>();

        private IActable currentActor;
        private bool isResolvingTurn = false;
        private IAction pendingPlayerAction;

        private static TurnManager instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        public static TurnManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("TurnManager instance not found. Ensure it is present in the scene.");
                }
                return instance;
            }
        }

        private void OnEnable()
        {
            // Ensure the turn queue is built when manager is enabled
            if (registeredEntities.Count > 0 && turnQueue.Count == 0)
            {
                RebuildTurnQueue();
            }
        }

        /// <summary>
        /// Registers an entity to participate in the turn system.
        /// </summary>
        /// <param name="actable">The entity to register.</param>
        public void RegisterEntity(IActable actable)
        {
            if (actable == null)
            {
                Debug.LogError("Cannot register null entity.");
                return;
            }

            if (entityRegistry.ContainsKey(actable.EntityID))
            {
                Debug.LogWarning($"Entity with ID {actable.EntityID} is already registered.");
                return;
            }

            registeredEntities.Add(actable);
            entityRegistry[actable.EntityID] = actable;

            if (debugLogging)
            {
                Debug.Log($"[TurnManager] Entity registered: {actable.EntityID} (Speed: {actable.Speed})");
            }

            // Rebuild queue to maintain speed-based ordering
            RebuildTurnQueue();
        }

        /// <summary>
        /// Unregisters an entity from the turn system.
        /// </summary>
        /// <param name="actable">The entity to unregister.</param>
        public void UnregisterEntity(IActable actable)
        {
            if (actable == null)
            {
                Debug.LogError("Cannot unregister null entity.");
                return;
            }

            if (!entityRegistry.ContainsKey(actable.EntityID))
            {
                Debug.LogWarning($"Entity with ID {actable.EntityID} is not registered.");
                return;
            }

            registeredEntities.Remove(actable);
            entityRegistry.Remove(actable.EntityID);

            // If the unregistered entity is currently acting, move to next
            if (currentActor == actable)
            {
                currentActor = null;
            }

            if (debugLogging)
            {
                Debug.Log($"[TurnManager] Entity unregistered: {actable.EntityID}");
            }

            // Rebuild queue to maintain proper state
            RebuildTurnQueue();
        }

        /// <summary>
        /// Submits the player's action and triggers turn resolution.
        /// </summary>
        /// <param name="action">The action the player wants to perform.</param>
        public void SubmitPlayerAction(IAction action)
        {
            if (action == null)
            {
                Debug.LogError("Cannot submit null action.");
                return;
            }

            if (isResolvingTurn)
            {
                Debug.LogWarning("Cannot submit action while a turn is already resolving.");
                return;
            }

            pendingPlayerAction = action;

            if (debugLogging)
            {
                Debug.Log($"[TurnManager] Player action submitted: {action.ActionType}");
            }

            // Begin turn resolution
            ResolveTurn();
        }

        /// <summary>
        /// Resolves a complete turn: executes player action, then all enemies in speed order.
        /// </summary>
        private void ResolveTurn()
        {
            if (isResolvingTurn)
            {
                Debug.LogWarning("Turn resolution is already in progress.");
                return;
            }

            isResolvingTurn = true;

            // Invoke turn resolution start event
            OnTurnResolutionStart?.Invoke();

            if (debugLogging)
            {
                Debug.Log("[TurnManager] Turn resolution started.");
            }

            // Execute player action (assumed to be the first entity or a designated player)
            ExecutePlayerTurn();

            // Execute all enemy turns in speed order
            ExecuteEnemyTurns();

            // Invoke turn resolution end event
            OnTurnResolutionEnd?.Invoke();

            if (debugLogging)
            {
                Debug.Log("[TurnManager] Turn resolution completed.");
            }

            isResolvingTurn = false;

            // Reset pending action
            pendingPlayerAction = null;

            // Optionally rebuild queue for next turn
            RebuildTurnQueue();
        }

        /// <summary>
        /// Executes the player's turn with the pending action.
        /// </summary>
        private void ExecutePlayerTurn()
        {
            if (registeredEntities.Count == 0)
            {
                Debug.LogWarning("[TurnManager] No entities registered for turn execution.");
                return;
            }

            // Assume first entity is the player (or find player by ID)
            IActable playerEntity = registeredEntities.FirstOrDefault();

            if (playerEntity == null || !playerEntity.CanAct())
            {
                Debug.LogWarning("[TurnManager] Player entity cannot act.");
                return;
            }

            currentActor = playerEntity;
            playerEntity.OnTurnStarted();

            if (debugLogging)
            {
                Debug.Log($"[TurnManager] Player turn started (EntityID: {playerEntity.EntityID})");
            }

            // Execute the player action
            if (pendingPlayerAction != null)
            {
                playerEntity.ExecuteTurn(pendingPlayerAction);

                if (debugLogging)
                {
                    Debug.Log($"[TurnManager] Player executed action: {pendingPlayerAction.ActionType}");
                }
            }

            playerEntity.OnTurnEnded();

            if (debugLogging)
            {
                Debug.Log($"[TurnManager] Player turn ended (EntityID: {playerEntity.EntityID})");
            }

            currentActor = null;
        }

        /// <summary>
        /// Executes all enemy turns in speed order (highest speed first).
        /// </summary>
        private void ExecuteEnemyTurns()
        {
            // Get all enemies sorted by speed (descending)
            List<IActable> enemies = registeredEntities
                .Where(e => e.CanAct())
                .OrderByDescending(e => e.Speed)
                .ToList();

            if (debugLogging)
            {
                Debug.Log($"[TurnManager] Starting enemy turn execution. Enemy count: {enemies.Count}");
            }

            foreach (IActable enemy in enemies)
            {
                // Skip the player (first entity)
                if (enemy == registeredEntities.FirstOrDefault())
                {
                    continue;
                }

                if (!enemy.CanAct())
                {
                    if (debugLogging)
                    {
                        Debug.Log($"[TurnManager] Skipping inactive enemy: {enemy.EntityID}");
                    }
                    continue;
                }

                currentActor = enemy;
                enemy.OnTurnStarted();

                if (debugLogging)
                {
                    Debug.Log($"[TurnManager] Enemy turn started (EntityID: {enemy.EntityID}, Speed: {enemy.Speed})");
                }

                // For enemies, you might want to implement AI decision logic here
                // For now, we'll let the enemy decide its own action
                IAction enemyAction = GetEnemyAction(enemy);
                if (enemyAction != null)
                {
                    enemy.ExecuteTurn(enemyAction);

                    if (debugLogging)
                    {
                        Debug.Log($"[TurnManager] Enemy executed action: {enemyAction.ActionType}");
                    }
                }

                enemy.OnTurnEnded();

                if (debugLogging)
                {
                    Debug.Log($"[TurnManager] Enemy turn ended (EntityID: {enemy.EntityID})");
                }

                currentActor = null;
            }
        }

        /// <summary>
        /// Gets the next action for an enemy. Override or extend this for AI logic.
        /// </summary>
        /// <param name="enemy">The enemy entity.</param>
        /// <returns>The action the enemy should perform.</returns>
        private IAction GetEnemyAction(IActable enemy)
        {
            // This is a placeholder for AI decision logic.
            // In a real implementation, you'd have enemies decide their actions here.
            return null;
        }

        /// <summary>
        /// Rebuilds the turn queue based on current registered entities and their speeds.
        /// </summary>
        private void RebuildTurnQueue()
        {
            turnQueue.Clear();

            // Sort entities by speed (descending) and add to queue
            var sortedEntities = registeredEntities
                .Where(e => e != null && e.CanAct())
                .OrderByDescending(e => e.Speed)
                .ToList();

            foreach (IActable entity in sortedEntities)
            {
                turnQueue.Enqueue(entity);
            }

            if (debugLogging && sortedEntities.Count > 0)
            {
                Debug.Log($"[TurnManager] Turn queue rebuilt with {sortedEntities.Count} entities.");
            }
        }

        /// <summary>
        /// Gets the current actor (entity whose turn it is).
        /// </summary>
        public IActable GetCurrentActor()
        {
            return currentActor;
        }

        /// <summary>
        /// Checks if a turn is currently being resolved.
        /// </summary>
        public bool IsResolvingTurn()
        {
            return isResolvingTurn;
        }

        /// <summary>
        /// Gets all registered entities.
        /// </summary>
        public IReadOnlyList<IActable> GetRegisteredEntities()
        {
            return registeredEntities.AsReadOnly();
        }

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="entityID">The entity's ID.</param>
        /// <returns>The entity, or null if not found.</returns>
        public IActable GetEntityByID(int entityID)
        {
            if (entityRegistry.TryGetValue(entityID, out IActable entity))
            {
                return entity;
            }
            return null;
        }

        /// <summary>
        /// Clears all registered entities and resets the turn system.
        /// </summary>
        public void ClearAllEntities()
        {
            registeredEntities.Clear();
            entityRegistry.Clear();
            turnQueue.Clear();
            currentActor = null;
            pendingPlayerAction = null;

            if (debugLogging)
            {
                Debug.Log("[TurnManager] All entities cleared.");
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
