using UnityEngine;
using UnityEngine.InputSystem;

public class PointAndClickCharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public LayerMask groundLayer;

    private Camera _camera;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private PointAndClickInput inputActions; // Change this to match your generated class name

    private void Awake()
    {
        _camera = Camera.main;
        inputActions = new PointAndClickInput();
        targetPosition = transform.position;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Update()
    {
        // 1. Check for Input
        if (inputActions.Player.Click.triggered)
        {
            SetTargetPosition();
        }

        // 2. Move towards target
        if (isMoving)
        {
            MovePlayer();
        }
    }

    private void SetTargetPosition()
    {
        // Create a ray from the camera through the mouse position
        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer))
        {
            targetPosition = hit.point;
            // Keep the target at the player's current height to avoid tilting
            targetPosition.y = transform.position.y; 
            isMoving = true;
        }
    }

    private void MovePlayer()
    {
        // Calculate distance to target
        var distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > 0.1f)
        {
            // Rotate towards target
            var direction = (targetPosition - transform.position).normalized;
            var lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

            // Move forward
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
        else
        {
            isMoving = false;
        }
    }
}
