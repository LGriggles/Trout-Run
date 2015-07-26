using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullets : MonoBehaviour
{
    public WeaponHolder _owner;
    Weapon _myWeapon; // ref to weapon that shoots these bullets
    int _sceneMask; // layer mask for solid scenery
    int _targetMask; // layer mask for thing we want to shoot (player for enemy and enemy for player)
    int _shootableMask; // layer mask for 'shootable' objects that are neither movers nor traditional scenery

    public int maxHitsPerBullet = 3; // max number of collisions we bother checking per particle

    List<GameObject> _bulletPool;
    public Bullet bulletBase;

    void Awake()
    {
        _myWeapon = GetComponentInParent<Weapon>();
        _sceneMask = 1 << LayerMask.NameToLayer("SolidScenery");
        _shootableMask = 1 << LayerMask.NameToLayer("ShootableObject");
    }

    public void InstantiatePool(int size, GameObject prefab)
    {
        _bulletPool = new List<GameObject>();
        for (int i = 0; i < 200; i++)
        {
            GameObject newObj = (GameObject)Instantiate(prefab);
            newObj.SetActive(false);
            _bulletPool.Add(newObj);
        }
    }

    public void Shoot(Vector2 position, Vector2 velocity, WeaponHolder owner)
    {
        GameObject newBullet = GetObject();
        newBullet.GetComponent<Bullet>().Shot(this, velocity, owner);
        newBullet.transform.position = position;
    }

    public GameObject GetObject()
    {
        if (_bulletPool.Count > 0)
        {
            GameObject obj = _bulletPool[0];
            obj.SetActive(true);
            _bulletPool.RemoveAt(0);
            return obj;
        }
        return null;
    }

    public void DestroyObject(GameObject obj)
    {
        _bulletPool.Add(obj);
        obj.SetActive(false);
    }
}
