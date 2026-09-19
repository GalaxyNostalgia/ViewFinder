using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    public float speed = 6.0f;
    
    public float mouseSensitivity = 0.1f;
    public Transform cameraHolder;
    public bool isMoving;
    public Volume ppProfile;
 
    [Header("Input Actions")]
    [Tooltip("Vector2 action, e.g. WASD / Left Stick composite")]
    [SerializeField] private InputActionReference moveAction;
    [Tooltip("Vector2 action bound to Mouse Delta / Right Stick")]
    [SerializeField] private InputActionReference lookAction;
    [Tooltip("Button action, e.g. Space / South Button")]
    [SerializeField] private InputActionReference jumpAction;
 
    private CharacterController characterController;
    private float verticalVelocity = 0.0f;
    private float gravity = -9.81f;
    private float jumpHeight = 1.5f;
    private float xRotation = 0f;
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
 
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        isPlayerActive = true;
    }
 
    void Update()
    {
        if (isPlayerActive)
        {
            LookAround();
            Move();
        }
    }
 
    public void ChangePlayerState(bool isActive) {
        isPlayerActive = isActive;
    }
 
    void LookAround()
    {
        Vector2 look = lookAction != null ? lookAction.action.ReadValue<Vector2>() : Vector2.zero;
        
        float mouseX = look.x * mouseSensitivity;
        float mouseY = look.y * mouseSensitivity;
 
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
 
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
 
    void Move()
    {
        Vector2 moveInput = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        float moveX = moveInput.x;
        float moveZ = moveInput.y;
 
        isMoving = moveX != 0 || moveZ != 0f;
 
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
 
        if (characterController.isGrounded)
        {
            verticalVelocity = -2f;
            if (jumpAction != null && jumpAction.action.WasPressedThisFrame())
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
 
        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }
    
 
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.name.Contains("Ending"))
            EndGame();
    }
 
    void EndGame() {
        StartCoroutine(Ending());
    }
 
    IEnumerator Ending() {
        float x = 0;
        while (x < 0.5f)
        {
            x+=Time.deltaTime / 6;
            yield return null;
        }
    }
}
