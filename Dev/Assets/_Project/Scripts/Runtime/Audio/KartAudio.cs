using UnityEngine;

namespace Battlekart
{
    [RequireComponent(typeof(Rigidbody))]
    public class KartAudio : MonoBehaviour
    {
        [Header("Engine")]
        [SerializeField] private AudioClip _engineClip;
        [SerializeField, Range(0.0f, 1.0f)] private float _engineVolume = 0.2f;
        [SerializeField] private float _enginePitchMin = 0.8f;
        [SerializeField] private float _enginePitchMax = 1.8f;
        [SerializeField] private float _speedForMaxPitch = 25.0f;

        [Header("Tire Screech")]
        [SerializeField] private AudioClip _screechClip;
        [SerializeField, Range(0.0f, 1.0f)] private float _screechVolume = 0.35f;
        [SerializeField] private float _screechSlipSpeed = 4.0f;
        [SerializeField] private float _screechMinSpeed = 5.0f;
        [SerializeField] private float _screechFadeSpeed = 4.0f;

        private Rigidbody _body;
        private BalloonCarrier _carrier;
        private AudioSource _engine;
        private AudioSource _screech;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            TryGetComponent(out _carrier);
            _engine = CreateLoop("Engine Audio", _engineClip, _engineVolume);
            _screech = CreateLoop("Screech Audio", _screechClip, 0.0f);
        }

        private void Update()
        {
            bool hidden = _carrier is not null && _carrier.IsHidden;
            Vector3 localVelocity = transform.InverseTransformDirection(_body.linearVelocity);
            float speed = _body.linearVelocity.magnitude;

            _engine.volume = hidden ? 0.0f : _engineVolume;
            _engine.pitch = Mathf.Lerp(_enginePitchMin, _enginePitchMax, Mathf.Clamp01(speed / _speedForMaxPitch));

            bool slipping = hidden == false && speed > _screechMinSpeed && Mathf.Abs(localVelocity.x) > _screechSlipSpeed;
            float targetScreech = slipping ? _screechVolume : 0.0f;
            _screech.volume = Mathf.MoveTowards(_screech.volume, targetScreech, _screechFadeSpeed * Time.deltaTime);
        }

        private AudioSource CreateLoop(string sourceName, AudioClip clip, float volume)
        {
            GameObject sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0.0f;
            source.volume = volume;
            if (clip != null)
            {
                source.time = Random.Range(0.0f, clip.length);
                source.Play();
            }
            return source;
        }
    }
}
