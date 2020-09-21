using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Weapon : MonoBehaviourPunCallbacks
{
    #region Variables

    public Gun[] loadout;
    public Transform weaponParent;
    private int _currInd;
    private float _currCoolDown;
    private GameObject _currentWeapon;
    public GameObject bulletHolePrefab;
    public LayerMask canBeShot;

    #endregion

    #region MonoBehaviour Callbacks

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) photonView.RPC("Equip", RpcTarget.All, 0);
        if (_currentWeapon != null) 
        {
            Aim(Input.GetMouseButton(1));
            if (Input.GetMouseButtonDown(0) && _currCoolDown <= 0) photonView.RPC("Shoot",RpcTarget.All);

            // Weapon Elasticity
            _currentWeapon.transform.localPosition = Vector3.Lerp(_currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);

            // CoolDown
            if (_currCoolDown > 0) _currCoolDown -= Time.deltaTime;
        }
    }

    #endregion
    [PunRPC]
    void Equip(int id)
    {
        if (_currentWeapon != null) Destroy(_currentWeapon);

        _currInd = id;

        GameObject newGun = Instantiate(loadout[id].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        newGun.transform.localPosition = Vector3.zero;
        newGun.transform.localEulerAngles = Vector3.zero;
        _currentWeapon = newGun;
        _currentWeapon.GetComponent<Sway>().isMine = photonView.IsMine;
    }

    void Aim(bool isAiming)
    {
        Transform anchor = _currentWeapon.transform.Find("Anchor");
        Transform state_ads = _currentWeapon.transform.Find("States/ADS");
        Transform state_hip = _currentWeapon.transform.Find("States/Hip");
        if (isAiming)
        {
            anchor.position = Vector3.Lerp(anchor.position, state_ads.position, Time.deltaTime * loadout[_currInd].aimSpeed);
        }
        else
        {
            anchor.position = Vector3.Lerp(anchor.position, state_hip.position, Time.deltaTime * loadout[_currInd].aimSpeed);
        }
    }

    [PunRPC]
    void Shoot()
    {
        Transform spawn = transform.Find("Cameras/FPS Cam");
        // Bloom
        Vector3 t_bloom = spawn.position + spawn.forward * 1000f;
        t_bloom += Random.Range(-loadout[_currInd].bloom, loadout[_currInd].bloom) * spawn.up;
        t_bloom += Random.Range(-loadout[_currInd].bloom, loadout[_currInd].bloom) * spawn.right;
        t_bloom -= spawn.position;
        t_bloom.Normalize();


        // Raycast
        RaycastHit hit = new RaycastHit();
        if(Physics.Raycast(spawn.position, t_bloom, out hit, 1000f, canBeShot))
        {
            GameObject newHole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject;
            newHole.transform.LookAt(hit.point + hit.normal);
            Destroy(newHole, 5f);

            if (photonView.IsMine)
            {
                // If shooting another player on network
                if(hit.collider.gameObject.layer ==  11)
                {
                    hit.collider.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[_currInd].Damage);
                }
            }
        }

        // Gun fx
        _currentWeapon.transform.Rotate(-loadout[_currInd].recoil, 0, 0);
        _currentWeapon.transform.position -= _currentWeapon.transform.forward * loadout[_currInd].kickback;

        // CoolDown
        _currCoolDown = loadout[_currInd].firerate;
    }

    [PunRPC]
    void TakeDamage(int damage)
    {
        GetComponent<PlayerMovement>().TakeDamage(damage);
    }
}
