using UnityEngine;

namespace Battlekart
{
    public class BoostPad : MonoBehaviour
    {
        private const int ChevronCount = 3;

        [Header("Pad")]
        [SerializeField] private Vector2 _size = new Vector2(3.0f, 5.0f);
        [SerializeField] private float _boostDuration = 1.0f;
        [SerializeField] private float _boostImpulse = 5.0f;
        [SerializeField] private float _cooldown = 4.0f;

        [Header("Visuals")]
        [SerializeField] private Color _color = new Color(0.2f, 0.85f, 1.0f);
        [SerializeField] private Color _cooldownColor = new Color(0.25f, 0.25f, 0.25f);
        [SerializeField] private float _padThickness = 0.04f;
        [SerializeField] private float _chevronWidth = 0.2f;
        [SerializeField] private float _floorHeight = 0.1f;
        [SerializeField] private float _scrollSpeed = 1.0f;

        private readonly LineRenderer[] _chevrons = new LineRenderer[ChevronCount];
        private Renderer _padRenderer;
        private float _cooldownTimer;

        private bool IsReady => _cooldownTimer <= 0.0f;

        private void Awake()
        {
            GameObject padObject = RuntimePrimitive.Create(PrimitiveType.Cube);
            padObject.name = "Boost Pad Visual";
            padObject.transform.SetParent(transform, false);
            padObject.transform.localPosition = Vector3.up * (_padThickness * 0.5f);
            padObject.transform.localScale = new Vector3(_size.x, _padThickness, _size.y);
            Destroy(padObject.GetComponent<Collider>());
            _padRenderer = padObject.GetComponent<Renderer>();

            for (int i = 0; i < ChevronCount; i++)
            {
                _chevrons[i] = FloorLine.Create($"Boost Chevron {i + 1}", _chevronWidth);
                _chevrons[i].positionCount = 3;
            }
        }

        private void OnEnable()
        {
            MapElements.OnReset += ResetPad;
        }

        private void OnDisable()
        {
            MapElements.OnReset -= ResetPad;
        }

        private void OnDestroy()
        {
            foreach (LineRenderer chevron in _chevrons)
            {
                FloorLine.Destroy(chevron);
            }
        }

        private void ResetPad()
        {
            _cooldownTimer = 0.0f;
        }

        private void Update()
        {
            bool active = MapElements.Active;
            _cooldownTimer = Mathf.Max(0.0f, _cooldownTimer - Time.deltaTime);
            _padRenderer.enabled = active;
            RendererTint.Apply(_padRenderer, IsReady ? _color * 0.5f : _cooldownColor);
            UpdateChevrons(active);
            if (active == false || IsReady == false)
            {
                return;
            }

            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                if (carrier.IsHidden || IsInside(carrier.transform.position) == false)
                {
                    continue;
                }

                if (carrier.TryGetComponent(out KartItems items))
                {
                    items.ApplyBoost(_boostDuration, _boostImpulse);
                    AudioManager.Play(eSfx.BoostPad);
                    _cooldownTimer = _cooldown;
                    return;
                }
            }
        }

        private bool IsInside(Vector3 position)
        {
            Vector3 local = Quaternion.Inverse(transform.rotation) * (position - transform.position);
            return Mathf.Abs(local.x) <= _size.x * 0.5f && Mathf.Abs(local.z) <= _size.y * 0.5f;
        }

        private void UpdateChevrons(bool active)
        {
            float halfWidth = _size.x * 0.35f;
            float depth = _size.x * 0.3f;
            float spacing = _size.y / ChevronCount;
            float scroll = IsReady ? Mathf.Repeat(Time.time * _scrollSpeed * spacing, spacing) : 0.0f;
            float recharge = _cooldown > 0.0f ? 1.0f - _cooldownTimer / _cooldown : 1.0f;
            Vector3 lift = Vector3.up * _floorHeight;

            for (int i = 0; i < ChevronCount; i++)
            {
                LineRenderer chevron = _chevrons[i];
                bool charged = IsReady || recharge >= (i + 1) / (float)ChevronCount;
                chevron.enabled = active && charged;
                if (chevron.enabled == false)
                {
                    continue;
                }

                float z = -_size.y * 0.5f + depth + spacing * i + scroll;
                if (z > _size.y * 0.5f)
                {
                    z -= _size.y;
                }

                Color color = IsReady ? _color : _cooldownColor * 2.0f;
                float fade = Mathf.Clamp01(Mathf.Min(z + _size.y * 0.5f - depth, _size.y * 0.5f - z) / (spacing * 0.5f));
                color.a = IsReady ? fade : 1.0f;
                FloorLine.SetColor(chevron, color);

                chevron.SetPosition(0, transform.position + transform.rotation * new Vector3(-halfWidth, 0.0f, z - depth) + lift);
                chevron.SetPosition(1, transform.position + transform.rotation * new Vector3(0.0f, 0.0f, z) + lift);
                chevron.SetPosition(2, transform.position + transform.rotation * new Vector3(halfWidth, 0.0f, z - depth) + lift);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _color;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(_size.x, 0.1f, _size.y));
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * (_size.y * 0.5f));
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
