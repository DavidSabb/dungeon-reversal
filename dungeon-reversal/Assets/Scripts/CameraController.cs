using UnityEngine;
 
/// <summary>
/// CameraController.cs
/// Dungeon Reversal - Third-person follow camera controlled by mouse.
/// Place this script on the Main Camera.
/// Set 'target' to the Cave Troll root transform in the Inspector.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;           // Cave Troll root
    public Vector3 offset = new Vector3(0f, 2.5f, -5f);
 
    [Header("Mouse Sensitivity")]
    public float sensitivityX = 3f;
    public float sensitivityY = 2f;
 
    [Header("Pitch Clamp")]
    public float minPitch = -20f;
    public float maxPitch = 60f;
 
    [Header("Collision")]
    public float collisionRadius = 0.3f;
    public LayerMask collisionMask;
 
    private float _yaw;
    private float _pitch;
    private LockOnSystem _lockOn;
 
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
 
        if (target != null)
            _lockOn = target.GetComponent<LockOnSystem>();
 
        _yaw   = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;
    }
 
    private void LateUpdate()
    {
        if (target == null) return;
 
        // If locked on, look at target from over-the-shoulder
        if (_lockOn != null && _lockOn.HasTarget)
        {
            Vector3 midPoint = (target.position + _lockOn.CurrentTarget.position) * 0.5f;
            Vector3 dir = (midPoint - target.position).normalized;
            _yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        }
        else
        {
            _yaw   += Input.GetAxis("Mouse X") * sensitivityX;
            _pitch -= Input.GetAxis("Mouse Y") * sensitivityY;
            _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }
 
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desiredPos  = target.position + rotation * offset;
 
        // Simple camera collision
        Vector3 direction = desiredPos - target.position;
        if (Physics.SphereCast(target.position, collisionRadius, direction.normalized,
            out RaycastHit hit, direction.magnitude, collisionMask))
        {
            desiredPos = hit.point + hit.normal * collisionRadius;
        }
 
        transform.position = desiredPos;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}