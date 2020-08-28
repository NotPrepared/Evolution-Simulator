using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerController : MonoBehaviour
{
    public float speed = 20f;
    public Vector2 mD;
    

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        var movement = Input.GetAxis("Vertical");
        movement *= Time.deltaTime;

        var mC = new Vector2(Input.GetAxisRaw("Mouse X") * 3f, Input.GetAxisRaw("Mouse Y") * 3f);
        
        var rotationEulerAngles = transform.rotation.eulerAngles;
        if (rotationEulerAngles.x < 42f || rotationEulerAngles.x > 360f - 42f)
        {
            mD += mC;
        }
        else
        {
            mD.y -= mC.y * 3f;
        }

        var qR = Quaternion.AngleAxis (mD.x, Vector3.up);
        
        var localRotation = qR * Quaternion.AngleAxis (-mD.y, Vector3.right);
        transform.localRotation = localRotation;
        transform.Translate(Vector3.forward * movement);
    }

    //void FixedUpdate()
    //{
    //    var moveHorizontal = Input.GetAxis("Horizontal");
    //    var moveVertical = Input.GetAxis("Vertical");

    //    Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
    //    transform.Translate(movement.normalized * speed);
    //}
}