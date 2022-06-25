using UnityEngine;
using System.Collections;

// https://forums.oculusvr.com/t5/Unity-VR-Development/How-do-I-move-the-Camera/td-p/278806
public class FlyControl : MonoBehaviour
{
    public float speed = 10;
    public bool isEnabled = true;

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.ScrollLock))
        {
            isEnabled = !isEnabled;
        }

        if (!isEnabled)
            return;

        if (Input.GetKey(KeyCode.A))
        {
            Strafe(speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            Strafe(-speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W))
        {
            Fly(speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            Fly(-speed * Time.deltaTime);
        }

        float dx = Input.GetAxis("Mouse X");
        float dy = Input.GetAxis("Mouse Y");
        Look(new Vector3(dx, dy, 0.0f) * 1.25f);
    }
#endif
    void Strafe(float dist)
    {
        transform.Translate(Vector3.left * dist);
    }

    void Fly(float dist)
    {
        transform.Translate(Vector3.forward * dist);
    }

    void Look(Vector3 dist)
    {
        Vector3 angles = transform.eulerAngles;
        angles += new Vector3(-dist.y, dist.x, dist.z);
        transform.eulerAngles = new Vector3(ClampAngle(angles.x), angles.y, angles.z);
    }

    float ClampAngle(float angle)
    {
        if (angle < 180f)
        {
            if (angle > 80f) angle = 80f;
        }
        else
        {
            if (angle < 280f) angle = 280f;
        }
        return angle;
    }
}