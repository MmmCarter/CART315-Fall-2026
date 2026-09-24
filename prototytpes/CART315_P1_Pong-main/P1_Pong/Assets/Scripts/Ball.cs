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

        _currentSpeed = Mathf.Min(
       _currentSpeed * (1f + Mathf.Max(0f, speedIncreasePerHit)),
       _roundSpeedLimit
   );

        Vector2 velocity = _rigidBody.linearVelocity;

        float horizontalDirection =
        transform.position.x >= paddle.transform.position.x ? 1f : -1f;

        Vector2 direction = velocity.sqrMagnitude > 0.0001f
            ? velocity.normalized
            : new Vector2(horizontalDirection, 0f);

        // Maximum angle from horizontal
        const float minHorizontalComponent = 0.5f;

        float horizontal = Mathf.Max(
            Mathf.Abs(direction.x),
            minHorizontalComponent
        );

        float vertical = Mathf.Sqrt(
            Mathf.Max(0f, 1f - horizontal * horizontal)
        );

        float verticalDirection = direction.y >= 0f ? 1f : -1f;

        direction = new Vector2(
            horizontalDirection * horizontal,
            verticalDirection * vertical
        );

        _rigidBody.linearVelocity = direction * _currentSpeed;
    }

}