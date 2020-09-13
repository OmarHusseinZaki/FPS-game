using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _speed = 300f;
    [SerializeField]
    private float _sprintModifier = 2;
    private Rigidbody _rig;
    public Camera FPScam;
    private float _baseFov;
    private float _sprintFovModifier = 1.15f;
    // Start is called before the first frame update
    void Start()
    {
        _baseFov = FPScam.fieldOfView;
        Camera.main.enabled = false;
        _rig = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float hMove = Input.GetAxisRaw("Horizontal");
        float vMove = Input.GetAxisRaw("Vertical");

        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isSprinting = sprint && vMove >0;

        Vector3 direction = new Vector3(hMove, 0, vMove);
        direction.Normalize();

        float adjustedSpeed = _speed;
        if (isSprinting) adjustedSpeed *= _sprintModifier;

        _rig.velocity = transform.TransformDirection(direction) * adjustedSpeed * Time.fixedDeltaTime;

        if (isSprinting) 
        { 
            FPScam.fieldOfView = Mathf.Lerp(FPScam.fieldOfView, _baseFov * _sprintFovModifier, Time.fixedDeltaTime * 8f); 
        }
        else { FPScam.fieldOfView = Mathf.Lerp(FPScam.fieldOfView, _baseFov, Time.fixedDeltaTime * 8f); }
    }
}
