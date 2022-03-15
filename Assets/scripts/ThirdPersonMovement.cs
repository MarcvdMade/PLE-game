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
    public float directionX;
    public float directionY;

    // Makes sure the character turns smooth
    public float smoothTurnTime = 0.1f;
    float turnSmoothVelocity;

    // The camera element
    public Transform cam;

    // Gravity
    public float gravity = -98f;

    // Ground check
    public LayerMask groundMask;
    public bool isGrounded;

    // Object check
    public LayerMask objectMask;

    // Velocity
    public Vector3 velocity;

    // Spawnpoint
    public GameObject spawnPoint;

    // Aim object
    public GameObject aimObject;

    //private void OnCollisionEnter(Collision collision)
    //{
    //    Debug.Log(collision.collider.gameObject.layer);
    //    if (collision.collider.gameObject.layer == 0)
    //    {
    //        Debug.Log("touched ground");
    //        isGrounded = true;
    //    }
    //}
    //
    //private void OnCollisionExit(Collision collision)
    //{
    //    Debug.Log("collision exited");
    //    if (collision.collider.gameObject.layer == 0)
    //    {
    //        isGrounded = false;
    //    }
    //}

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            velocity.y = 0;
            velocity.x = 0;
            directionX = 0;
            Time.timeScale = 1f;

            // Set aim unactive
            aimObject.SetActive(false);
        }
        
        if (!isGrounded)
        {
            velocity.y = directionY;
            velocity.x = directionX;
            Time.timeScale = 0.5f;

            directionY += gravity * Time.deltaTime;

            // Set aim object to mouse pointer
            aimObject.SetActive(true);
            Vector3 mousePos = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, groundMask))
            {
                aimObject.transform.position = hit.point;
            }

            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, objectMask))
            {
                aimObject.transform.position = hit.point;
            }
        }

        // Player controls & Camera follow
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Debug.Log("space pressed");
            directionY = jumpForce;
            directionX = 0;
        }

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothTurnTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        controller.Move(velocity * Time.deltaTime);

        if (controller.gameObject.transform.position.y < minHeight)
        {
            controller.gameObject.transform.position = spawnPoint.transform.position;
        }
    }
}
