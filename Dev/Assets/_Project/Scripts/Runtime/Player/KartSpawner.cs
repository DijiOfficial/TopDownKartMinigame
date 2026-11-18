using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battlekart
{
    [RequireComponent(typeof(PlayerInputManager))]
    public class KartSpawner : MonoBehaviour
    {
        [SerializeField] private Transform[] _spawnPoints;

        [SerializeField] private Color[] _playerColors =
        {
            new Color(0.90f, 0.20f, 0.20f),
            new Color(0.20f, 0.45f, 0.95f),
            new Color(0.20f, 0.80f, 0.30f),
            new Color(0.98f, 0.85f, 0.15f),
            new Color(0.95f, 0.50f, 0.10f),
            new Color(0.65f, 0.30f, 0.90f),
            new Color(0.10f, 0.85f, 0.85f),
            new Color(0.95f, 0.40f, 0.75f),
            new Color(0.95f, 0.95f, 0.95f),
            new Color(0.55f, 0.35f, 0.20f),
            new Color(0.60f, 0.90f, 0.20f),
            new Color(0.25f, 0.25f, 0.30f),
        };

        [Header("Bots")]
        [SerializeField] private GameObject _botPrefab;
        [SerializeField, Range(0, 11)] private int _botCount = 0;

        private PlayerInputManager _manager;
        private readonly HashSet<int> _usedSlots = new();
        private readonly Dictionary<PlayerInput, int> _playerSlots = new();
        private readonly List<GameObject> _bots = new();

        public readonly List<PlayerInput> Players = new();

        private void Awake()
        {
            _manager = GetComponent<PlayerInputManager>();
        }

        private void OnEnable()
        {
            _manager.onPlayerJoined += OnPlayerJoined;
            _manager.onPlayerLeft += OnPlayerLeft;
        }

        private void OnDisable()
        {
            _manager.onPlayerJoined -= OnPlayerJoined;
            _manager.onPlayerLeft -= OnPlayerLeft;
        }

        public void SpawnBots()
        {
            _bots.RemoveAll(bot => bot == null);

            while (_bots.Count < _botCount && TryReserveSlot(out int slot))
            {
                GameObject bot = Instantiate(_botPrefab);
                SetupKart(bot, slot, $"CPU {slot + 1}");
                _bots.Add(bot);
            }
        }

        public bool TryGetHostPlayer(out PlayerInput host)
        {
            host = null;
            int lowestSlot = int.MaxValue;
            foreach (KeyValuePair<PlayerInput, int> entry in _playerSlots)
            {
                if (entry.Value < lowestSlot)
                {
                    lowestSlot = entry.Value;
                    host = entry.Key;
                }
            }

            return host is not null;
        }

        private void OnPlayerJoined(PlayerInput player)
        {
            if (TryReserveSlot(out int slot) == false)
            {
                Debug.LogWarning("No free kart slot left, removing the joined player.", this);
                Destroy(player.gameObject);
                return;
            }

            Players.Add(player);
            _playerSlots.Add(player, slot);
            SetupKart(player.gameObject, slot, $"P{slot + 1}");

            Debug.Log($"P{slot + 1} joined with {player.currentControlScheme} ({string.Join(", ", player.devices)})");
        }

        private void OnPlayerLeft(PlayerInput player)
        {
            Players.Remove(player);
            if (_playerSlots.Remove(player, out int slot))
            {
                _usedSlots.Remove(slot);
            }
        }

        private bool TryReserveSlot(out int slot)
        {
            for (slot = 0; slot < _playerColors.Length; slot++)
            {
                if (_usedSlots.Add(slot))
                {
                    return true;
                }
            }

            slot = -1;
            return false;
        }

        private void SetupKart(GameObject kart, int slot, string displayName)
        {
            kart.name = $"Kart {displayName}";
            MoveToSpawn(kart, slot);

            Color color = _playerColors[slot];
            Tint(kart, color);

            if (kart.TryGetComponent(out BalloonCarrier carrier))
            {
                carrier.Initialize(slot, color, displayName);
            }
        }

        public void RespawnAll()
        {
            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                MoveToSpawn(carrier.gameObject, carrier.PlayerIndex);
            }
        }

        private void MoveToSpawn(GameObject kart, int slot)
        {
            if (_spawnPoints is not { Length: > 0 })
            {
                return;
            }

            Transform spawn = _spawnPoints[slot % _spawnPoints.Length];
            if (kart.TryGetComponent(out Rigidbody body))
            {
                body.position = spawn.position;
                body.rotation = spawn.rotation;
                if (body.isKinematic == false)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }
            kart.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
        }

        public static void Tint(GameObject kart, Color color)
        {
            Renderer[] renderers = kart.TryGetComponent(out KartTint tint) && tint.Renderers.Length > 0 ? tint.Renderers : kart.GetComponentsInChildren<MeshRenderer>();
            RendererTint.Apply(renderers, color);
        }
    }
}
