using System.Collections.Generic;
using UnityEngine;

namespace Battlekart
{
    public class StealSystem : MonoBehaviour
    {
        [Header("Zone")]
        [SerializeField] private float _innerRadius = 1.0f;
        [SerializeField] private float _outerRadius = 8.0f;
        [SerializeField, Range(10.0f, 180.0f)] private float _halfAngle = 30.0f;

        [Header("Timing")]
        [SerializeField] private float _fillDuration = 0.4f;

        [Header("Visuals")]
        [SerializeField] private bool _showZones = true;
        [SerializeField] private Material _lineMaterial;
        [SerializeField] private float _visualHeight = 0.1f;
        [SerializeField] private float _outlineWidth = 0.05f;
        [SerializeField] private float _fillWidth = 0.35f;
        [SerializeField, Range(0.0f, 1.0f)] private float _outlineAlpha = 0.35f;

        private readonly Dictionary<(BalloonCarrier attacker, BalloonCarrier target), float> _progress = new();
        private readonly Dictionary<BalloonCarrier, StealZoneVisual> _visuals = new();
        private readonly List<(BalloonCarrier attacker, BalloonCarrier target)> _staleKeys = new();

        public bool StealingEnabled { get; set; } = true;

        private void Awake()
        {
            if (_lineMaterial == null)
            {
                _lineMaterial = new Material(Shader.Find("Sprites/Default"));
            }
        }

        private void Update()
        {
            List<BalloonCarrier> carriers = BalloonCarrier.All;

            if (StealingEnabled)
            {
                UpdateProgress(carriers);
            }
            else
            {
                _progress.Clear();
            }

            UpdateVisuals(carriers);
        }

        private void UpdateProgress(List<BalloonCarrier> carriers)
        {
            _staleKeys.Clear();
            foreach ((BalloonCarrier attacker, BalloonCarrier target) key in _progress.Keys)
            {
                _staleKeys.Add(key);
            }

            for (int t = 0; t < carriers.Count; t++)
            {
                BalloonCarrier target = carriers[t];
                if (target.CanBeStolenFrom == false)
                {
                    continue;
                }

                for (int a = 0; a < carriers.Count; a++)
                {
                    BalloonCarrier attacker = carriers[a];
                    if (attacker == target || IsInZone(attacker, target) == false)
                    {
                        continue;
                    }

                    (BalloonCarrier attacker, BalloonCarrier target) key = (attacker, target);
                    _staleKeys.Remove(key);
                    _progress.TryGetValue(key, out float progress);
                    progress += Time.deltaTime;

                    if (progress >= _fillDuration)
                    {
                        Steal(attacker, target);
                        _progress.Remove(key);
                        break;
                    }

                    _progress[key] = progress;
                }
            }

            foreach ((BalloonCarrier attacker, BalloonCarrier target) key in _staleKeys)
            {
                _progress.Remove(key);
            }
        }

        private bool IsInZone(BalloonCarrier attacker, BalloonCarrier target)
        {
            Vector3 local = target.transform.InverseTransformPoint(attacker.NosePosition);
            Vector2 flat = new Vector2(local.x, local.z);
            float distance = flat.magnitude;
            if (distance < _innerRadius || distance > _outerRadius)
            {
                return false;
            }

            return Vector2.Angle(Vector2.down, flat) <= _halfAngle;
        }

        private void Steal(BalloonCarrier attacker, BalloonCarrier target)
        {
            if (target.TryTakeBalloon(out Vector3 balloonPosition))
            {
                attacker.GiveBalloon(balloonPosition);
                AudioManager.Play(eSfx.Steal);
            }
        }

        private void UpdateVisuals(List<BalloonCarrier> carriers)
        {
            foreach (BalloonCarrier carrier in carriers)
            {
                if (_visuals.TryGetValue(carrier, out StealZoneVisual visual) == false)
                {
                    visual = new StealZoneVisual(carrier.transform, _lineMaterial, _visualHeight, _outlineWidth, _fillWidth);
                    _visuals.Add(carrier, visual);
                }

                float bestProgress = 0.0f;
                Color fillColor = Color.white;
                foreach (KeyValuePair<(BalloonCarrier attacker, BalloonCarrier target), float> entry in _progress)
                {
                    if (entry.Key.target == carrier && entry.Value > bestProgress)
                    {
                        bestProgress = entry.Value;
                        fillColor = entry.Key.attacker.PlayerColor;
                    }
                }

                bool showOutline = _showZones && StealingEnabled && carrier.CanBeStolenFrom;
                Color outlineColor = carrier.PlayerColor;
                outlineColor.a = _outlineAlpha;

                visual.SetOutline(showOutline, _innerRadius, _outerRadius, _halfAngle, outlineColor);
                visual.SetFill(bestProgress / _fillDuration, (_innerRadius + _outerRadius) * 0.5f, _halfAngle, fillColor);
            }
        }
    }
}
