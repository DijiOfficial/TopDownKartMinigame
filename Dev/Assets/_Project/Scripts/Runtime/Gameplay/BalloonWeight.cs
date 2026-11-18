using UnityEngine;

namespace Battlekart
{
    [RequireComponent(typeof(BalloonCarrier))]
    public class BalloonWeight : MonoBehaviour, IKartSpeedModifier
    {
        [SerializeField] private int _neutralCount = 3;
        [SerializeField, Range(0.0f, 0.2f)] private float _slowPerExtraBalloon = 0.03f;
        [SerializeField, Range(0.0f, 0.9f)] private float _maxSlow = 0.25f;
        [SerializeField, Range(0.0f, 0.2f)] private float _bonusPerMissingBalloon = 0.05f;

        private BalloonCarrier _carrier;

        public float SpeedMultiplier { get; private set; } = 1.0f;

        private void Awake()
        {
            _carrier = GetComponent<BalloonCarrier>();
        }

        private void OnEnable()
        {
            _carrier.OnCountChanged += HandleCountChanged;
            HandleCountChanged(_carrier);
        }

        private void OnDisable()
        {
            _carrier.OnCountChanged -= HandleCountChanged;
        }

        private void HandleCountChanged(BalloonCarrier carrier)
        {
            int difference = carrier.Count - _neutralCount;
            if (difference > 0)
            {
                SpeedMultiplier = 1.0f - Mathf.Min(difference * _slowPerExtraBalloon, _maxSlow);
            }
            else
            {
                SpeedMultiplier = 1.0f - difference * _bonusPerMissingBalloon;
            }
        }
    }
}
