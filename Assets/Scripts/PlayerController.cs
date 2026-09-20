using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float speed = 6.0f;
    public float mouseSensitivity = 0.1f;
    public Transform cameraHolder;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;

    const float Gravity = -9.81f;
    const float JumpHeight = 1.5f;

    CharacterController characterController;
    float verticalVelocity;
    float xRotation;
    bool isPlayerActive;

    void OnEnable()
    {
        moveAction?.action.Enable();
        lookAction?.action.Enable();
        jumpAction?.action.Enable();
    }

    void OnDisable()
    {
        moveAction?.action.Disable();
        lookAction?.action.Disable();
        jumpAction?.action.Disable();
    }

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        isPlayerActive = true;
    }

    void Update()
    {
        if (!isPlayerActive)
            return;

        LookAround();
        Move();
    }

    public void ChangePlayerState(bool isActive) => isPlayerActive = isActive;

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        characterController.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        characterController.enabled = true;

        verticalVelocity = 0f;
        xRotation = 0f;
        cameraHolder.localRotation = Quaternion.identity;
    }

    void LookAround()
    {
        Vector2 look = lookAction != null ? lookAction.action.ReadValue<Vector2>() : Vector2.zero;

        xRotation = Mathf.Clamp(xRotation - look.y * mouseSensitivity, -90f, 90f);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * (look.x * mouseSensitivity));
    }

    void Move()
    {
        Vector2 input = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        if (characterController.isGrounded)
        {
            verticalVelocity = -2f;

            if (jumpAction != null && jumpAction.action.WasPressedThisFrame())
                verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }
        else
        {
            verticalVelocity += Gravity * Time.deltaTime;
        }

        characterController.Move((move * speed + Vector3.up * verticalVelocity) * Time.deltaTime);
    }
}
