using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum MovementAxis { X, Z }
    public MovementAxis axis = MovementAxis.X;
    public float moveDistance = 5f;
    public float moveSpeed = 2f;

    [Header("Obstacle Detection")]
    public float detectionDistance = 0.8f;
    public LayerMask obstacleLayer = -1;
    public bool canBeStoppedByHeldObjects = true;
    public bool canBeStoppedByAnchoredObjects = true;

    private Vector3 startPosition;
    private Vector3 targetPositionA;
    private Vector3 targetPositionB;
    private Vector3 currentTarget;
    private bool movingToB = true;
    private bool isStopped = false;
    private BoxCollider boxCollider;
    private Rigidbody rb;

    private void Start()
    {
        startPosition = transform.position;
        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (axis == MovementAxis.X)
        {
            targetPositionA = startPosition - Vector3.right * moveDistance;
            targetPositionB = startPosition + Vector3.right * moveDistance;
        }
        else
        {
            targetPositionA = startPosition - Vector3.forward * moveDistance;
            targetPositionB = startPosition + Vector3.forward * moveDistance;
        }

        currentTarget = targetPositionB;
    }

    private void FixedUpdate()
    {
        CheckForObstacles();

        if (!isStopped)
        {
            MovePlatform();
        }
    }

    private void MovePlatform()
    {
        Vector3 newPosition = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.fixedDeltaTime);

        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }

        if (Vector3.Distance(transform.position, currentTarget) < 0.01f)
        {
            movingToB = !movingToB;
            currentTarget = movingToB ? targetPositionB : targetPositionA;
        }
    }

    private void CheckForObstacles()
    {
        if (boxCollider == null) return;

        Vector3 direction = (currentTarget - transform.position).normalized;

        Vector3 boxCenter = transform.TransformPoint(boxCollider.center);
        Vector3 boxSize = Vector3.Scale(boxCollider.size, transform.localScale);

        float halfSizeInDirection;
        if (axis == MovementAxis.X)
        {
            halfSizeInDirection = boxSize.x * 0.3f;
        }
        else
        {
            halfSizeInDirection = boxSize.z * 0.3f;
        }

        Vector3 castOrigin = boxCenter + direction * halfSizeInDirection;

        Vector3 halfExtents = boxSize * 0.1f;

        RaycastHit[] hits = Physics.BoxCastAll(
            castOrigin,
            halfExtents,
            direction,
            transform.rotation,
            detectionDistance + 0.1f,
            obstacleLayer
        );

        isStopped = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == boxCollider) continue;
            if (hit.collider.transform == transform) continue;
            if (hit.collider.transform.IsChildOf(transform)) continue;

            if (hit.distance > detectionDistance) continue;

            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();

            if (interactable != null)
            {
                if (canBeStoppedByAnchoredObjects && interactable.IsAnchored)
                {
                    isStopped = true;
                    return;
                }

                if (canBeStoppedByHeldObjects && interactable.IsBeingManipulated)
                {
                    isStopped = true;
                    return;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 start = Application.isPlaying ? startPosition : transform.position;
        Vector3 endA, endB;

        if (axis == MovementAxis.X)
        {
            endA = start - Vector3.right * moveDistance;
            endB = start + Vector3.right * moveDistance;
        }
        else
        {
            endA = start - Vector3.forward * moveDistance;
            endB = start + Vector3.forward * moveDistance;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(endA, endB);
        Gizmos.DrawWireSphere(endA, 0.2f);
        Gizmos.DrawWireSphere(endB, 0.2f);

        if (Application.isPlaying && boxCollider != null)
        {
            Vector3 direction = (currentTarget - transform.position).normalized;
            Vector3 boxCenter = transform.TransformPoint(boxCollider.center);
            Vector3 boxSize = Vector3.Scale(boxCollider.size, transform.localScale);

            float halfSizeInDirection;
            if (axis == MovementAxis.X)
            {
                halfSizeInDirection = boxSize.x * 0.5f;
            }
            else
            {
                halfSizeInDirection = boxSize.z * 0.5f;
            }

            Vector3 castOrigin = boxCenter + direction * halfSizeInDirection;
            Vector3 halfExtents = boxSize * 0.45f;

            Gizmos.color = isStopped ? Color.red : Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(castOrigin, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.DrawRay(castOrigin, direction * (detectionDistance + 0.1f));
        }
    }
}
