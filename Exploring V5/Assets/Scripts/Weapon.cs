﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region Variables

    public Gun[] loadout;
    public Transform weaponParent;
    private int _currInd;
    private GameObject _currentWeapon;
    public GameObject bulletHolePrefab;
    public LayerMask canBeShot;

    #endregion

    #region MonoBehaviour Callbacks

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(0);
        if (_currentWeapon != null) 
        {
            Aim(Input.GetMouseButton(1));
            if (Input.GetMouseButton(0)) Shoot();
        }
    }

    #endregion

    void Equip(int id)
    {
        if (_currentWeapon != null) Destroy(_currentWeapon);

        _currInd = id;

        GameObject newGun = Instantiate(loadout[id].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        newGun.transform.localPosition = Vector3.zero;
        newGun.transform.localEulerAngles = Vector3.zero;
        _currentWeapon = newGun;
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

    void Shoot()
    {
        Transform spawn = transform.Find("Cameras/FPS Cam");
        RaycastHit hit = new RaycastHit();
        if(Physics.Raycast(spawn.position, spawn.forward, out hit, 1000f, canBeShot))
        {
            GameObject newHole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject;
            newHole.transform.LookAt(hit.point + hit.normal);
            Destroy(newHole, 5f);
        }
    }
}
