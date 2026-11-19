using UnityEngine;

namespace Battlekart
{
    [RequireComponent(typeof(Collider))]
    public class ReverseWall : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float _warningTime = 0.6f;
        [SerializeField] private float _moveSpeed = 8.0f;
        [SerializeField] private float _clearMargin = 1.2f;

        [Header("Visuals")]
        [SerializeField] private Color _color = new Color(0.7f, 0.35f, 1.0f);
        [SerializeField] private Color _warningColor = Color.white;
        [SerializeField] private float _warningBlinkRate = 8.0f;
        [SerializeField] private float _sinkExtra = 0.05f;

        private Collider _collider;
        private Renderer _renderer;
        private Vector3 _raisedPosition;
        private float _sinkDepth;
        private float _drop;
        private float _openTimer;

        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _collider = GetComponent<Collider>();

            _raisedPosition = transform.position;
            _sinkDepth = transform.lossyScale.y + _sinkExtra;
        }

        public void Open(float duration)
        {
            _openTimer = Mathf.Max(_openTimer, duration);
        }

        public void ResetWall()
        {
            _openTimer = 0.0f;
        }

        private void Update()
        {
            _openTimer = Mathf.Max(0.0f, _openTimer - Time.deltaTime);

            bool wantsDown = _openTimer > 0.0f || MapElements.Active == false;
            if (wantsDown == false && _drop >= _sinkDepth && IsFootprintBlocked())
            {
                wantsDown = true;
            }

            float target = wantsDown ? _sinkDepth : 0.0f;
            _drop = Mathf.MoveTowards(_drop, target, _moveSpeed * Time.deltaTime);
            transform.position = _raisedPosition + Vector3.down * _drop;
            _collider.enabled = wantsDown == false || _drop < _sinkDepth * 0.5f;

            if (_renderer == null)
            {
                return;
            }

            _renderer.enabled = _drop < _sinkDepth;
            bool warning = _openTimer > 0.0f && _openTimer <= _warningTime;
            bool flash = warning && Mathf.Repeat(Time.time * _warningBlinkRate, 1.0f) < 0.5f;
            RendererTint.Apply(_renderer, flash ? _warningColor : _color);
        }

        private bool IsFootprintBlocked()
        {
            Vector3 scale = transform.lossyScale;
            float halfX = scale.x * 0.5f + _clearMargin;
            float halfZ = scale.z * 0.5f + _clearMargin;

            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                if (carrier.IsHidden)
                {
                    continue;
                }

                Vector3 local = Quaternion.Inverse(transform.rotation) * (carrier.transform.position - _raisedPosition);
                if (Mathf.Abs(local.x) <= halfX && Mathf.Abs(local.z) <= halfZ)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
