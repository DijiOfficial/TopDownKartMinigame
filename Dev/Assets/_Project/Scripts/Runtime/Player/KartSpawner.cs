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

        private PlayerInputManager _manager;
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

        private void OnPlayerJoined(PlayerInput player)
        {
            Players.Add(player);
            int index = player.playerIndex;
            player.gameObject.name = $"Kart P{index + 1}";

            if (_spawnPoints is { Length: > 0 })
            {
                Transform spawn = _spawnPoints[index % _spawnPoints.Length];
                if (player.TryGetComponent(out Rigidbody body))
                {
                    body.position = spawn.position;
                    body.rotation = spawn.rotation;
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            }

            Color color = _playerColors[index % _playerColors.Length];
            Tint(player.gameObject, color);

            if (player.TryGetComponent(out BalloonCarrier carrier))
            {
                carrier.Initialize(index, color);
            }

            Debug.Log($"P{index + 1} joined with {player.currentControlScheme} ({string.Join(", ", player.devices)})");
        }

        private void OnPlayerLeft(PlayerInput player)
        {
            Players.Remove(player);
        }

        public static void Tint(GameObject kart, Color color)
        {
            Renderer[] renderers = kart.TryGetComponent(out KartTint tint) && tint.Renderers.Length > 0 ? tint.Renderers : kart.GetComponentsInChildren<MeshRenderer>();
            RendererTint.Apply(renderers, color);
        }
    }
}
