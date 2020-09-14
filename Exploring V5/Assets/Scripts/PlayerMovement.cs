using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private float _speed = 300f;
    [SerializeField]
    private float _sprintModifier = 2;
    private Rigidbody _rig;
    public Camera FPScam;
    private float _baseFov;
    private float _sprintFovModifier = 1.15f;
    [SerializeField]
    private float _jumpForce = 500f;
    public Transform groundDetector;
    public LayerMask ground;
    #endregion

    #region MonoBehaviour Callbacks
    void Start()
    {
        _baseFov = FPScam.fieldOfView;
        Camera.main.enabled = false;
        _rig = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Axis
        float hMove = Input.GetAxisRaw("Horizontal");
        float vMove = Input.GetAxisRaw("Vertical");

        // Controls
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);

        // States
        bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool isJumping = jump && isGrounded;
        bool isSprinting = sprint && vMove > 0 && !isJumping && isGrounded;

        // Jumping
        if (isJumping)
        {
            _rig.AddForce(Vector3.up * _jumpForce);
        }
    }

    void FixedUpdate()
    {
        // Axis
        float hMove = Input.GetAxisRaw("Horizontal");
        float vMove = Input.GetAxisRaw("Vertical");

        // Controls
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);

        // States
        bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool isJumping = jump && isGrounded;
        bool isSprinting = sprint && vMove >0 && !isJumping && isGrounded;


        // Movememnt
        Vector3 direction = new Vector3(hMove, 0, vMove);
        direction.Normalize();

        float adjustedSpeed = _speed;
        if (isSprinting) adjustedSpeed *= _sprintModifier;

        Vector3 targetVelocity = transform.TransformDirection(direction) * adjustedSpeed * Time.fixedDeltaTime;
        targetVelocity.y = _rig.velocity.y;
        _rig.velocity = targetVelocity;

        // FOV
        if (isSprinting) 
        { 
            FPScam.fieldOfView = Mathf.Lerp(FPScam.fieldOfView, _baseFov * _sprintFovModifier, Time.fixedDeltaTime * 8f); 
        }
        else { FPScam.fieldOfView = Mathf.Lerp(FPScam.fieldOfView, _baseFov, Time.fixedDeltaTime * 8f); }
    }
    #endregion
}
