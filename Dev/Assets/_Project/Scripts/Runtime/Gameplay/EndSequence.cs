using UnityEngine;

namespace Battlekart
{
    public class EndSequence : MonoBehaviour
    {
        [SerializeField] private EndCameraDirector _cameraDirector;
        [SerializeField] private Transform _circuitRoot;
        [SerializeField] private float _waypointRadius = 3.0f;
        [SerializeField] private float _cruiseSpeed = 8.0f;
        [SerializeField] private float _steerFullAngle = 25.0f;

        private CircuitDriver _driver;

        public void Play(BalloonCarrier winner)
        {
            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                if (carrier.TryGetComponent(out ICarInputReceiver receiver))
                {
                    receiver.OverrideInputSource(null);
                }

                if (carrier != winner)
                {
                    carrier.SetHidden(true);
                }
            }

            if (winner == null)
            {
                return;
            }

            _driver = new CircuitDriver(winner.transform, GetWaypoints(), _waypointRadius, _cruiseSpeed, _steerFullAngle);
            if (winner.TryGetComponent(out ICarInputReceiver winnerReceiver))
            {
                winnerReceiver.OverrideInputSource(_driver);
            }

            _cameraDirector.FocusOn(winner.transform);
        }

        public void Stop()
        {
            _driver = null;

            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                carrier.SetHidden(false);
                if (carrier.TryGetComponent(out ICarInputReceiver receiver))
                {
                    receiver.ClearInputOverride();
                }
            }

            _cameraDirector.ReturnHome();
        }

        private void Update()
        {
            _driver?.Tick();
        }

        private Transform[] GetWaypoints()
        {
            if (_circuitRoot == null)
            {
                return System.Array.Empty<Transform>();
            }

            Transform[] waypoints = new Transform[_circuitRoot.childCount];
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypoints[i] = _circuitRoot.GetChild(i);
            }

            return waypoints;
        }
    }
}
