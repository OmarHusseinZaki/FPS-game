using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit.Forms;

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
    private bool _isReloading;
    public bool isAim;

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        foreach (Gun a in loadout) a.Initialize();
        photonView.RPC("Equip", RpcTarget.All, 0);
    }

    void Update()
    {
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1)) photonView.RPC("Equip", RpcTarget.All, 0);
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2)) photonView.RPC("Equip", RpcTarget.All, 1);
        if (_currentWeapon != null) 
        {
            if (photonView.IsMine) 
            {
                Aim(Input.GetMouseButton(1));

                if (loadout[_currInd].burst != 1)
                {
                    if (Input.GetMouseButtonDown(0) && _currCoolDown <= 0)
                    {
                        if (loadout[_currInd].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                        else StartCoroutine(Reload(loadout[_currInd].reloadTime));
                    }
                }
                else
                {
                    if (Input.GetMouseButton(0) && _currCoolDown <= 0)
                    {
                        if (loadout[_currInd].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                        else StartCoroutine(Reload(loadout[_currInd].reloadTime));
                    }
                }


                if(Input.GetKeyDown(KeyCode.R)) StartCoroutine(Reload(loadout[_currInd].reloadTime));

                // CoolDown
                if (_currCoolDown > 0) _currCoolDown -= Time.deltaTime;
            }

            // Weapon Elasticity
            _currentWeapon.transform.localPosition = Vector3.Lerp(_currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);

           
        }
    }

    #endregion
    [PunRPC]
    void Equip(int id)
    {
        if (_currentWeapon != null) 
        {
            if(_isReloading) StopCoroutine("Reload");
            Destroy(_currentWeapon);
        }

        _currInd = id;

        GameObject newGun = Instantiate(loadout[id].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        newGun.transform.localPosition = Vector3.zero;
        newGun.transform.localEulerAngles = Vector3.zero;
        _currentWeapon = newGun;
        _currentWeapon.GetComponent<Sway>().isMine = photonView.IsMine;
    }

    void Aim(bool isAiming)
    {
        isAim = isAiming;
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
                    hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[_currInd].Damage);
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
        GetComponent<Player>().TakeDamage(damage);
    }

    IEnumerator Reload(float wait)
    {
        _isReloading = true;
        _currentWeapon.SetActive(false);

        yield return new WaitForSeconds(wait);
        
        _currentWeapon.SetActive(true);
        loadout[_currInd].Reload();
        _isReloading = false;
    }

    public void RefreshAmmo(Text text)
    {
        int clip = loadout[_currInd].GetClip();
        int stash = loadout[_currInd].GetStash();
        text.text = clip.ToString() + " / " + stash.ToString();
    }
}
