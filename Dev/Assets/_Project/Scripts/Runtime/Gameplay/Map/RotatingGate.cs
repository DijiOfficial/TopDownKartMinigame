using System.Collections.Generic;
using UnityEngine;

namespace Battlekart
{
    public class RotatingGate : MonoBehaviour
    {
        private const int RingSegments = 64;

        public static readonly List<RotatingGate> All = new();

        [Header("Shape")]
        [SerializeField] private float _length = 12.0f;
        [SerializeField] private float _height = 1.5f;
        [SerializeField] private float _colliderHeightMultiplier = 2.0f;
        [SerializeField] private float _thickness = 0.5f;
        [SerializeField, Range(1, 4)] private int _bars = 1;
        [SerializeField] private float _hubRadius = 0.8f;

        [Header("Motion")]
        [SerializeField] private float _degreesPerSecond = 30.0f;
        [SerializeField] private float _startAngle = 0.0f;
        [SerializeField] private float _kartRadius = 2.2f;

        [Header("Visuals")]
        [SerializeField] private Color _color = new Color(0.9f, 0.3f, 0.9f);
        [SerializeField] private float _ringWidth = 0.12f;
        [SerializeField] private float _floorHeight = 0.1f;
        [SerializeField, Range(0.0f, 1.0f)] private float _ringAlpha = 0.25f;

        private Transform _rotor;
        private Rigidbody _body;
        private LineRenderer _ring;
        private float _angle;

        private void Awake()
        {
            GameObject rotorObject = new GameObject("Gate Rotor");
            rotorObject.transform.SetParent(transform, false);
            _rotor = rotorObject.transform;

            _body = rotorObject.AddComponent<Rigidbody>();
            _body.isKinematic = true;
            _body.interpolation = RigidbodyInterpolation.Interpolate;

            for (int i = 0; i < _bars; i++)
            {
                GameObject bar = RuntimePrimitive.Create(PrimitiveType.Cube);
                bar.name = $"Gate Bar {i + 1}";
                bar.transform.SetParent(_rotor, false);
                bar.transform.localPosition = Vector3.up * (_height * 0.5f);
                bar.transform.localRotation = Quaternion.Euler(0.0f, 180.0f / _bars * i, 0.0f);
                bar.transform.localScale = new Vector3(_thickness, _height, _length);
                RuntimePrimitive.ExtendColliderUp(bar, _colliderHeightMultiplier);
                RendererTint.Apply(bar.GetComponent<Renderer>(), _color);
            }

            GameObject hub = RuntimePrimitive.Create(PrimitiveType.Cylinder);
            hub.name = "Gate Hub";
            hub.transform.SetParent(_rotor, false);
            hub.transform.localPosition = Vector3.up * (_height * 0.5f + 0.05f);
            hub.transform.localScale = new Vector3(_hubRadius * 2.0f, _height * 0.5f + 0.05f, _hubRadius * 2.0f);
            RendererTint.Apply(hub.GetComponent<Renderer>(), _color * 0.7f);

            _ring = FloorLine.Create("Gate Sweep Ring", _ringWidth);
            ResetGate();
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

        public void ResetGate()
        {
            _angle = _startAngle;
            _rotor.rotation = transform.rotation * Quaternion.Euler(0.0f, _angle, 0.0f);
        }

        private void Update()
        {
            bool active = MapElements.Active;
            if (_rotor.gameObject.activeSelf != active)
            {
                _rotor.gameObject.SetActive(active);
            }

            _ring.enabled = active;
            if (active)
            {
                Color color = _color;
                color.a = _ringAlpha;
                FloorLine.SetColor(_ring, color);
                Vector3 center = new Vector3(transform.position.x, transform.position.y + _floorHeight, transform.position.z);
                FloorLine.SetArc(_ring, center, _length * 0.5f, 0.0f, 360.0f, RingSegments);
            }
        }

        private void FixedUpdate()
        {
            if (MapElements.Active == false)
            {
                return;
            }

            if (IsKartBlockingSweep())
            {
                return;
            }

            _angle = Mathf.Repeat(_angle + _degreesPerSecond * Time.fixedDeltaTime, 360.0f);
            _body.MoveRotation(transform.rotation * Quaternion.Euler(0.0f, _angle, 0.0f));
        }

        private bool IsKartBlockingSweep()
        {
            float contactDistance = _thickness * 0.5f + _kartRadius;
            float halfLength = _length * 0.5f;
            float direction = Mathf.Sign(_degreesPerSecond);

            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                if (carrier.IsHidden)
                {
                    continue;
                }

                Vector3 offset = carrier.transform.position - transform.position;
                offset.y = 0.0f;
                if (offset.magnitude > halfLength + _kartRadius)
                {
                    continue;
                }

                for (int i = 0; i < _bars; i++)
                {
                    Vector3 barDirection = transform.rotation * Quaternion.Euler(0.0f, _angle + 180.0f / _bars * i, 0.0f) * Vector3.forward;
                    float along = Mathf.Clamp(Vector3.Dot(offset, barDirection), -halfLength, halfLength);
                    Vector3 closest = barDirection * along;
                    Vector3 toKart = offset - closest;
                    if (toKart.magnitude > contactDistance)
                    {
                        continue;
                    }

                    Vector3 barVelocity = new Vector3(closest.z, 0.0f, -closest.x) * direction;
                    if (Vector3.Dot(toKart, barVelocity) > 0.0f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _color;
            for (int i = 0; i < _bars; i++)
            {
                Quaternion rotation = transform.rotation * Quaternion.Euler(0.0f, _startAngle + 180.0f / _bars * i, 0.0f);
                Gizmos.matrix = Matrix4x4.TRS(transform.position + Vector3.up * (_height * 0.5f), rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(_thickness, _height, _length));
            }
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
