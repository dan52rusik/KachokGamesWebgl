using UnityEngine;

namespace Tutorial
{
    public class CameraControl : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -8f);
        [SerializeField] private float lookHeight = 1.5f;

        private Transform _target;

        private void Start()
        {
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
                _target = player.transform;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                Player player = FindFirstObjectByType<Player>();
                if (player != null)
                    _target = player.transform;
            }

            if (_target == null)
                return;

            Quaternion targetRotation = Quaternion.Euler(0f, _target.eulerAngles.y, 0f);
            Vector3 rotatedOffset = targetRotation * offset;
            Vector3 newPos = _target.position + rotatedOffset;

            transform.position = Vector3.Lerp(transform.position, newPos, speed * Time.deltaTime);
            transform.LookAt(_target.position + Vector3.up * lookHeight);
        }
    }
}
