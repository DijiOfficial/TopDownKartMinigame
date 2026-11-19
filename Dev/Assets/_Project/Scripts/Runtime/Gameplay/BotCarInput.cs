using UnityEngine;

namespace Battlekart
{
    [RequireComponent(typeof(BalloonCarrier))]
    public class BotCarInput : MonoBehaviour, ICarInputSource
    {
        [Header("Decisions")]
        [SerializeField] private float _thinkInterval = 0.3f;
        [SerializeField, Range(0.0f, 1.0f)] private float _throttleChance = 0.85f;

        [Header("Chasing")]
        [SerializeField] private float _chaseDistance = 2.5f;
        [SerializeField] private float _steerFullAngle = 25.0f;
        [SerializeField] private float _handbrakeAngle = 70.0f;
        [SerializeField] private float _handbrakeMinSpeed = 6.0f;

        [Header("Items")]
        [SerializeField] private float _boostMinDistance = 8.0f;
        [SerializeField] private float _boostMaxAngle = 15.0f;

        [Header("Unstuck")]
        [SerializeField] private float _stuckSpeed = 1.0f;
        [SerializeField] private float _stuckTime = 1.5f;
        [SerializeField] private float _reverseTime = 1.0f;

        private BalloonCarrier _self;
        private Rigidbody _body;
        private KartItems _items;
        private BalloonCarrier _target;
        private float _thinkTimer;
        private float _stuckTimer;
        private float _reverseTimer;
        private bool _throttleThisTick = true;

        public float Steer { get; private set; }
        public bool Accelerate { get; private set; }
        public bool Brake { get; private set; }
        public bool Handbrake { get; private set; }
        public bool UseItem { get; private set; }

        private void Awake()
        {
            _self = GetComponent<BalloonCarrier>();
            TryGetComponent(out _body);
            TryGetComponent(out _items);
            _thinkTimer = Random.Range(0.0f, _thinkInterval);
        }

        private void Update()
        {
            UseItem = false;
            _thinkTimer -= Time.deltaTime;
            if (_thinkTimer <= 0.0f)
            {
                _thinkTimer = _thinkInterval;
                _target = PickTarget();
                _throttleThisTick = Random.value <= _throttleChance;
            }

            float speed = _body is not null ? _body.linearVelocity.magnitude : 0.0f;
            Vector3 destination = GetDestination();
            Vector3 toDestination = destination - transform.position;
            toDestination.y = 0.0f;
            float angle = Vector3.SignedAngle(transform.forward, toDestination, Vector3.up);

            if (_reverseTimer > 0.0f)
            {
                _reverseTimer -= Time.deltaTime;
                Steer = -Mathf.Sign(angle);
                Accelerate = false;
                Brake = true;
                Handbrake = false;
                return;
            }

            Steer = Mathf.Clamp(angle / _steerFullAngle, -1.0f, 1.0f);
            Accelerate = _throttleThisTick;
            Brake = false;
            Handbrake = Mathf.Abs(angle) > _handbrakeAngle && speed > _handbrakeMinSpeed;
            UseItem = ShouldUseItem(toDestination.magnitude, angle);

            UpdateStuck(speed);
        }

        private bool ShouldUseItem(float distanceToDestination, float angle)
        {
            if (_items is null)
            {
                return false;
            }

            switch (_items.HeldItem)
            {
                case eItemType.Grapple:
                    return _items.GrappleTarget is not null;
                case eItemType.Boost:
                    return distanceToDestination > _boostMinDistance && Mathf.Abs(angle) < _boostMaxAngle;
                default:
                    return false;
            }
        }

        private void UpdateStuck(float speed)
        {
            if (Accelerate && speed < _stuckSpeed)
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer >= _stuckTime)
                {
                    _stuckTimer = 0.0f;
                    _reverseTimer = _reverseTime;
                }
            }
            else
            {
                _stuckTimer = 0.0f;
            }
        }

        private Vector3 GetDestination()
        {
            if (_target == null || _target.isActiveAndEnabled == false)
            {
                return transform.position + transform.forward * 10.0f + transform.right * 5.0f;
            }

            return _target.transform.position - _target.transform.forward * _chaseDistance;
        }

        private BalloonCarrier PickTarget()
        {
            BalloonCarrier best = null;
            float bestScore = float.MaxValue;

            foreach (BalloonCarrier other in BalloonCarrier.All)
            {
                if (other == _self)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, other.transform.position);
                float score = other.CanBeStolenFrom ? distance : distance + 50.0f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = other;
                }
            }

            return best;
        }
    }
}
