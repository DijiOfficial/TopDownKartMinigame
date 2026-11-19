using UnityEngine;

namespace Battlekart
{
    [RequireComponent(typeof(Rigidbody))]
    public class KartFlipRecovery : MonoBehaviour
    {
        [SerializeField, Range(-1.0f, 1.0f)] private float _uprightThreshold = 0.3f;
        [SerializeField] private float _flippedTime = 1.5f;
        [SerializeField] private float _maxSpeed = 3.0f;
        [SerializeField] private float _liftHeight = 1.0f;

        private Rigidbody _body;
        private BalloonCarrier _carrier;
        private float _flippedTimer;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            TryGetComponent(out _carrier);
        }

        private void Update()
        {
            if (_carrier != null && _carrier.IsHidden)
            {
                _flippedTimer = 0.0f;
                return;
            }

            bool flipped = transform.up.y < _uprightThreshold;
            bool settled = _body.linearVelocity.magnitude < _maxSpeed;
            _flippedTimer = flipped && settled ? _flippedTimer + Time.deltaTime : 0.0f;

            if (_flippedTimer >= _flippedTime)
            {
                Recover();
            }
        }

        private void Recover()
        {
            _flippedTimer = 0.0f;

            Vector3 forward = transform.forward;
            forward.y = 0.0f;
            if (forward.sqrMagnitude < 0.01f)
            {
                forward = Vector3.ProjectOnPlane(transform.up, Vector3.up);
            }
            if (forward.sqrMagnitude < 0.01f)
            {
                forward = Vector3.forward;
            }

            Vector3 position = transform.position + Vector3.up * _liftHeight;
            Quaternion rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);

            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            _body.position = position;
            _body.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
