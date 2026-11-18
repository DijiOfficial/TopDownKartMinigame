using UnityEngine;

namespace Battlekart
{
    [DefaultExecutionOrder(1000)]
    [RequireComponent(typeof(Camera))]
    public class EndCameraDirector : MonoBehaviour
    {
        [SerializeField] private float _transitionDuration = 2.0f;
        [SerializeField] private float _sideDistance = 7.0f;
        [SerializeField] private float _height = 3.0f;
        [SerializeField] private float _framingOffset = 0.5f;
        [SerializeField] private float _lookHeight = 0.6f;
        [SerializeField] private float _fieldOfView = 40.0f;
        [SerializeField] private float _headingSharpness = 2.0f;
        [SerializeField] private float _followSharpness = 6.0f;

        private Camera _camera;
        private Vector3 _homePosition;
        private Quaternion _homeRotation;
        private float _homeFieldOfView;

        private Transform _target;
        private Vector3 _smoothedForward;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private float _startFieldOfView;
        private float _transitionTime;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _homePosition = transform.position;
            _homeRotation = transform.rotation;
            _homeFieldOfView = _camera.fieldOfView;
        }

        public void FocusOn(Transform target)
        {
            _target = target;
            _smoothedForward = FlatForward(target);
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _startFieldOfView = _camera.fieldOfView;
            _transitionTime = 0.0f;
        }

        public void ReturnHome()
        {
            _target = null;
            transform.SetPositionAndRotation(_homePosition, _homeRotation);
            _camera.fieldOfView = _homeFieldOfView;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            float headingBlend = 1.0f - Mathf.Exp(-_headingSharpness * Time.deltaTime);
            _smoothedForward = Vector3.Slerp(_smoothedForward, FlatForward(_target), headingBlend).normalized;

            Vector3 right = Vector3.Cross(Vector3.up, _smoothedForward);
            Vector3 desiredPosition = _target.position - right * _sideDistance + Vector3.up * _height;
            Vector3 lookPoint = _target.position - _smoothedForward * _framingOffset + Vector3.up * _lookHeight;
            Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - desiredPosition, Vector3.up);

            if (_transitionTime < _transitionDuration)
            {
                _transitionTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0.0f, 1.0f, Mathf.Clamp01(_transitionTime / _transitionDuration));
                transform.SetPositionAndRotation(Vector3.Lerp(_startPosition, desiredPosition, t), Quaternion.Slerp(_startRotation, desiredRotation, t));
                _camera.fieldOfView = Mathf.Lerp(_startFieldOfView, _fieldOfView, t);
                return;
            }

            float followBlend = 1.0f - Mathf.Exp(-_followSharpness * Time.deltaTime);
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, desiredPosition, followBlend), Quaternion.Slerp(transform.rotation, desiredRotation, followBlend));
        }

        private static Vector3 FlatForward(Transform target)
        {
            Vector3 forward = target.forward;
            forward.y = 0.0f;
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }
    }
}
