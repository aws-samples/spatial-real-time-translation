using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Vector3 _rotationAxis = Vector3.zero;
    [SerializeField] private bool _flipDirection;

    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        _rotationAxis = _rotationAxis.normalized;
    }

    private void LateUpdate()
    {
        this.transform.LookAt(_camera.transform.position);
        this.transform.rotation = Quaternion.Euler(Vector3.Scale(this.transform.rotation.eulerAngles, _rotationAxis));
        if (_flipDirection)
        {
            this.transform.Rotate(new Vector3(0, 180, 0));
        }
    }

}
