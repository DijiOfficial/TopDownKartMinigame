using UnityEngine;

namespace Battlekart
{
    public class LeaderCrown : MonoBehaviour
    {
        [SerializeField] private MatchManager _matchManager;
        [SerializeField] private GameObject _crownPrefab;
        [SerializeField] private Color _fallbackColor = new Color(1.0f, 0.82f, 0.1f);
        [SerializeField] private float _height = 2.5f;
        [SerializeField] private float _followSharpness = 10.0f;
        [SerializeField] private float _spinSpeed = 90.0f;
        [SerializeField] private float _bobAmplitude = 0.15f;
        [SerializeField] private float _bobFrequency = 2.0f;

        private Transform _crown;

        private void Awake()
        {
            _crown = CreateCrown();
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

            Vector3 target = leader.transform.position + Vector3.up * (_height + Mathf.Sin(Time.time * _bobFrequency * Mathf.PI * 2.0f) * _bobAmplitude);

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
    }
}
