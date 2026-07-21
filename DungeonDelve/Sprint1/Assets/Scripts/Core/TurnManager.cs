using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonDelve.Core
{
    /// <summary>
    /// Central turn resolution system. Maintains an ordered list of IActable entities,
    /// tracks whose turn it is, and drives each entity's action each round.
    ///
    /// Sequence per round:
    ///   1. OnTurnResolutionStart fires
    ///   2. Player acts (TurnManager waits for SubmitPlayerAction)
    ///   3. All enemies act in descending Speed order
    ///   4. OnTurnResolutionEnd fires
    ///   5. Repeat
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────
        public static TurnManager Instance { get; private set; }

        // ── Inspector Fields ─────────────────────────────────────────────
        [SerializeField] private bool _debugLogging = false;

        // ── Events ───────────────────────────────────────────────────────
        /// <summary>Fires before any entity acts this round.</summary>
        public event Action OnTurnResolutionStart;

        /// <summary>Fires after all entities have acted this round.</summary>
        public event Action OnTurnResolutionEnd;

        /// <summary>Fires when it becomes the player's time to act.</summary>
        public event Action OnPlayerTurnReady;

        // ── Private State ─────────────────────────────────────────────
        private IActable         _player;
        private List<IActable>   _enemies        = new List<IActable>();
        private IAction          _pendingAction;
        private bool             _waitingForPlayer;
        private int              _roundNumber;

        // ── Properties ───────────────────────────────────────────────────
        /// <summary>True while TurnManager is waiting on player input.</summary>
        public bool IsPlayerTurn => _waitingForPlayer;

        public int RoundNumber => _roundNumber;

        // ── Unity Callbacks ──────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Public API ───────────────────────────────────────────────────

        /// <summary>
        /// Register an entity to participate in turns. Call this from the entity's
        /// Start() or when it spawns. Pass isPlayer=true for exactly one entity.
        /// </summary>
        public void RegisterEntity(IActable entity, bool isPlayer = false)
        {
            if (entity == null) { Debug.LogWarning("[TurnManager] RegisterEntity: null entity"); return; }

            if (isPlayer)
            {
                _player = entity;
                Log($"Registered player: {entity.ActableName}");
            }
            else
            {
                if (!_enemies.Contains(entity))
                {
                    _enemies.Add(entity);
                    Log($"Registered enemy: {entity.ActableName} (total: {_enemies.Count})");
                }
            }
        }

        /// <summary>Remove an entity from the turn queue. Call this on death or despawn.</summary>
        public void UnregisterEntity(IActable entity)
        {
            if (entity == null) return;

            if (entity == _player)
            {
                _player = null;
                Log("Unregistered player");
            }
            else
            {
                _enemies.Remove(entity);
                Log($"Unregistered enemy: {entity.ActableName} (remaining: {_enemies.Count})");
            }
        }

        /// <summary>
        /// Called by PlayerController when the player has chosen their action.
        /// Providing an action here unblocks turn resolution.
        /// </summary>
        public void SubmitPlayerAction(IAction action)
        {
            if (!_waitingForPlayer)
            {
                Debug.LogWarning("[TurnManager] SubmitPlayerAction called but it is not the player's turn.");
                return;
            }

            _pendingAction    = action;
            _waitingForPlayer = false;
            ResolveTurn();
        }

        /// <summary>
        /// Begin the first round. Call this once the Gameplay scene is fully loaded
        /// and all entities are registered.
        /// </summary>
        public void BeginRounds()
        {
            _roundNumber = 0;
            StartNextRound();
        }

        /// <summary>Remove all entities and reset state (call on scene unload).</summary>
        public void Reset()
        {
            _player           = null;
            _enemies.Clear();
            _pendingAction    = null;
            _waitingForPlayer = false;
            _roundNumber      = 0;
        }

        // ── Private Methods ──────────────────────────────────────────────
        private void StartNextRound()
        {
            _roundNumber++;
            Log($"── Round {_roundNumber} start ──");
            OnTurnResolutionStart?.Invoke();
            BeginPlayerTurn();
        }

        private void BeginPlayerTurn()
        {
            if (_player == null || !_player.CanAct)
            {
                // Player is dead or missing — skip directly to enemies (shouldn't normally happen).
                ResolveEnemyTurns();
                return;
            }

            _waitingForPlayer = true;
            _player.OnTurnStarted?.Invoke();  // fires player's turn-start effects (regeneration, debuffs, etc.)
            Log("Waiting for player action...");
            OnPlayerTurnReady?.Invoke();
            // Execution pauses here until SubmitPlayerAction() is called.
        }

        private void ResolveTurn()
        {
            // Execute the player's submitted action.
            if (_pendingAction != null)
            {
                ExecuteAction(_pendingAction);
                _player?.OnTurnEnded?.Invoke();
                _pendingAction = null;
            }

            ResolveEnemyTurns();
        }

        private void ResolveEnemyTurns()
        {
            // Sort enemies by Speed descending so fast enemies act first.
            _enemies.Sort((a, b) => b.Speed.CompareTo(a.Speed));

            // Iterate over a snapshot to handle deaths mid-resolution safely.
            var snapshot = new List<IActable>(_enemies);
            foreach (var enemy in snapshot)
            {
                if (!_enemies.Contains(enemy)) continue;  // died this round
                if (!enemy.CanAct) continue;

                enemy.OnTurnStarted?.Invoke();
                enemy.RequestTurn();   // EnemyAI executes synchronously and returns
                enemy.OnTurnEnded?.Invoke();
            }

            EndRound();
        }

        private void EndRound()
        {
            Log($"── Round {_roundNumber} end ──");
            OnTurnResolutionEnd?.Invoke();
            StartNextRound();
        }

        private void ExecuteAction(IAction action)
        {
            Log($"Executing {action.ActionType} by {action.Actor?.ActableName ?? "unknown"}");
            // Action execution is handled by the action itself or the system that listens
            // to it. TurnManager only drives the sequence — it doesn't interpret actions.
            // Combat system, movement system, etc. listen to the events fired here.
        }

        private void Log(string msg)
        {
            if (_debugLogging) Debug.Log($"[TurnManager] {msg}");
        }
    }
}
