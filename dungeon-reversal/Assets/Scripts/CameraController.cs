using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2.5f, -5f);

    public float sensitivityX = 3f;
    public float sensitivityY = 2f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    public float collisionRadius = 0.3f;
    public LayerMask collisionMask;

    float yaw;
    float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        yaw += Input.GetAxis("Mouse X") * sensitivityX;
        pitch -= Input.GetAxis("Mouse Y") * sensitivityY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desired = target.position + rot * offset;

        Vector3 dir = desired - target.position;
        if (Physics.SphereCast(target.position, collisionRadius, dir.normalized, out RaycastHit hit, dir.magnitude, collisionMask))
            desired = hit.point + hit.normal * collisionRadius;

        transform.position = desired;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
