using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region Variables

    public Gun[] loadout;
    public Transform weaponParent;
    private int _currInd;
    private GameObject _currentWeapon;

    #endregion

    #region MonoBehaviour Callbacks

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(0);
        if (_currentWeapon != null) Aim(Input.GetMouseButton(1));
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
}
