using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed  = 10f;
    public float gravity   = -20f;

    [Header("Jump")]
    public float jumpHeight = 2f;

    [Header("Mouse Look")]
    public Transform cameraTransform;   // drag your Camera child here in Inspector
    public float mouseSensitivity = 2f;
    public float pitchMin = -40f;
    public float pitchMax = 60f;

    private CharacterController _cc;
    private Animator            _anim;
    private PlayerCombat        _combat;
    private Vector3             _velocity;
    private bool                _isGrounded;
    private float               _pitch = 0f;

    private static readonly int HashSpeed    = Animator.StringToHash("Speed");
    private static readonly int HashGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int HashJump     = Animator.StringToHash("Jump");

    private void Awake()
    {
        _cc     = GetComponent<CharacterController>();
        _anim   = GetComponent<Animator>();
        _combat = GetComponent<PlayerCombat>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void Update()
    {
        MouseLook();
        GroundCheck();
        Move();
        ApplyGravity();
    }

    private void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down only
        _pitch -= mouseY;
        _pitch  = Mathf.Clamp(_pitch, pitchMin, pitchMax);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void GroundCheck()
    {
        _isGrounded = _cc.isGrounded;
        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -4f;

        _anim.SetBool(HashGrounded, _isGrounded);
    }

    private void Move()
    {
        if (_combat != null && _combat.IsAttacking) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        if (move.magnitude > 1f) move.Normalize();

        bool running  = Input.GetKey(KeyCode.LeftShift);
        float speed   = running ? runSpeed : walkSpeed;

        _cc.Move(move * speed * Time.deltaTime);

        // Jump
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _anim.SetTrigger(HashJump);
        }

        // Animator
        _anim.SetFloat(HashSpeed, move.magnitude * (running ? 2f : 1f), 0.1f, Time.deltaTime);
    }

    private void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(Vector3.up * _velocity.y * Time.deltaTime);
    }
}