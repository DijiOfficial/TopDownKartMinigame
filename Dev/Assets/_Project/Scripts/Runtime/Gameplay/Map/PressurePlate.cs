using System.Collections.Generic;
using UnityEngine;

namespace Battlekart
{
    public class PressurePlate : MonoBehaviour
    {
        private const int RingSegments = 40;

        public static readonly List<PressurePlate> All = new();

        [Header("Walls")]
        [SerializeField] private BarrierWall[] _walls = System.Array.Empty<BarrierWall>();
        [SerializeField] private ReverseWall[] _reverseWalls = System.Array.Empty<ReverseWall>();
        [SerializeField] private float _wallDuration = 4.0f;

        [Header("Plate")]
        [SerializeField] private float _radius = 1.8f;
        [SerializeField] private float _cooldown = 8.0f;

        [Header("Visuals")]
        [SerializeField] private Color _color = new Color(1.0f, 0.8f, 0.2f);
        [SerializeField] private Color _cooldownColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private float _plateThickness = 0.05f;
        [SerializeField] private float _ringWidth = 0.15f;
        [SerializeField] private float _floorHeight = 0.1f;
        [SerializeField] private float _pulseSpeed = 2.0f;

        private Renderer _plateRenderer;
        private LineRenderer _ring;
        private float _cooldownTimer;

        private void Awake()
        {
            GameObject plateObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plateObject.name = "Plate Visual";
            plateObject.transform.SetParent(transform, false);
            plateObject.transform.localPosition = Vector3.up * (_plateThickness * 0.5f);
            plateObject.transform.localScale = new Vector3(_radius * 2.0f, _plateThickness * 0.5f, _radius * 2.0f);
            Destroy(plateObject.GetComponent<Collider>());
            _plateRenderer = plateObject.GetComponent<Renderer>();

            _ring = FloorLine.Create("Plate Ring", _ringWidth);

            foreach (BarrierWall wall in _walls)
            {
                if (wall != null)
                {
                    wall.Color = _color;
                }
            }

            foreach (ReverseWall wall in _reverseWalls)
            {
                if (wall != null)
                {
                    wall.Color = _color;
                }
            }
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
            FloorLine.Destroy(_ring);
        }

        public void ResetPlate()
        {
            _cooldownTimer = 0.0f;
            foreach (BarrierWall wall in _walls)
            {
                if (wall != null)
                {
                    wall.Lower();
                }
            }

            foreach (ReverseWall wall in _reverseWalls)
            {
                if (wall != null)
                {
                    wall.ResetWall();
                }
            }
        }

        private void Update()
        {
            _cooldownTimer = Mathf.Max(0.0f, _cooldownTimer - Time.deltaTime);
            if (MapElements.Active && _cooldownTimer <= 0.0f && IsKartOnPlate())
            {
                Activate();
            }

            UpdateVisuals();
        }

        private bool IsKartOnPlate()
        {
            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                if (carrier.IsHidden)
                {
                    continue;
                }

                Vector3 offset = carrier.transform.position - transform.position;
                offset.y = 0.0f;
                if (offset.magnitude <= _radius)
                {
                    return true;
                }
            }

            return false;
        }

        private void Activate()
        {
            _cooldownTimer = _cooldown;
            AudioManager.Play(eSfx.PlatePress);
            foreach (BarrierWall wall in _walls)
            {
                if (wall != null)
                {
                    wall.Raise(_wallDuration);
                }
            }

            foreach (ReverseWall wall in _reverseWalls)
            {
                if (wall != null)
                {
                    wall.Open(_wallDuration);
                }
            }
        }

        private void UpdateVisuals()
        {
            bool ready = _cooldownTimer <= 0.0f && MapElements.Active;
            RendererTint.Apply(_plateRenderer, ready ? _color : _cooldownColor);

            Vector3 center = new Vector3(transform.position.x, transform.position.y + _floorHeight, transform.position.z);
            _ring.enabled = true;
            if (ready)
            {
                Color pulse = _color;
                pulse.a = Mathf.Lerp(0.4f, 1.0f, (Mathf.Sin(Time.time * Mathf.PI * 2.0f * _pulseSpeed) + 1.0f) * 0.5f);
                FloorLine.SetColor(_ring, pulse);
                FloorLine.SetArc(_ring, center, _radius, 0.0f, 360.0f, RingSegments);
                return;
            }

            float progress = _cooldown > 0.0f ? 1.0f - _cooldownTimer / _cooldown : 1.0f;
            FloorLine.SetColor(_ring, _color);
            FloorLine.SetArc(_ring, center, _radius, 0.0f, 360.0f * progress, RingSegments);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _color;
            Gizmos.DrawWireSphere(transform.position, _radius);
            foreach (BarrierWall wall in _walls)
            {
                if (wall != null)
                {
                    Gizmos.DrawLine(transform.position, wall.transform.position);
                }
            }

            foreach (ReverseWall wall in _reverseWalls)
            {
                if (wall != null)
                {
                    Gizmos.DrawLine(transform.position, wall.transform.position);
                }
            }
        }
    }
}
