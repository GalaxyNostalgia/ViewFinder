using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6.0f;
    public float mouseSensitivity = 0.1f;
    private Vector3 playerVelocity;
    private readonly float gravity = -9.81f;
    private readonly float jumpHeight = 1.5f;
    private bool isGrounded;
    private float xRotation;
    
    public Transform cameraHolder;
    
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    
    [Header("Ending")]
    private UnityEvent onReachedEnding;
    
    private CharacterController characterController;
    

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        moveAction = inputActions.FindAction("Move");
        lookAction = inputActions.FindAction("Look");
        jumpAction = inputActions.FindAction("Jump");
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void Update()
    {
        Movement();
        MouseLook();
    }

    private void Movement()
    {
        isGrounded = characterController.isGrounded;
        if(isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        
        move = Vector3.ClampMagnitude(move, 1f);

        if (isGrounded && jumpAction.WasPressedThisFrame())
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        playerVelocity.y += gravity * Time.deltaTime;
        
        Vector3 finalMove = move * speed + Vector3.up * playerVelocity.y;
        characterController.Move(finalMove * Time.deltaTime);
    }
    
    private void MouseLook()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
