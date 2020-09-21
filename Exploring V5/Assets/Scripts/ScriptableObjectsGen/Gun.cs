using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
public class Gun : ScriptableObject
{
    public string gunName;
    public int Damage;
    public int ammo;
    public int clipSize;
    public float reloadTime;
    public float firerate;
    public float bloom;
    public float recoil;
    public float kickback;
    public float aimSpeed;
    public GameObject prefab;

    private int _stash; // Current ammo
    private int _clip; // Current clip


    public void Initialize()
    {
        _stash = ammo;
        _clip = clipSize;
    }

    public bool FireBullet()
    {
        if (_clip > 0)
        {
            _clip -= 1;
            return true;
        }
        else return false;
    }

    public void Reload()
    {
        _stash += _clip;
        _clip = Mathf.Min(clipSize, _stash);
        _stash -= _clip;
    }

    public int GetStash()
    {
        return _stash;
    }

    public int GetClip()
    {
        return _clip;
    }
}
