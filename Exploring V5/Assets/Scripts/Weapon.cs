using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region Variables

    public Gun[] loadout;
    public Transform weaponParent;
    private GameObject _currentWeapon;

    #endregion

    #region MonoBehaviour Callbacks

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(0);
    }

    #endregion

    void Equip(int id)
    {
        if (_currentWeapon != null) Destroy(_currentWeapon);

        GameObject newGun = Instantiate(loadout[id].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        newGun.transform.localPosition = Vector3.zero;
        newGun.transform.localEulerAngles = Vector3.zero;
        _currentWeapon = newGun;
    }
}
