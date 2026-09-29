using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private float _currentSpeed;
    private float _roundSpeedLimit;

    public float speed = 100.0f;

    // Ball speed setup
    [Header("Rally Acceleration")]
    [Tooltip("Speed increase per paddle hit. 0.1 = 10%.")]
    [Min(0f)]
    public float speedIncreasePerHit = 0.1f;

    // Speed limitation
    [Tooltip("Maximum speed relative to the starting speed.")]
    [Min(1f)]
    public float maxSpeedMultiplier = 3f;

    // Bounce angle
    [Header("Bounce Angle")]
    [Tooltip("Maximum bounce angle measured from horizontal.")]
    [Range(0f, 70f)]
    public float maxBounceAngle = 60f;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();

        // Reduce the risk of passing through colliders at high speed.
        _rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

    }

    public void ResetBall()
    {
        _rigidBody.linearVelocity = Vector2.zero;
        _rigidBody.angularVelocity = 0;
        transform.position = Vector3.zero;

        // Clear acceleration from the previous round.
        _currentSpeed = 0f;
        _roundSpeedLimit = 0f;

    }

    public void AddStartingForce()
    {
        float x = Random.value < 0.5f ? -1.0f : 1.0f;
        float y = (Random.value < 0.5f ? -1.0f : 1.0f) * Random.Range(0.5f, 0.9f);

        Vector2 direction = new Vector2(x, y);

        // preserve the original launch force.
        _currentSpeed = (direction * speed).magnitude * Time.fixedDeltaTime / _rigidBody.mass;

        _roundSpeedLimit = _currentSpeed * Mathf.Max(1f, maxSpeedMultiplier);

        _rigidBody.AddForce(direction * speed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only increase speed when paddle hits.
        Paddle paddle = collision.gameObject.GetComponentInParent<Paddle>();

        if (paddle == null) return;
        if (_currentSpeed <= 0f) return;

        Collider2D paddleCollider = collision.collider;

        // Bounds account for paddle's size and scale.
        Bounds bounds = paddleCollider.bounds;
        float halfHeight = bounds.extents.y;

        if (halfHeight <= 0.0001f) return;

        // Hit position
        float hitPosition = Mathf.Clamp(
        (_rigidBody.position.y - bounds.center.y) / halfHeight,
        -1f,
        1f
   );

        float angle = hitPosition
                  * Mathf.Clamp(maxBounceAngle, 0f, 70f)
                  * Mathf.Deg2Rad;

        // Send ball away from the paddle.
        float horizontalDirection =
        _rigidBody.position.x >= bounds.center.x ? 1f : -1f;

        Vector2 direction = new Vector2(
        Mathf.Cos(angle) * horizontalDirection,
        Mathf.Sin(angle)
   );

        // Keep existing accleration and speed limit
        _currentSpeed = Mathf.Min(
       _currentSpeed * (1f + Mathf.Max(0f, speedIncreasePerHit)),
       _roundSpeedLimit
   );

        _rigidBody.linearVelocity = direction * _currentSpeed;
    }

}