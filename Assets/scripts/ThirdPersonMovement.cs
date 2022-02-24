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

    // Makes sure the character turns smooth
    public float smoothTurnTime = 0.1f;
    float turnSmoothVelocity;

    // The camera element
    public Transform cam;

    // Gravity
    public float gravity = -10f;

    // Ground check
    public Transform groundCheck;
    public LayerMask groundMask;
    bool isGrounded;

    // Velocity
    Vector3 velocity;

    // Spawnpoint
    public GameObject spawnPoint;

    // Update is called once per frame
    void Update()
    {
        // Gravity
        isGrounded = Physics.CheckSphere(groundCheck.position, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }


        // Player controls & Camera follow
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothTurnTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }
}
