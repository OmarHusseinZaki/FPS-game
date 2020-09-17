using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Sway : MonoBehaviourPunCallbacks
{
    #region Variables

    public float intensity;
    public float smooth;
    private Quaternion _originRotation;
    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        _originRotation = transform.localRotation;   
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        UpdateSway();
    }
    #endregion

    void UpdateSway()
    {
        // Controls
        float xMouse = Input.GetAxis("Mouse X");
        float yMouse = Input.GetAxis("Mouse Y");

        //Calculate target rotation
        Quaternion xAdj = Quaternion.AngleAxis(-intensity * xMouse, Vector3.up);
        Quaternion yAdj = Quaternion.AngleAxis(intensity * yMouse, Vector3.right);
        Quaternion _targetRotation = _originRotation * xAdj * yAdj;

        // Rotate towards target
        transform.localRotation = Quaternion.Lerp(transform.localRotation, _targetRotation, Time.deltaTime * smooth);
    }
}
