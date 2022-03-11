using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    // The player controller
    public CharacterController controller;

    // Player attributes
    public float speed = 6f;
    public float minHeight = -10f; // Not being used yet
    public float jumpForce = 10.0f;
    private float directionY;

    // Makes sure the character turns smooth
    public float smoothTurnTime = 0.1f;
    float turnSmoothVelocity;

    // The camera element
    public Transform cam;

    // Gravity
    public float gravity = -5f;

    // Ground check
    public Transform groundCheck;
    public LayerMask groundMask;
    public bool isGrounded;

    // Velocity
    public Vector3 velocity;

    // Spawnpoint
    public GameObject spawnPoint;

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;

        if (controller.isGrounded)
        {
            velocity.y = 0;
            Time.timeScale = 1f;
        }
        else
        {
            velocity.y = directionY;
            Time.timeScale = 0.5f;
        }

        // Player controls & Camera follow
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            Debug.Log("space pressed");
            directionY = jumpForce;
        }

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothTurnTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        directionY += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);


    }
}
