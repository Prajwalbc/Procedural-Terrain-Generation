using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
  [SerializeField] Transform cam;
  [SerializeField] Transform orientation;

  [SerializeField] float XmouseSensitivity = 100f;
  [SerializeField] float YmouseSensitivity = 100f;

  float xRotation = 0f;
  float yRotation = 0f;

  [SerializeField] float smoothTurn = 1f;
  Vector3 smoothRotation;
  Vector3 targetRotation;
  Vector3 velocity;

  // Start
  void Start()
  {
    Cursor.lockState = CursorLockMode.Locked;
    smoothTurn *= 0.01f;
  }

  // Update
  void Update()
  {
    if (Input.GetKey(KeyCode.Escape))
    {
      Cursor.lockState = CursorLockMode.None;
    }
    if (Input.GetKey(KeyCode.Mouse0))
    {
      Cursor.lockState = CursorLockMode.Locked;
    }

    float mouseX = Input.GetAxisRaw("Mouse X") * XmouseSensitivity * Time.deltaTime;
    float mouseY = Input.GetAxisRaw("Mouse Y") * YmouseSensitivity * Time.deltaTime;

    // rotation
    xRotation -= mouseY;
    xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    yRotation += mouseX;

    //smooth rotation
    targetRotation = new Vector3(xRotation, yRotation, 0);
    smoothRotation = Vector3.SmoothDamp(smoothRotation, targetRotation, ref velocity, smoothTurn);

    cam.transform.localRotation = Quaternion.Euler(smoothRotation);
    orientation.transform.rotation = Quaternion.Euler(Vector3.up * yRotation);
  }
}
