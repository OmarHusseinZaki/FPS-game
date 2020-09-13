using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _speed = 300f;
    private Rigidbody _rig;
    // Start is called before the first frame update
    void Start()
    {
        Camera.main.enabled = false;
        _rig = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float hMove = Input.GetAxisRaw("Horizontal");
        float vMove = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(hMove, 0, vMove);
        direction.Normalize();

        _rig.velocity = transform.TransformDirection(direction) * _speed * Time.fixedDeltaTime;
    }
}
