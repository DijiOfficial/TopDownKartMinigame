using UnityEngine;

namespace Battlekart
{
    public class CircuitDriver : ICarInputSource
    {
        private readonly Transform _kart;
        private readonly Rigidbody _body;
        private readonly Transform[] _waypoints;
        private readonly float _waypointRadius;
        private readonly float _cruiseSpeed;
        private readonly float _steerFullAngle;
        private int _current;

        public float Steer { get; private set; }
        public bool Accelerate { get; private set; }
        public bool Brake => false;
        public bool Handbrake => false;
        public bool UseItem => false;

        public CircuitDriver(Transform kart, Transform[] waypoints, float waypointRadius, float cruiseSpeed, float steerFullAngle)
        {
            _kart = kart;
            _kart.TryGetComponent(out _body);
            _waypoints = waypoints;
            _waypointRadius = waypointRadius;
            _cruiseSpeed = cruiseSpeed;
            _steerFullAngle = steerFullAngle;
            _current = FindNearestWaypoint();
        }

        public void Tick()
        {
            if (_waypoints.Length == 0)
            {
                Steer = 0.0f;
                Accelerate = false;
                return;
            }

            Vector3 toWaypoint = _waypoints[_current].position - _kart.position;
            toWaypoint.y = 0.0f;
            if (toWaypoint.magnitude <= _waypointRadius)
            {
                _current = (_current + 1) % _waypoints.Length;
                toWaypoint = _waypoints[_current].position - _kart.position;
                toWaypoint.y = 0.0f;
            }

            float angle = Vector3.SignedAngle(_kart.forward, toWaypoint, Vector3.up);
            Steer = Mathf.Clamp(angle / _steerFullAngle, -1.0f, 1.0f);

            float speed = _body is not null ? _body.linearVelocity.magnitude : 0.0f;
            Accelerate = speed < _cruiseSpeed;
        }

        private int FindNearestWaypoint()
        {
            int nearest = 0;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < _waypoints.Length; i++)
            {
                float distance = Vector3.Distance(_kart.position, _waypoints[i].position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = i;
                }
            }

            return nearest;
        }
    }
}
