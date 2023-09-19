using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public Transform player;
    public float sensitivity = 100f;
    public float clampAngle = 65f;

    private float verticalRotation;
    private float horizontalRotation;

    // Start is called before the first frame update
    void Start()
    {
        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = player.transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        Look();
    }

    private void Look()
    {
        float mouseVertical = -Input.GetAxis("Mouse Y");
        float mouseHorizontal = Input.GetAxis("Mouse X");

        verticalRotation += mouseVertical * sensitivity * Time.deltaTime;
        horizontalRotation += mouseHorizontal * sensitivity * Time.deltaTime;

        verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        player.transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
    }
}
