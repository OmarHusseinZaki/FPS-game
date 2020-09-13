using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Look : MonoBehaviour
{
    public static bool cursorLock = true;
    public Transform player;
    public Transform cams;
    public float xSens;
    public float ySens;
    private float _maxAngle = 70;
    private Quaternion _camsCenter;
    // Start is called before the first frame update
    void Start()
    {
        _camsCenter = cams.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        SetY();
        SetX();
        UpdateCursorLock();
    }

    void SetY()
    {
        float input = Input.GetAxis("Mouse Y") * ySens;
        Quaternion adj = Quaternion.AngleAxis(input, -Vector3.right);
        Quaternion delta = cams.localRotation * adj;
        if(Quaternion.Angle(_camsCenter, delta) < _maxAngle)
        {
            cams.localRotation = delta;
        }
    }

    void SetX()
    {
        float input = Input.GetAxis("Mouse X") * xSens;
        Quaternion adj = Quaternion.AngleAxis(input, Vector3.up);
        Quaternion delta = player.localRotation * adj;
        player.localRotation = delta;
    }

    void UpdateCursorLock()
    {
        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLock = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLock = true;
            }
        }
    }
}
