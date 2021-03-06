using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerGravity : MonoBehaviour
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
	float maxGroundAngle = 25f, maxStairsAngle = 50f;

	[SerializeField, Range(0f, 100f)]
	float maxSnapSpeed = 100f;

	// This is the distance for snapping to the ground.
	[SerializeField, Min(0f)]
	float probeDistance = 1f;

	// Makes sure the character turns smooth
	[SerializeField, Range(0f, 1f)]
	public float smoothTurnTime = 0.1f;

	float turnSmoothVelocity;

	[SerializeField]
	LayerMask probeMask = -1, stairsMask = -1;

	[SerializeField]
	Transform cam;

	[SerializeField]
	Transform spawnPoint;

	[SerializeField]
	public string nextLevelName;

	[SerializeField]
	Transform playerInputSpace = default;

	//[SerializeField]
	//Transform customWorldUp;

	Vector3 velocity, desiredVelocity;

	Rigidbody body;

	Animator animator;

	bool desiredJump;

	Vector3 contactNormal, steepNormal;

	int groundContactCount, steepContactCount;

	bool OnGround => groundContactCount > 0;

	bool OnSteep => steepContactCount > 0;

	int jumpPhase;

	float minGroundDotProduct, minStairsDotProduct;


	int stepsSinceLastGrounded, stepsSinceLastJump;

	Vector3 upAxis, rightAxis, forwardAxis;

	private void OnValidate()
	{
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
		minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
	}

	private void Awake()
	{
		body = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		body.useGravity = false;
		OnValidate();
	}

	void OnCollisionEnter(Collision collision)
	{
		EvaluateCollision(collision);

		// Check if water was hit
		if (collision.gameObject.layer == 4)
		{
			body.position = spawnPoint.position;
		}

		// Check if portal was hit
		if (collision.gameObject.CompareTag("Portal"))
		{
			SceneManager.LoadScene(nextLevelName);
			Debug.Log("touched portal");
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void EvaluateCollision(Collision collision)
	{
		float minDot = GetMinDot(collision.gameObject.layer);
		for (int i = 0; i < collision.contactCount; i++)
		{
			Vector3 normal = collision.GetContact(i).normal;
			float upDot = Vector3.Dot(upAxis, normal);
			if (upDot >= minDot)
			{
				groundContactCount += 1;
				contactNormal += normal;
			}
			else if (upDot > -0.01f)
			{
				steepContactCount += 1;
				steepNormal += normal;
			}
		}
	}

	void Update()
	{
		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
		playerInput = Vector2.ClampMagnitude(playerInput, 1f);
		// Vector3 direction = new Vector3(playerInput.x, 0f, playerInput.y).normalized;

		if (playerInputSpace)
        {
			rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
			forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        } else
        {
			rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
			forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }
		// old
		desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

		//desiredVelocity = playerInputSpace.right * playerInput.x * maxSpeed;
		//desiredVelocity += playerInputSpace.forward * playerInput.y * maxSpeed;			


		desiredJump |= Input.GetButtonDown("Jump");

		// if (direction.magnitude >= 0.1f)
		// {
		// 	float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
		// 	float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothTurnTime);
		// 
		// 	transform.rotation = Quaternion.Euler(0f, angle, 0f);
		// }

		// customWorldUp.rotation = Quaternion.LookRotation(transform.forward, contactNormal);
		transform.rotation = Quaternion.LookRotation(playerInputSpace.transform.forward, upAxis);

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			SceneManager.LoadScene(0);
		}

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			SceneManager.LoadScene(1);
		}
	}

	void FixedUpdate()
	{
		upAxis = -Physics.gravity.normalized;
		Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);

		UpdateState();
		AdjustVelocity();

		if (desiredJump)
		{
			desiredJump = false;
			Jump(gravity);
		}

		velocity += gravity * Time.deltaTime;

		body.velocity = velocity;
		ClearState();
	}

	void ClearState()
	{
		groundContactCount = steepContactCount = 0;
		contactNormal = steepNormal = Vector3.zero;
	}

	void UpdateState()
	{
		stepsSinceLastGrounded += 1;
		stepsSinceLastJump += 1;
		velocity = body.velocity;

		if (OnGround || SnapToGround() || CheckSteepContacts())
		{
			stepsSinceLastGrounded = 0;
			if (stepsSinceLastJump > 1)
			{
				jumpPhase = 0;
			}
			if (groundContactCount > 1)
			{
				contactNormal.Normalize();
			}
		}
		else
		{
			contactNormal = upAxis;
		}
	}

	void Jump(Vector3 gravity)
	{
		Vector3 jumpDirection;
		if (OnGround)
		{
			jumpDirection = contactNormal;
		}
		else if (OnSteep)
		{
			jumpDirection = steepNormal;
			jumpPhase = 0;
		}
		else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
		{
			if (jumpPhase == 0)
			{
				jumpPhase = 1;
			}
			jumpDirection = contactNormal;
		}
		else
		{
			return;
		}

		stepsSinceLastJump = 0;
		jumpPhase += 1;
		float jumpSpeed = Mathf.Sqrt(2f * Physics.gravity.magnitude * jumpHeight);
		jumpDirection = (jumpDirection + upAxis).normalized;
		float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
		if (alignedSpeed > 0f)
		{
			jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
		}
		velocity += jumpDirection * jumpSpeed;
	}

	Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
		return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

	void AdjustVelocity()
	{
		Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
		Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

		float currentX = Vector3.Dot(velocity, xAxis);
		float currentZ = Vector3.Dot(velocity, zAxis);

		float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		float newX =
			Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
		float newZ =
			Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

		velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
	}

	bool SnapToGround()
	{
		if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
		{
			return false;
		}
		float speed = velocity.magnitude;
		if (speed > maxSnapSpeed)
		{
			return false;
		}
		if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask))
		{
			return false;
		}
		float upDot = Vector3.Dot(upAxis, hit.normal);
		if (upDot < GetMinDot(hit.collider.gameObject.layer))
		{
			return false;
		}
		groundContactCount = 1;
		contactNormal = hit.normal;
		float dot = Vector3.Dot(velocity, hit.normal);
		if (dot > 0f)
		{
			velocity = (velocity - hit.normal * dot).normalized * speed;
		}
		return true;
	}

	float GetMinDot(int layer)
	{
		return (stairsMask & (1 << layer)) == 0 ?
			minGroundDotProduct : minStairsDotProduct;
	}

	bool CheckSteepContacts()
	{
		if (steepContactCount > 1)
		{
			steepNormal.Normalize();
			float upDot = Vector3.Dot(upAxis, steepNormal);
			if (upDot >= minGroundDotProduct)
			{
				groundContactCount = 1;
				contactNormal = steepNormal;
				return true;
			}
		}
		return false;
	}
}