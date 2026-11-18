using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlekart
{
    public class BalloonCarrier : MonoBehaviour
    {
        public static readonly List<BalloonCarrier> All = new();

        [Header("Balloons")]
        [SerializeField] private GameObject _balloonPrefab;
        [SerializeField] private int _startingBalloons = 3;

        [Header("Trail")]
        [SerializeField] private float _tailOffset = 1.5f;
        [SerializeField] private float _balloonHeight = 1.5f;
        [SerializeField] private float _balloonSpacing = 1.0f;
        [SerializeField] private float _followSharpness = 15.0f;

        [Header("Steal")]
        [SerializeField] private float _noseOffset = 1.2f;
        [SerializeField] private float _invulnerableDuration = 2.0f;
        [SerializeField] private float _blinkInterval = 0.1f;

        private readonly List<Transform> _balloons = new();
        private MeshRenderer[] _kartRenderers = Array.Empty<MeshRenderer>();
        private float _invulnerableTimer;

        public event Action<BalloonCarrier> OnCountChanged;

        public int PlayerIndex { get; private set; }
        public Color PlayerColor { get; private set; } = Color.white;
        public string PlayerName => $"P{PlayerIndex + 1}";
        public int Count => _balloons.Count;
        public bool IsInvulnerable => _invulnerableTimer > 0.0f;
        public bool CanBeStolenFrom => Count > 0 && IsInvulnerable == false;
        public Vector3 NosePosition => transform.position + transform.forward * _noseOffset;

        private Vector3 TailAnchor => transform.position - transform.forward * _tailOffset + Vector3.up * _balloonHeight;

        private void Awake()
        {
            _kartRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        private void OnEnable()
        {
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void OnDestroy()
        {
            ClearBalloons();
        }

        public void Initialize(int playerIndex, Color color)
        {
            PlayerIndex = playerIndex;
            PlayerColor = color;
            ResetBalloons();
        }

        public void ResetBalloons()
        {
            ClearBalloons();
            for (int i = 0; i < _startingBalloons; i++)
            {
                SpawnBalloon(TailAnchor - transform.forward * (_balloonSpacing * i));
            }
            SetInvulnerable(0.0f);
            OnCountChanged?.Invoke(this);
        }

        public bool TryTakeBalloon(out Vector3 balloonPosition)
        {
            balloonPosition = TailAnchor;
            if (Count == 0)
            {
                return false;
            }

            int last = _balloons.Count - 1;
            Transform balloon = _balloons[last];
            balloonPosition = balloon.position;
            _balloons.RemoveAt(last);
            Destroy(balloon.gameObject);

            SetInvulnerable(_invulnerableDuration);
            OnCountChanged?.Invoke(this);
            return true;
        }

        public void GiveBalloon(Vector3 spawnPosition)
        {
            SpawnBalloon(spawnPosition);
            OnCountChanged?.Invoke(this);
        }

        private void SpawnBalloon(Vector3 position)
        {
            GameObject balloon = Instantiate(_balloonPrefab, position, Quaternion.identity);
            balloon.name = $"Balloon {PlayerName}";
            if (balloon.TryGetComponent(out Renderer balloonRenderer))
            {
                RendererTint.Apply(balloonRenderer, PlayerColor);
            }
            _balloons.Add(balloon.transform);
        }

        private void ClearBalloons()
        {
            foreach (Transform balloon in _balloons)
            {
                if (balloon != null)
                {
                    Destroy(balloon.gameObject);
                }
            }
            _balloons.Clear();
        }

        private void SetInvulnerable(float duration)
        {
            _invulnerableTimer = duration;
            SetKartVisible(true);
        }

        private void Update()
        {
            UpdateInvulnerability();
        }

        private void LateUpdate()
        {
            UpdateTrail();
        }

        private void UpdateInvulnerability()
        {
            if (IsInvulnerable == false)
            {
                return;
            }

            _invulnerableTimer -= Time.deltaTime;
            if (IsInvulnerable)
            {
                bool visible = Mathf.FloorToInt(_invulnerableTimer / _blinkInterval) % 2 == 0;
                SetKartVisible(visible);
            }
            else
            {
                SetKartVisible(true);
            }
        }

        private void SetKartVisible(bool visible)
        {
            foreach (MeshRenderer kartRenderer in _kartRenderers)
            {
                kartRenderer.enabled = visible;
            }
        }

        private void UpdateTrail()
        {
            float blend = 1.0f - Mathf.Exp(-_followSharpness * Time.deltaTime);
            Vector3 leader = TailAnchor;

            foreach (Transform balloon in _balloons)
            {
                Vector3 offset = balloon.position - leader;
                offset.y = 0.0f;
                if (offset.sqrMagnitude < 0.0001f)
                {
                    offset = -transform.forward;
                }

                Vector3 target = leader + offset.normalized * _balloonSpacing;
                target.y = leader.y;
                balloon.position = Vector3.Lerp(balloon.position, target, blend);
                leader = balloon.position;
            }
        }
    }
}
