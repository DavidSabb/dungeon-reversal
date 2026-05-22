using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpHeight = 2f;
    public Transform cameraTransform;

    const float gravity = -20f;
    const float mouseSensitivity = 2f;
    const float pitchMin = -40f;
    const float pitchMax = 60f;

    CharacterController cc;
    Animator anim;
    PlayerCombat combat;
    Vector3 velocity;
    bool grounded;
    float pitch;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        combat = GetComponent<PlayerCombat>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MouseLook();
        GroundCheck();
        Move();
        ApplyGravity();
    }

    void MouseLook()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mx);
        pitch -= my;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void GroundCheck()
    {
        grounded = cc.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -4f;
        anim.SetBool("IsGrounded", grounded);
    }

    void Move()
    {
        if (combat != null && combat.IsAttacking) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        if (move.magnitude > 1f) move.Normalize();

        bool running = Input.GetKey(KeyCode.LeftShift);
        float speed = running ? runSpeed : walkSpeed;
        cc.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            anim.SetTrigger("Jump");
        }

        anim.SetFloat("Speed", move.magnitude * (running ? 2f : 1f), 0.1f, Time.deltaTime);
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        cc.Move(Vector3.up * velocity.y * Time.deltaTime);
    }
}
