using System.Collections.Generic;
using UnityEngine;

namespace Battlekart
{
    public class ItemPickup : MonoBehaviour
    {
        private const int RingSegments = 40;

        public static readonly List<ItemPickup> All = new();

        [Header("Items")]
        [SerializeField] private bool _allowBoost = true;
        [SerializeField] private bool _allowGrapple = true;
        [SerializeField] private bool _allowBomb = true;

        [Header("Timing")]
        [SerializeField] private float _pickupRadius = 2.5f;
        [SerializeField] private float _respawnTime = 9.0f;

        [Header("Visuals")]
        [SerializeField] private Transform _visual;
        [SerializeField] private float _visualSize = 0.8f;
        [SerializeField] private float _visualHeight = 1.0f;
        [SerializeField] private float _spinSpeed = 90.0f;
        [SerializeField] private float _bobAmplitude = 0.15f;
        [SerializeField] private float _ringRadius = 1.4f;
        [SerializeField] private float _ringWidth = 0.12f;
        [SerializeField] private float _floorHeight = 0.1f;

        [Header("Glow")]
        [SerializeField] private Color _glowColor = new Color(1.0f, 0.95f, 0.8f);
        [SerializeField] private float _emissionIntensity = 1.2f;
        [SerializeField] private float _lightIntensity = 1.5f;
        [SerializeField] private float _lightRange = 3.0f;

        private static readonly int _emissionColorId = Shader.PropertyToID("_EmissionColor");

        private readonly List<eItemType> _choices = new();
        private Vector3 _visualBasePosition;
        private LineRenderer _ring;
        private float _respawnTimer;

        public bool IsAvailable { get; private set; }

        private void Awake()
        {
            SetupVisual();

            _ring = FloorLine.Create("Pickup Respawn Ring", _ringWidth);
            ResetSpot();
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

        public void ResetSpot()
        {
            Respawn();
        }

        private void Update()
        {
            if (KartItems.ItemsEnabled == false)
            {
                _visual.gameObject.SetActive(false);
                _ring.enabled = false;
                return;
            }

            if (IsAvailable)
            {
                UpdateAvailable();
            }
            else
            {
                UpdateRespawning();
            }
        }

        private void UpdateAvailable()
        {
            _visual.gameObject.SetActive(true);
            _ring.enabled = false;
            _visual.localPosition = _visualBasePosition + Vector3.up * (Mathf.Sin(Time.time * Mathf.PI * 2.0f * 0.5f) * _bobAmplitude);
            _visual.Rotate(Vector3.up, _spinSpeed * Time.deltaTime, Space.World);

            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                if (carrier.IsHidden)
                {
                    continue;
                }

                Vector3 offset = carrier.transform.position - transform.position;
                offset.y = 0.0f;
                if (offset.magnitude > _pickupRadius)
                {
                    continue;
                }

                if (carrier.TryGetComponent(out KartItems items) && items.CanReceiveItem && items.TryGiveItem(RollItem()))
                {
                    IsAvailable = false;
                    AudioManager.Play(eSfx.Pickup);
                    _respawnTimer = _respawnTime;
                    return;
                }
            }
        }

        private void UpdateRespawning()
        {
            _visual.gameObject.SetActive(false);
            _respawnTimer -= Time.deltaTime;
            if (_respawnTimer <= 0.0f)
            {
                Respawn();
                return;
            }

            float progress = 1.0f - _respawnTimer / _respawnTime;
            _ring.enabled = true;
            FloorLine.SetColor(_ring, new Color(1.0f, 1.0f, 1.0f, 0.7f));
            Vector3 center = new Vector3(transform.position.x, transform.position.y + _floorHeight, transform.position.z);
            FloorLine.SetArc(_ring, center, _ringRadius, 0.0f, 360.0f * progress, RingSegments);
        }

        private void Respawn()
        {
            IsAvailable = true;
        }

        private eItemType RollItem()
        {
            _choices.Clear();
            if (_allowBoost)
            {
                _choices.Add(eItemType.Boost);
            }
            if (_allowGrapple)
            {
                _choices.Add(eItemType.Grapple);
            }
            if (_allowBomb)
            {
                _choices.Add(eItemType.Bomb);
            }

            return _choices.Count > 0 ? _choices[Random.Range(0, _choices.Count)] : eItemType.Boost;
        }

        private void SetupVisual()
        {
            if (_visual == null)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "Pickup Visual";
                cube.transform.SetParent(transform, false);
                cube.transform.localPosition = Vector3.up * _visualHeight;
                cube.transform.localScale = Vector3.one * _visualSize;
                _visual = cube.transform;
            }

            foreach (Collider visualCollider in _visual.GetComponentsInChildren<Collider>())
            {
                Destroy(visualCollider);
            }

            _visualBasePosition = _visual.localPosition;
            foreach (Renderer visualRenderer in _visual.GetComponentsInChildren<Renderer>())
            {
                Material material = visualRenderer.material;
                if (material.HasProperty(_emissionColorId))
                {
                    material.EnableKeyword("_EMISSION");
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    material.SetColor(_emissionColorId, _glowColor * _emissionIntensity);
                }
            }

            GameObject lightObject = new GameObject("Pickup Glow");
            lightObject.transform.SetParent(_visual, false);
            Light glowLight = lightObject.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.color = _glowColor;
            glowLight.intensity = _lightIntensity;
            glowLight.range = _lightRange;
            glowLight.shadows = LightShadows.None;
        }
    }
}
