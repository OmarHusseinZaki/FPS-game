using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    #region Variables
    [SerializeField]
    private float _speed = 300f;
    [SerializeField]
    private float _sprintModifier = 2;
    private Rigidbody _rig;
    public Camera FPScam;
    public GameObject cameraParent;
    public Transform weaponParent;
    private Vector3 _targetWepBobPos;
    private float _baseFov;
    private float _movementCounter;
    private float _idleCounter;
    private float _sprintFovModifier = 1.15f;
    [SerializeField]
    private float _jumpForce = 500f;
    public Transform groundDetector;
    public LayerMask ground;
    private Vector3 _weaponParentOrigin;
    public int maxHealth;
    private int _currHealth;
    private Manager manager;
    #endregion

    #region MonoBehaviour Callbacks
    void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        if(manager == null)
        {
            Debug.LogError("Manager is null");
        }
        _currHealth = maxHealth;
        cameraParent.SetActive(photonView.IsMine);
        if (!photonView.IsMine) gameObject.layer = 11;

        _baseFov = FPScam.fieldOfView;
        if(Camera.main) Camera.main.enabled = false;
        _rig = GetComponent<Rigidbody>();
        _weaponParentOrigin = weaponParent.localPosition;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

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
        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(700);

        //HeadBob
        if (hMove == 0 && vMove == 0) 
        { 
            HeadBob(_idleCounter, 0.025f, 0.025f); 
            _idleCounter += Time.deltaTime;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, _targetWepBobPos, Time.deltaTime * 2f);
        }
        else if(!isSprinting)
        { 
            HeadBob(_movementCounter, 0.035f, 0.035f); 
            _movementCounter += Time.deltaTime * 3f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, _targetWepBobPos, Time.deltaTime * 6f);
        }
        else
        {
            HeadBob(_movementCounter, 0.15f, 0.075f);
            _movementCounter += Time.deltaTime * 7f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, _targetWepBobPos, Time.deltaTime * 10f);
        }
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

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

    void HeadBob(float z, float xIntensity, float yIntensity)
    {
        _targetWepBobPos = _weaponParentOrigin + new Vector3(Mathf.Cos(z) * xIntensity, Mathf.Sin(z * 2) * yIntensity, 0);
    }

    public void TakeDamage(int damage)
    {
        if (photonView.IsMine)
        {
            _currHealth -= damage;
            Debug.Log(_currHealth);
        }

        if(_currHealth <= 0)
        {
            manager.Spawn();
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
