using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RogueLikeDungeon.Combat
{
    /// <summary>
    /// Manages turn-based combat for a roguelike dungeon crawler.
    /// Maintains a turn queue of actable entities, handles turn resolution,
    /// and notifies all registered listeners of turn events.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        // ── Events ───────────────────────────────────────────────────────
        /// <summary>Invoked when turn resolution begins (before player acts).</summary>
        public event Action OnTurnResolutionStart;

        /// <summary>Invoked when turn resolution ends (after all enemies act).</summary>
        public event Action OnTurnResolutionEnd;

        /// <summary>Invoked when a new turn begins for an entity.</summary>
        public event Action<IActable> OnEntityTurnStart;

        /// <summary>Invoked when an entity's turn ends.</summary>
        public event Action<IActable> OnEntityTurnEnd;

        // ── Inspector Fields ─────────────────────────────────────────────
        [Header("Configuration")]
        [SerializeField] private bool _debugMode = false;

        // ── Private State ─────────────────────────────────────────────
        private IActable _playerActable;
        private List<IActable> _enemies = new List<IActable>();
        private IAction _pendingPlayerAction;
        private bool _resolutionInProgress = false;
        private int _nextActableID = 0;

        // ── Properties ───────────────────────────────────────────────────
        /// <summary>Gets the currently registered player actable entity.</summary>
        public IActable PlayerActable => _playerActable;

        /// <summary>Gets the count of registered enemies.</summary>
        public int EnemyCount => _enemies.Count;

        /// <summary>Gets whether turn resolution is currently in progress.</summary>
        public bool ResolutionInProgress => _resolutionInProgress;

        // ── Singleton Instance ───────────────────────────────────────────
        public static TurnManager Instance { get; private set; }

        // ── Unity Callbacks ──────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Public API ───────────────────────────────────────────────────
        /// <summary>Registers a player as the active actable entity.</summary>
        /// <param name="player">The player's IActable implementation.</param>
        public void RegisterPlayer(IActable player)
        {
            if (player == null)
            {
                Debug.LogError("[TurnManager] Cannot register null player!");
                return;
            }

            _playerActable = player;
            _nextActableID = 1;

            if (_debugMode)
                Debug.Log($"[TurnManager] Player registered (ID: {_playerActable.ActableID})");
        }

        /// <summary>Unregisters the current player.</summary>
        public void UnregisterPlayer()
        {
            if (_playerActable == null)
                return;

            _playerActable = null;
            _nextActableID = 0;

            if (_debugMode)
                Debug.Log("[TurnManager] Player unregistered");
        }

        /// <summary>Registers an enemy as an actable entity.</summary>
        /// <param name="enemy">The enemy's IActable implementation.</param>
        public void RegisterEnemy(IActable enemy)
        {
            if (enemy == null)
            {
                Debug.LogError("[TurnManager] Cannot register null enemy!");
                return;
            }

            if (_enemies.Contains(enemy))
            {
                Debug.LogWarning("[TurnManager] Enemy already registered!");
                return;
            }

            _enemies.Add(enemy);
            _enemies.Sort((a, b) => b.Speed.CompareTo(a.Speed)); // Sort by speed descending

            if (_debugMode)
                Debug.Log($"[TurnManager] Enemy registered (ID: {enemy.ActableID}, Speed: {enemy.Speed}). Total enemies: {_enemies.Count}");
        }

        /// <summary>Unregisters an enemy from the turn queue.</summary>
        /// <param name="enemy">The enemy to remove.</param>
        public void UnregisterEnemy(IActable enemy)
        {
            if (enemy == null)
                return;

            if (_enemies.Remove(enemy))
            {
                if (_debugMode)
                    Debug.Log($"[TurnManager] Enemy unregistered (ID: {enemy.ActableID}). Remaining: {_enemies.Count}");
            }
        }

        /// <summary>
        /// Submits the player's action and triggers full turn resolution.
        /// Player acts first, then all enemies act in speed order.
        /// </summary>
        /// <param name="action">The player's chosen action.</param>
        public void SubmitPlayerAction(IAction action)
        {
            if (_playerActable == null)
            {
                Debug.LogError("[TurnManager] Cannot submit player action: player not registered!");
                return;
            }

            if (_resolutionInProgress)
            {
                Debug.LogWarning("[TurnManager] Turn resolution already in progress!");
                return;
            }

            if (action == null)
            {
                Debug.LogError("[TurnManager] Cannot submit null action!");
                return;
            }

            _pendingPlayerAction = action;

            if (_debugMode)
                Debug.Log($"[TurnManager] Player action submitted: {action.ActionType}");

            ResolveTurn();
        }

        /// <summary>Gets all registered enemies in speed order (highest speed first).</summary>
        /// <returns>A copy of the enemies list sorted by speed descending.</returns>
        public List<IActable> GetEnemiesInSpeedOrder()
        {
            return new List<IActable>(_enemies); // Already sorted during registration/update
        }

        // ── Private Methods ──────────────────────────────────────────────
        /// <summary>Resolves a complete turn: player action followed by all enemy actions.</summary>
        private void ResolveTurn()
        {
            _resolutionInProgress = true;

            if (_debugMode)
                Debug.Log("[TurnManager] ═══ TURN RESOLUTION START ═══");

            // Signal turn resolution start
            OnTurnResolutionStart?.Invoke();

            // Player acts first
            if (_playerActable != null && _pendingPlayerAction != null)
            {
                ExecuteEntityTurn(_playerActable, _pendingPlayerAction);
                _pendingPlayerAction = null;
            }

            // All enemies act in speed order (highest speed first)
            foreach (IActable enemy in _enemies)
            {
                if (enemy == null) continue;

                // In a full implementation, AI would decide the action here
                // For now, we call OnTurnStart and ExecuteAction with a default action
                IAction enemyAction = GenerateDefaultEnemyAction(enemy);
                ExecuteEntityTurn(enemy, enemyAction);
            }

            // Signal turn resolution end
            OnTurnResolutionEnd?.Invoke();

            if (_debugMode)
                Debug.Log("[TurnManager] ═══ TURN RESOLUTION END ═══");

            _resolutionInProgress = false;
        }

        /// <summary>Executes a single entity's turn (OnTurnStart, ExecuteAction, OnTurnEnd).</summary>
        /// <param name="entity">The entity taking a turn.</param>
        /// <param name="action">The action to execute.</param>
        private void ExecuteEntityTurn(IActable entity, IAction action)
        {
            if (entity == null)
                return;

            entity.OnTurnStart();
            OnEntityTurnStart?.Invoke(entity);

            if (_debugMode)
                Debug.Log($"[TurnManager] Entity {entity.ActableID} turn start (Speed: {entity.Speed})");

            if (action != null)
            {
                entity.ExecuteAction(action);

                if (_debugMode)
                    Debug.Log($"[TurnManager] Entity {entity.ActableID} executed action: {action.ActionType}");
            }

            entity.OnTurnEnd();
            OnEntityTurnEnd?.Invoke(entity);

            if (_debugMode)
                Debug.Log($"[TurnManager] Entity {entity.ActableID} turn end");
        }

        /// <summary>Generates a default idle action for an enemy (placeholder for AI decision).</summary>
        /// <param name="enemy">The enemy to generate an action for.</param>
        /// <returns>A default idle action.</returns>
        private IAction GenerateDefaultEnemyAction(IActable enemy)
        {
            // This is a placeholder. In a full implementation, this would call an AI system
            // to decide the enemy's action based on game state.
            return new DefaultAction(ActionType.Idle, Vector2.zero, null);
        }

        /// <summary>Clears all registered entities from the turn queue.</summary>
        public void ClearAllActables()
        {
            _playerActable = null;
            _enemies.Clear();
            _pendingPlayerAction = null;

            if (_debugMode)
                Debug.Log("[TurnManager] All actables cleared");
        }
    }

    /// <summary>A simple default action implementation for basic turn resolution.</summary>
    public class DefaultAction : IAction
    {
        public ActionType ActionType { get; }
        public Vector2 TargetPosition { get; }
        public object ActionData { get; }

        public DefaultAction(ActionType actionType, Vector2 targetPosition, object actionData)
        {
            ActionType = actionType;
            TargetPosition = targetPosition;
            ActionData = actionData;
        }
    }
}
