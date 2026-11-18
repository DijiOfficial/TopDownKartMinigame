using UnityEngine;

namespace Battlekart
{
    public class LeaderCrown : MonoBehaviour
    {
        [SerializeField] private MatchManager _matchManager;
        [SerializeField] private GameObject _crownPrefab;
        [SerializeField] private Color _fallbackColor = new Color(1.0f, 0.82f, 0.1f);
        [SerializeField] private float _height = 1.6f;
        [SerializeField] private float _forwardOffset = 0.8f;
        [SerializeField] private float _followSharpness = 10.0f;
        [SerializeField] private float _spinSpeed = 90.0f;
        [SerializeField] private float _bobAmplitude = 0.15f;
        [SerializeField] private float _bobFrequency = 2.0f;

        [Header("Glow")]
        [SerializeField] private Color _glowColor = new Color(1.0f, 0.8f, 0.3f);
        [SerializeField] private float _emissionIntensity = 1.5f;
        [SerializeField] private float _lightIntensity = 2.0f;
        [SerializeField] private float _lightRange = 4.0f;

        private static readonly int _emissionColorId = Shader.PropertyToID("_EmissionColor");

        private Transform _crown;

        private void Awake()
        {
            _crown = CreateCrown();
            AddGlow(_crown);
            _crown.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            bool matchActive = _matchManager.State == MatchManager.eMatchState.Playing
                || _matchManager.State == MatchManager.eMatchState.SuddenDeath
                || _matchManager.State == MatchManager.eMatchState.Finished;

            if (matchActive == false || MatchManager.TryGetSingleLeader(out BalloonCarrier leader) == false)
            {
                _crown.gameObject.SetActive(false);
                return;
            }

            Vector3 forward = leader.transform.forward;
            forward.y = 0.0f;
            Vector3 target = leader.transform.position + forward.normalized * _forwardOffset + Vector3.up * (_height + Mathf.Sin(Time.time * _bobFrequency * Mathf.PI * 2.0f) * _bobAmplitude);

            if (_crown.gameObject.activeSelf == false)
            {
                _crown.gameObject.SetActive(true);
                _crown.position = target;
            }

            float blend = 1.0f - Mathf.Exp(-_followSharpness * Time.deltaTime);
            _crown.position = Vector3.Lerp(_crown.position, target, blend);
            _crown.Rotate(Vector3.up, _spinSpeed * Time.deltaTime, Space.World);
        }

        private Transform CreateCrown()
        {
            if (_crownPrefab != null)
            {
                return Instantiate(_crownPrefab).transform;
            }

            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crown.name = "Leader Crown";
            crown.transform.localScale = new Vector3(1.0f, 0.15f, 1.0f);
            Destroy(crown.GetComponent<Collider>());
            RendererTint.Apply(crown.GetComponent<Renderer>(), _fallbackColor);
            return crown.transform;
        }

        private void AddGlow(Transform crown)
        {
            foreach (Renderer crownRenderer in crown.GetComponentsInChildren<Renderer>())
            {
                Material material = crownRenderer.material;
                if (material.HasProperty(_emissionColorId))
                {
                    material.EnableKeyword("_EMISSION");
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    material.SetColor(_emissionColorId, _glowColor * _emissionIntensity);
                }
            }

            GameObject lightObject = new GameObject("Crown Glow");
            lightObject.transform.SetParent(crown, false);
            Light glow = lightObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = _glowColor;
            glow.intensity = _lightIntensity;
            glow.range = _lightRange;
            glow.shadows = LightShadows.None;
        }
    }
}
