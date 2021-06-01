using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  [SerializeField] Rigidbody playerRigidbody;
  [SerializeField] Transform groundCheck;
  [SerializeField] Transform orientation;
  [SerializeField] LayerMask mask;

  RaycastHit slopeHit;

  // float playerHeight = 2f;

  [Header("Player Input")]
  Vector3 moveDirection;
  Vector3 slopeMoveDirection;
  Vector3 playerVelocity;
  float moveMag;

  [Header("Movement")]
  [SerializeField] float jumpForce = 15f;
  [SerializeField] float walkSpeed = 6f;
  [SerializeField] float sprintSpeed = 100f;
  float speed;
  float movementMultiplier = 10f;
  [SerializeField] float airMovementMultiplier = 0.5f;

  [Header("Drag")]
  float playerDrag;
  float groundDrag = 6f;
  float airDrag = 2f;

  bool isGrounded;
  bool isSprinting;


  //START
  void Start()
  {
    playerRigidbody = GetComponent<Rigidbody>();
  }

  //UPDATE
  void Update()
  {
    isGrounded = Physics.Raycast(groundCheck.position, -groundCheck.up, 0.4f, mask);
    // Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red);

    MyInput();
    ControlDrag();

    // if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
    // {
    //   Jump();
    // }
    if (Input.GetKeyDown(KeyCode.Space))
    {
      Jump();
    }

    slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);

  }

  //FIXED UPDATE
  void FixedUpdate()
  {
    MovePlayer();
  }


  void MyInput()
  {
    float x = Input.GetAxisRaw("Horizontal");
    float y = Input.GetAxisRaw("Vertical");
    moveDirection = (orientation.right * x + orientation.forward * y).normalized;
    moveMag = moveDirection.magnitude;

    if (moveMag >= 0.1f)
    {
      //to avoid diagonal speed exceeding
      moveDirection = Vector3.ClampMagnitude(moveDirection, 1);

      // SPRINT
      // isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded;
      isSprinting = Input.GetKey(KeyCode.LeftShift);
      speed = isSprinting ? sprintSpeed : walkSpeed;

    }
  }

  void MovePlayer()
  {
    if (moveMag >= 0.1f)
    {
      if (isGrounded && !OnSlope())
      {
        playerVelocity = moveDirection * speed * movementMultiplier * Time.fixedDeltaTime;
      }
      else if (isGrounded && OnSlope())
      {
        playerVelocity = slopeMoveDirection * speed * movementMultiplier * Time.fixedDeltaTime;
      }
      else if (!isGrounded)
      {
        playerVelocity = moveDirection * speed * movementMultiplier * airMovementMultiplier * Time.fixedDeltaTime;
      }

      playerRigidbody.AddForce(playerVelocity, ForceMode.VelocityChange);
    }
  }

  void ControlDrag()
  {
    playerDrag = isGrounded ? groundDrag : airDrag;
    playerRigidbody.drag = playerDrag;
  }

  bool OnSlope()
  {
    if (Physics.Raycast(groundCheck.position, -groundCheck.up, out slopeHit, 0.4f, mask))
    {
      //returns true if onSlope
      if (slopeHit.normal != Vector3.up)
      {
        return true;
      }
      else
      {
        return false;
      }
    }
    return false;
  }

  void Jump()
  {
    // if (isGrounded)
    // {
    playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
    playerRigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    // }
  }
}
