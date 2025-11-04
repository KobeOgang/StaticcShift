using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier = 0.4f;
    bool readyToJump;

    [Header("Air Control")]
    public float airControlForce = 8f;
    public float maxAirSpeed = 8f;
    public float airDrag = 1f;
    public bool allowAirAcceleration = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Stair Handling")]
    [SerializeField] GameObject stepRayUpper;
    [SerializeField] GameObject stepRayLower;
    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] float stepSmooth = 0.1f;
    [SerializeField] float stepRayLowerDistance = 0.6f;
    [SerializeField] float stepRayUpperDistance = 0.6f;
    [SerializeField] LayerMask whatIsStairs;

    [Header("Movement Restrictions")]
    private bool isMovementRestricted = false;
    private float movementMultiplier = 1f;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    public bool IsClimbingStairs { get; private set; }

    Vector3 moveDirection;
    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = airDrag;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed * movementMultiplier;
        }
        else if (grounded && Input.GetKey(sprintKey) && !isMovementRestricted)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed * movementMultiplier;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed * movementMultiplier;
        }
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded && !Input.GetKey(crouchKey))
        {
            StepClimb();
        }

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            HandleAirMovement();
        }

        rb.useGravity = !OnSlope();
    }

    private void StepClimb()
    {
        IsClimbingStairs = false;

        if (stepRayLower == null || stepRayUpper == null)
        {
            return;
        }

        RaycastHit hitLower;
        if (Physics.Raycast(stepRayLower.transform.position, orientation.TransformDirection(Vector3.forward), out hitLower, stepRayLowerDistance, whatIsStairs))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepRayUpper.transform.position, orientation.TransformDirection(Vector3.forward), out hitUpper, stepRayUpperDistance, whatIsStairs))
            {
                rb.position -= new Vector3(0f, -stepSmooth, 0f);
                IsClimbingStairs = true;
            }
        }

        RaycastHit hitLower45;
        if (Physics.Raycast(stepRayLower.transform.position, orientation.TransformDirection(1.5f, 0, 1), out hitLower45, stepRayLowerDistance, whatIsStairs))
        {
            RaycastHit hitUpper45;
            if (!Physics.Raycast(stepRayUpper.transform.position, orientation.TransformDirection(1.5f, 0, 1), out hitUpper45, stepRayUpperDistance, whatIsStairs))
            {
                rb.position -= new Vector3(0f, -stepSmooth, 0f);
                IsClimbingStairs = true;
            }
        }

        RaycastHit hitLowerMinus45;
        if (Physics.Raycast(stepRayLower.transform.position, orientation.TransformDirection(-1.5f, 0, 1), out hitLowerMinus45, stepRayLowerDistance, whatIsStairs))
        {
            RaycastHit hitUpperMinus45;
            if (!Physics.Raycast(stepRayUpper.transform.position, orientation.TransformDirection(-1.5f, 0, 1), out hitUpperMinus45, stepRayUpperDistance, whatIsStairs))
            {
                rb.position -= new Vector3(0f, -stepSmooth, 0f);
                IsClimbingStairs = true;
            }
        }

        stepRayUpper.transform.localPosition = new Vector3(stepRayUpper.transform.localPosition.x, stepHeight, stepRayUpper.transform.localPosition.z);
    }

    private void HandleAirMovement()
    {
        if (moveDirection.magnitude < 0.1f) return;

        Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 desiredDirection = moveDirection.normalized;

        float restrictedAirControlForce = airControlForce * movementMultiplier;

        float currentSpeed = currentHorizontalVelocity.magnitude;
        float dotProduct = Vector3.Dot(currentHorizontalVelocity.normalized, desiredDirection);

        if (dotProduct > 0.5f)
        {
            if (allowAirAcceleration && currentSpeed < maxAirSpeed)
            {
                float accelerationForce = restrictedAirControlForce;
                rb.AddForce(desiredDirection * accelerationForce, ForceMode.Force);
            }
            else if (currentSpeed >= maxAirSpeed)
            {
                rb.AddForce(desiredDirection * (restrictedAirControlForce * 0.3f), ForceMode.Force);
            }
        }
        else
        {
            float directionChangeForce = restrictedAirControlForce * 1.2f;
            rb.AddForce(desiredDirection * directionChangeForce, ForceMode.Force);
        }

        rb.AddForce(moveDirection.normalized * moveSpeed * 5f * airMultiplier * movementMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            float speedLimit = grounded ? moveSpeed : maxAirSpeed;

            if (flatVel.magnitude > speedLimit)
            {
                Vector3 limitedVel = flatVel.normalized * speedLimit;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    public void SetMovementRestriction(bool restricted, float multiplier)
    {
        isMovementRestricted = restricted;
        movementMultiplier = multiplier;

        if (restricted)
        {
            Debug.Log($"Movement restricted! Speed multiplier: {multiplier}");
        }
        else
        {
            Debug.Log("Movement restriction lifted!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || stepRayLower == null || stepRayUpper == null) return;

        Vector3 forwardDir = transform.TransformDirection(Vector3.forward);
        Vector3 dir45 = transform.TransformDirection(1.5f, 0, 1);
        Vector3 dirMinus45 = transform.TransformDirection(-1.5f, 0, 1);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(stepRayLower.transform.position, 0.05f);
        Gizmos.DrawRay(stepRayLower.transform.position, forwardDir * stepRayLowerDistance);
        Gizmos.DrawRay(stepRayLower.transform.position, dir45.normalized * stepRayLowerDistance);
        Gizmos.DrawRay(stepRayLower.transform.position, dirMinus45.normalized * stepRayLowerDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(stepRayUpper.transform.position, 0.05f);
        Gizmos.DrawRay(stepRayUpper.transform.position, forwardDir * stepRayUpperDistance);
        Gizmos.DrawRay(stepRayUpper.transform.position, dir45.normalized * stepRayUpperDistance);
        Gizmos.DrawRay(stepRayUpper.transform.position, dirMinus45.normalized * stepRayUpperDistance);
    }
}
