using UnityEngine;

public class Player : MonoBehaviour
{

	[SerializeField, Range(0f, 100f)]
	float maxSpeed = 10f;

	[SerializeField, Range(0f, 100f)]
	float maxAcceleration = 10f, maxAirAcceleration = 1f;

	[SerializeField, Range(0f, 10f)]
	float jumpHeight = 2f;

	[SerializeField, Range(0, 5)]
	int maxAirJumps = 0;

	[SerializeField, Range(0f, 90f)]
	float maxGroundAngle = 25f;

	Vector3 velocity, desiredVelocity;

	Rigidbody body;

	bool desiredJump;

	bool onGround;

	int jumpPhase;

	float minGroundDotProduct;

	Vector3 contactNormal;

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
		OnValidate();
    }

    private void OnCollisionEnter(Collision collision)
    {
		EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
		EvaluateCollision(collision);
    }

	void EvaluateCollision (Collision collision)
    {
		for (int i = 0; i < collision.contactCount; i++)
        {
			Vector3 normal = collision.GetContact(i).normal;
			if (normal.y >= minGroundDotProduct)
            {
				onGround = true;
				contactNormal += normal;
            }
        }
    }

    void Update()
	{
		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
		playerInput = Vector2.ClampMagnitude(playerInput, 1f);

		desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

		desiredJump |= Input.GetButtonDown("Jump");
	}

    void FixedUpdate()
    {
		UpdateState();
		AdjustVelocity();

		if (desiredJump)
        {
			desiredJump = false;
			Jump();
        }

		body.velocity = velocity;
		ClearState();
	}

	void ClearState()
    {
		onGround = false;
		contactNormal = Vector3.zero;
    }

	void UpdateState()
    {
		velocity = body.velocity;

		if (onGround)
        {
			jumpPhase = 0;
			contactNormal.Normalize();
        } else
        {
			contactNormal = Vector3.up;
        }
    }

	void Jump()
    {
		if (onGround || jumpPhase < maxAirJumps)
        {
			jumpPhase += 1;
			float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
			float alignedSpeed = Vector3.Dot(velocity, contactNormal);
			if (alignedSpeed > 0f)
            {
				jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
			velocity += contactNormal * jumpSpeed;
        }
    }

	Vector3 ProjectOnContactPlane(Vector3 vector)
    {
		return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

	void AdjustVelocity()
    {
		Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
		Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

		float currentX = Vector3.Dot(velocity, xAxis);
		float currentZ = Vector3.Dot(velocity, zAxis);

		float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		float newX =
			Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
		float newZ =
			Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

		velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
	}
}