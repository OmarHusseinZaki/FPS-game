using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Player : MonoBehaviourPunCallbacks
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
    private Vector3 _weaponParentCurPos;
    public int maxHealth;
    private int _currHealth;
    private Manager _manager;
    private Weapon _weapon;
    private Transform _UIHealthBar;
    private Text _UIAmmo;
    private bool _sliding;
    private float _slideTime;
    private Vector3 _slideDir;
    public float slideMod;
    public float lengthOfSlide;
    private Vector3 _origin;
    
    #endregion

    #region MonoBehaviour Callbacks
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager>();
        _weapon = GetComponent<Weapon>();
        if(_weapon == null)
        {
            Debug.LogError("Weapon is Null");
        }
        if(_manager == null)
        {
            Debug.LogError("Manager is null");
        }

        _currHealth = maxHealth;

        cameraParent.SetActive(photonView.IsMine);

        if (!photonView.IsMine) gameObject.layer = 11;

        _baseFov = FPScam.fieldOfView;
        _origin = FPScam.transform.localPosition;

        if(Camera.main) Camera.main.enabled = false;
        _rig = GetComponent<Rigidbody>();

        _weaponParentOrigin = weaponParent.localPosition;
        _weaponParentCurPos = _weaponParentOrigin;

        if (photonView.IsMine)
        {
            _UIHealthBar = GameObject.Find("HUD/Health/Bar").transform;
            _UIAmmo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
            RefreshHealthBar();
        }
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
        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(100);

        //HeadBob
        if (_sliding) { }
        else if (hMove == 0 && vMove == 0) 
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

        // UI Refreshes
        RefreshHealthBar();
        _weapon.RefreshAmmo(_UIAmmo);
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
        bool slide = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        // States
        bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool isJumping = jump && isGrounded;
        bool isSprinting = sprint && vMove > 0 && !isJumping && isGrounded;
        bool isSliding = slide && isSprinting && !_sliding;


        // Movememnt
        Vector3 direction = Vector3.zero;
        float adjustedSpeed = _speed;

        if (!_sliding)
        {
            direction = new Vector3(hMove, 0, vMove);
            direction.Normalize();
            direction = transform.TransformDirection(direction);
            

            if (isSprinting) adjustedSpeed *= _sprintModifier;
        }
        else
        {
            direction = _slideDir;
            adjustedSpeed *= slideMod;
            _slideTime -= Time.fixedDeltaTime;
            if (_slideTime <= 0) 
            {
                _sliding = false;
                _weaponParentCurPos += Vector3.up * 0.5f;
            }
        }

        Vector3 targetVelocity = direction * adjustedSpeed * Time.fixedDeltaTime;
        targetVelocity.y = _rig.velocity.y;
        _rig.velocity = targetVelocity;


        // Sliding
        if (isSliding)
        {
            _sliding = true;
            _slideDir = direction;
            _slideTime = lengthOfSlide;
            _weaponParentCurPos += Vector3.down * 0.5f;
        }

        // Camera adj
        if (_sliding)
        {
            FPScam.fieldOfView = Mathf.Lerp(FPScam.fieldOfView, _baseFov * _sprintFovModifier * 1.25f, Time.fixedDeltaTime * 8f);
            FPScam.transform.localPosition = Vector3.Lerp(FPScam.transform.localPosition, _origin + Vector3.down * 0.5f, Time.fixedDeltaTime * 6f);
        }
        else
        {
            if (isSprinting)
            {
                FPScam.fieldOfView = Mathf.Lerp(FPScam.fieldOfView, _baseFov * _sprintFovModifier, Time.fixedDeltaTime * 8f);
            }
            else { FPScam.fieldOfView = Mathf.Lerp(FPScam.fieldOfView, _baseFov, Time.fixedDeltaTime * 8f); }
            FPScam.transform.localPosition = Vector3.Lerp(FPScam.transform.localPosition, _origin, Time.fixedDeltaTime * 6f);
        }
    }
    #endregion

    void HeadBob(float z, float xIntensity, float yIntensity)
    {
        _targetWepBobPos = _weaponParentCurPos + new Vector3(Mathf.Cos(z) * xIntensity, Mathf.Sin(z * 2) * yIntensity, 0);
    }

    public void TakeDamage(int damage)
    {
        if (photonView.IsMine)
        {
            _currHealth -= damage;
            RefreshHealthBar();
        }

        if(_currHealth <= 0)
        {
            _manager.Spawn();
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void RefreshHealthBar()
    {
        float healthRatio = (float)_currHealth / (float)maxHealth;
        _UIHealthBar.localScale =Vector3.Lerp(_UIHealthBar.localScale, new Vector3(healthRatio, 1, 1), Time.deltaTime * 8f);
    }
}
