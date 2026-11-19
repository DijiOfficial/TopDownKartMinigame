using UnityEngine;

namespace Battlekart
{
    public class BarrierWall : MonoBehaviour
    {
        [Header("Shape")]
        [SerializeField] private Vector3 _size = new Vector3(6.0f, 1.5f, 0.6f);
        [SerializeField] private float _clearMargin = 1.2f;
        [SerializeField] private float _colliderHeightMultiplier = 2.0f;

        [Header("Timing")]
        [SerializeField] private float _warningTime = 0.6f;
        [SerializeField] private float _moveSpeed = 8.0f;

        [Header("Visuals")]
        [SerializeField] private Color _color = new Color(1.0f, 0.8f, 0.2f);
        [SerializeField] private float _outlineWidth = 0.15f;
        [SerializeField] private float _floorHeight = 0.1f;
        [SerializeField, Range(0.0f, 1.0f)] private float _idleOutlineAlpha = 0.3f;
        [SerializeField] private float _warningBlinkRate = 8.0f;

        private Transform _block;
        private Rigidbody _body;
        private Renderer _blockRenderer;
        private LineRenderer _outline;
        private float _warningTimer;
        private float _holdTimer;
        private float _height;

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                if (_blockRenderer is not null)
                {
                    RendererTint.Apply(_blockRenderer, _color);
                }
            }
        }

        private void Awake()
        {
            GameObject blockObject = RuntimePrimitive.Create(PrimitiveType.Cube);
            blockObject.name = "Wall Block";
            blockObject.transform.SetParent(transform, false);
            blockObject.transform.localScale = _size;
            RuntimePrimitive.ExtendColliderUp(blockObject, _colliderHeightMultiplier);
            _block = blockObject.transform;
            _blockRenderer = blockObject.GetComponent<Renderer>();
            RendererTint.Apply(_blockRenderer, _color);

            _body = blockObject.AddComponent<Rigidbody>();
            _body.isKinematic = true;
            _body.interpolation = RigidbodyInterpolation.Interpolate;

            _outline = FloorLine.Create("Wall Outline", _outlineWidth);
            _outline.loop = true;
            _outline.enabled = true;

            Lower();
        }

        private void OnDestroy()
        {
            FloorLine.Destroy(_outline);
        }

        public void Raise(float duration)
        {
            if (_height <= 0.0f && _warningTimer <= 0.0f && _holdTimer <= 0.0f)
            {
                _warningTimer = _warningTime;
            }

            _holdTimer = Mathf.Max(_holdTimer, duration);
        }

        public void Lower()
        {
            _warningTimer = 0.0f;
            _holdTimer = 0.0f;
            _height = 0.0f;
            ApplyHeight();
        }

        private void Update()
        {
            if (_warningTimer > 0.0f)
            {
                _warningTimer -= Time.deltaTime;
            }
            else if (_holdTimer > 0.0f)
            {
                _holdTimer -= Time.deltaTime;
            }

            UpdateOutline();
        }

        private void FixedUpdate()
        {
            bool wantsUp = _warningTimer <= 0.0f && _holdTimer > 0.0f;
            if (wantsUp && _height <= 0.0f && IsFootprintBlocked())
            {
                wantsUp = false;
            }

            float target = wantsUp ? _size.y : 0.0f;
            _height = Mathf.MoveTowards(_height, target, _moveSpeed * Time.fixedDeltaTime);
            ApplyHeight();
        }

        private void ApplyHeight()
        {
            bool visible = _height > 0.0f;
            if (_block.gameObject.activeSelf != visible)
            {
                _block.gameObject.SetActive(visible);
            }

            Vector3 worldPosition = transform.position + transform.up * (_height - _size.y * 0.5f);
            if (visible)
            {
                _body.MovePosition(worldPosition);
            }
            else
            {
                _block.position = worldPosition;
            }
        }

        private bool IsFootprintBlocked()
        {
            float halfX = _size.x * 0.5f + _clearMargin;
            float halfZ = _size.z * 0.5f + _clearMargin;

            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                if (carrier.IsHidden)
                {
                    continue;
                }

                Vector3 local = Quaternion.Inverse(transform.rotation) * (carrier.transform.position - transform.position);
                if (Mathf.Abs(local.x) <= halfX && Mathf.Abs(local.z) <= halfZ)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateOutline()
        {
            Color color = _color;
            if (_warningTimer > 0.0f)
            {
                color.a = Mathf.Repeat(Time.time * _warningBlinkRate, 1.0f) < 0.5f ? 1.0f : 0.2f;
            }
            else
            {
                color.a = _height > 0.0f ? 1.0f : _idleOutlineAlpha;
            }
            FloorLine.SetColor(_outline, color);

            float halfX = _size.x * 0.5f;
            float halfZ = _size.z * 0.5f;
            Vector3 lift = Vector3.up * _floorHeight;
            _outline.positionCount = 4;
            _outline.SetPosition(0, transform.position + transform.rotation * new Vector3(-halfX, 0.0f, -halfZ) + lift);
            _outline.SetPosition(1, transform.position + transform.rotation * new Vector3(-halfX, 0.0f, halfZ) + lift);
            _outline.SetPosition(2, transform.position + transform.rotation * new Vector3(halfX, 0.0f, halfZ) + lift);
            _outline.SetPosition(3, transform.position + transform.rotation * new Vector3(halfX, 0.0f, -halfZ) + lift);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _color;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.up * (_size.y * 0.5f), _size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
