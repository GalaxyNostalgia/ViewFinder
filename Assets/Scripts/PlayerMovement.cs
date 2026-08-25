using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody _rb;
    
    private Vector2 _movement;
    [SerializeField] private int speed = 10;
    
    public InputActionAsset asset;
    private InputActionMap _playerMap;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _lookAction;

    private void OnEnable()
    {
        _playerMap  = asset.FindActionMap("Player");
        _playerMap.Enable();
    }

    private void OnDisable()
    {
        _playerMap  = asset.FindActionMap("Player");
        _playerMap.Disable();
    }

    private void Awake()
    {
        _playerMap  = asset.FindActionMap("Player");
        _moveAction  = _playerMap.FindAction("Move");
        _jumpAction = _playerMap.FindAction("Jump");
        _lookAction  = _playerMap.FindAction("Look");
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _movement = new Vector2(_moveAction.ReadValue<Vector2>().x, _moveAction.ReadValue<Vector2>().y);
        
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(new Vector3(_movement.x, _rb.position.y, _movement.y) * speed * Time.fixedDeltaTime);
    }
}
