using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    // The player controller
    // public CharacterController controller;
    public Rigidbody rig;

    // Player attributes
    public float speed;
    public float baseSpeed = 10f;
    public float sprintSpeed = 5f;

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

    private void Start()
    {
        Debug.Log("game started...");
        speed = baseSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // isGrounded = controller.isGrounded;

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
            //Time.timeScale = 0.5f;

            directionY += gravity * Time.deltaTime;

            // Set aim object to mouse pointer
            aimObject.SetActive(true);
            Vector3 mousePos = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(castPoint, out RaycastHit hit, Mathf.Infinity, groundMask))
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

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = baseSpeed + sprintSpeed;
        } else
        {
            speed = baseSpeed;
        }

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothTurnTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            rig.AddForce(moveDir.normalized * speed * Time.deltaTime);
        }

        rig.AddForce(velocity * Time.deltaTime);

        if (rig.gameObject.transform.position.y < minHeight || Input.GetKey(KeyCode.R))
        {
            rig.gameObject.transform.position = spawnPoint.transform.position;
        }
    }
}
