using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Look : MonoBehaviourPunCallbacks
{
    #region Variables
    public static bool cursorLock = true;
    public Transform player;
    public Transform cams;
    public Transform weapon;
    public float xSens;
    public float ySens;
    private float _maxAngle = 70;
    private Quaternion _camsCenter;
    #endregion

    #region MonoBehaviour Callbacks
    void Start()
    {
        _camsCenter = cams.localRotation;
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        SetY();
        SetX();
        UpdateCursorLock();
    }
    #endregion


    void SetY()
    {
        float input = Input.GetAxis("Mouse Y") * ySens * Time.fixedDeltaTime;
        Quaternion adj = Quaternion.AngleAxis(input, -Vector3.right);
        Quaternion delta = cams.localRotation * adj;
        if(Quaternion.Angle(_camsCenter, delta) < _maxAngle)
        {
            cams.localRotation = delta;
        }
        weapon.rotation = cams.rotation;
    }

    void SetX()
    {
        float input = Input.GetAxis("Mouse X") * xSens * Time.fixedDeltaTime;
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
