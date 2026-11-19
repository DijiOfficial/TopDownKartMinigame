using UnityEngine;

namespace Battlekart
{
    [RequireComponent(typeof(BalloonCarrier))]
    public class KartItems : MonoBehaviour, IKartSpeedModifier
    {
        private const int CircleSegments = 48;

        public static bool ItemsEnabled { get; set; }

        [Header("Boost")]
        [SerializeField] private float _boostDuration = 1.5f;
        [SerializeField] private float _boostSpeedMultiplier = 1.4f;
        [SerializeField] private float _boostImpulse = 6.0f;

        [Header("Grapple")]
        [SerializeField] private float _grappleRange = 12.0f;
        [SerializeField, Range(5.0f, 90.0f)] private float _grappleAngle = 25.0f;
        [SerializeField] private float _grappleLineDuration = 0.3f;
        [SerializeField] private float _grappleLineWidth = 0.12f;
        [SerializeField] private float _reticleRadius = 1.6f;

        [Header("Bomb")]
        [SerializeField] private float _bombFuse = 3.0f;
        [SerializeField] private float _bombRadius = 12.0f;
        [SerializeField] private float _stunDuration = 1.0f;
        [SerializeField] private float _explosionFlashDuration = 0.3f;
        [SerializeField] private float _bombRingWidth = 0.15f;

        [Header("Visuals")]
        [SerializeField] private float _floorHeight = 0.1f;
        [SerializeField] private float _labelHeight = 3.0f;
        [SerializeField] private Color _stunColor = new Color(0.35f, 0.35f, 0.35f);

        private BalloonCarrier _carrier;
        private ICarInputSource _input;
        private Rigidbody _body;

        private float _boostTimer;
        private float _stunTimer;
        private float _bombTimer;
        private float _bombBlinkPhase;
        private float _flashTimer;
        private float _grappleLineTimer;
        private BalloonCarrier _grappleLineTarget;

        private LineRenderer _reticle;
        private LineRenderer _grappleLine;
        private LineRenderer _bombRing;
        private GUIStyle _labelStyle;

        public eItemType HeldItem { get; private set; }
        public BalloonCarrier GrappleTarget { get; private set; }
        public bool HasBomb => _bombTimer > 0.0f;
        public bool CanReceiveItem => HeldItem == eItemType.None && HasBomb == false;
        public bool IsStunned => _stunTimer > 0.0f;
        public float SpeedMultiplier => IsStunned ? 0.0f : _boostTimer > 0.0f ? _boostSpeedMultiplier : 1.0f;

        private void Awake()
        {
            _carrier = GetComponent<BalloonCarrier>();
            TryGetComponent(out _input);
            TryGetComponent(out _body);

            _reticle = FloorLine.Create("Grapple Reticle", 0.1f);
            _grappleLine = FloorLine.Create("Grapple Line", _grappleLineWidth);
            _bombRing = FloorLine.Create("Bomb Ring", _bombRingWidth);
        }

        private void OnDestroy()
        {
            FloorLine.Destroy(_reticle);
            FloorLine.Destroy(_grappleLine);
            FloorLine.Destroy(_bombRing);
        }

        public bool TryGiveItem(eItemType item)
        {
            if (item == eItemType.Bomb)
            {
                if (HasBomb)
                {
                    return false;
                }

                _bombTimer = _bombFuse;
                _bombBlinkPhase = 0.0f;
                return true;
            }

            if (HeldItem != eItemType.None)
            {
                return false;
            }

            HeldItem = item;
            return true;
        }

        public void ResetItems()
        {
            HeldItem = eItemType.None;
            GrappleTarget = null;
            _boostTimer = 0.0f;
            _bombTimer = 0.0f;
            _flashTimer = 0.0f;
            _grappleLineTimer = 0.0f;
            _grappleLineTarget = null;
            if (IsStunned)
            {
                _stunTimer = 0.0f;
                KartSpawner.Tint(gameObject, _carrier.PlayerColor);
            }
            HideVisuals();
        }

        public void ApplyBoost(float duration, float impulse)
        {
            _boostTimer = Mathf.Max(_boostTimer, duration);
            if (_body is not null)
            {
                _body.AddForce(FlatForward() * impulse, ForceMode.VelocityChange);
            }
        }

        public void Stun()
        {
            _stunTimer = _stunDuration;
            KartSpawner.Tint(gameObject, _stunColor);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            _boostTimer = Mathf.Max(0.0f, _boostTimer - deltaTime);

            if (ItemsEnabled == false || _carrier.IsHidden)
            {
                HideVisuals();
                return;
            }

            UpdateStun(deltaTime);

            GrappleTarget = HeldItem == eItemType.Grapple ? FindGrappleTarget() : null;

            if (_input is not null && _input.UseItem)
            {
                UseHeldItem();
            }

            UpdateBomb(deltaTime);
        }

        private void LateUpdate()
        {
            if (ItemsEnabled == false || _carrier.IsHidden)
            {
                return;
            }

            UpdateReticle();
            UpdateGrappleLine();
            UpdateBombRing();
        }

        private void UpdateStun(float deltaTime)
        {
            if (IsStunned == false)
            {
                return;
            }

            _stunTimer -= deltaTime;
            if (IsStunned == false)
            {
                KartSpawner.Tint(gameObject, _carrier.PlayerColor);
            }
        }

        private void UseHeldItem()
        {
            switch (HeldItem)
            {
                case eItemType.Boost:
                    ApplyBoost(_boostDuration, _boostImpulse);
                    AudioManager.Play(eSfx.Boost);
                    HeldItem = eItemType.None;
                    break;
                case eItemType.Grapple:
                    if (GrappleTarget is not null && GrappleTarget.TryTakeBalloon(out Vector3 balloonPosition))
                    {
                        _carrier.GiveBalloon(balloonPosition);
                        AudioManager.Play(eSfx.Grapple);
                        _grappleLineTarget = GrappleTarget;
                        _grappleLineTimer = _grappleLineDuration;
                        HeldItem = eItemType.None;
                        GrappleTarget = null;
                    }
                    break;
            }
        }

        private BalloonCarrier FindGrappleTarget()
        {
            BalloonCarrier best = null;
            float bestDistance = float.MaxValue;
            Vector3 forward = FlatForward();

            foreach (BalloonCarrier other in BalloonCarrier.All)
            {
                if (other == _carrier || other.IsHidden || other.CanBeStolenFrom == false)
                {
                    continue;
                }

                Vector3 toOther = other.transform.position - transform.position;
                toOther.y = 0.0f;
                float distance = toOther.magnitude;
                if (distance > _grappleRange || distance >= bestDistance)
                {
                    continue;
                }

                if (Vector3.Angle(forward, toOther) <= _grappleAngle)
                {
                    best = other;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private void UpdateBomb(float deltaTime)
        {
            _flashTimer = Mathf.Max(0.0f, _flashTimer - deltaTime);
            if (HasBomb == false)
            {
                return;
            }

            int previousSecond = Mathf.CeilToInt(_bombTimer);
            _bombTimer -= deltaTime;
            if (Mathf.CeilToInt(_bombTimer) < previousSecond && _bombTimer > 0.0f)
            {
                AudioManager.Play(eSfx.BombTick);
            }

            float blinkInterval = Mathf.Lerp(0.08f, 0.4f, _bombTimer / _bombFuse);
            _bombBlinkPhase += deltaTime / blinkInterval;

            if (HasBomb == false)
            {
                Explode();
            }
        }

        private void Explode()
        {
            _bombTimer = 0.0f;
            _flashTimer = _explosionFlashDuration;
            AudioManager.Play(eSfx.BombExplode);

            foreach (BalloonCarrier other in BalloonCarrier.All)
            {
                if (other == _carrier || other.IsHidden)
                {
                    continue;
                }

                Vector3 offset = other.transform.position - transform.position;
                offset.y = 0.0f;
                if (offset.magnitude <= _bombRadius && other.TryGetComponent(out KartItems otherItems))
                {
                    otherItems.Stun();
                }
            }
        }

        private void UpdateReticle()
        {
            bool visible = GrappleTarget is not null;
            _reticle.enabled = visible;
            if (visible == false)
            {
                return;
            }

            FloorLine.SetColor(_reticle, _carrier.PlayerColor);
            FloorLine.SetArc(_reticle, FloorPoint(GrappleTarget.transform.position), _reticleRadius, 0.0f, 360.0f, CircleSegments);
        }

        private void UpdateGrappleLine()
        {
            _grappleLineTimer = Mathf.Max(0.0f, _grappleLineTimer - Time.deltaTime);
            bool visible = _grappleLineTimer > 0.0f && _grappleLineTarget != null;
            _grappleLine.enabled = visible;
            if (visible == false)
            {
                return;
            }

            Color color = _carrier.PlayerColor;
            color.a = _grappleLineTimer / _grappleLineDuration;
            FloorLine.SetColor(_grappleLine, color);
            _grappleLine.positionCount = 2;
            _grappleLine.SetPosition(0, transform.position + Vector3.up);
            _grappleLine.SetPosition(1, _grappleLineTarget.transform.position + Vector3.up);
        }

        private void UpdateBombRing()
        {
            if (_flashTimer > 0.0f)
            {
                float progress = 1.0f - _flashTimer / _explosionFlashDuration;
                Color flash = Color.Lerp(Color.white, ItemInfo.GetColor(eItemType.Bomb), progress);
                flash.a = 1.0f - progress;
                _bombRing.enabled = true;
                _bombRing.widthMultiplier = _bombRingWidth * 4.0f;
                FloorLine.SetColor(_bombRing, flash);
                FloorLine.SetArc(_bombRing, FloorPoint(transform.position), _bombRadius * Mathf.Lerp(0.6f, 1.0f, progress), 0.0f, 360.0f, CircleSegments);
                return;
            }

            bool visible = HasBomb && Mathf.FloorToInt(_bombBlinkPhase) % 2 == 0;
            _bombRing.enabled = visible;
            if (visible == false)
            {
                return;
            }

            _bombRing.widthMultiplier = _bombRingWidth;
            FloorLine.SetColor(_bombRing, ItemInfo.GetColor(eItemType.Bomb));
            FloorLine.SetArc(_bombRing, FloorPoint(transform.position), _bombRadius, 0.0f, 360.0f, CircleSegments);
        }

        private void HideVisuals()
        {
            _reticle.enabled = false;
            _grappleLine.enabled = false;
            _bombRing.enabled = false;
        }

        private Vector3 FlatForward()
        {
            Vector3 forward = transform.forward;
            forward.y = 0.0f;
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private Vector3 FloorPoint(Vector3 position)
        {
            return new Vector3(position.x, transform.position.y + _floorHeight, position.z);
        }

        private void OnGUI()
        {
            if (ItemsEnabled == false || _carrier.IsHidden || (HeldItem == eItemType.None && HasBomb == false))
            {
                return;
            }

            if (ItemInfo.TryGetScreenPosition(transform.position + Vector3.up * _labelHeight, out Vector2 position) == false)
            {
                return;
            }

            if (_labelStyle is null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            }
            _labelStyle.fontSize = Mathf.Max(10, Screen.height / 45);

            if (HeldItem != eItemType.None)
            {
                ItemInfo.DrawLabel(position, ItemInfo.GetLabel(HeldItem), _labelStyle, ItemInfo.GetColor(HeldItem));
                position.y -= _labelStyle.fontSize * 1.3f;
            }

            if (HasBomb)
            {
                ItemInfo.DrawLabel(position, $"{ItemInfo.GetLabel(eItemType.Bomb)} {Mathf.CeilToInt(_bombTimer)}", _labelStyle, ItemInfo.GetColor(eItemType.Bomb));
            }
        }
    }
}
