using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Enum for easy request of weapon by name (without using strings)
// Simply adding to this enum will give you a spot to add the prefab in the weapon manger inspector
public enum WeaponName
{
	PISTOL,
	MACHINE_GUN,
	CHARGE_CANNON,
	
	NUMBER_OF_WEAPONS, // by having this as the last element the editor can auto-calculate size of array
	NONE // because enemies might need to specify that their starting weapon is NONE
		// note NONE comes after NUMBER_OF_WEAPONS as we don't want it included as a weapon
}



// Manages creation and destruction of weapons
// Ensures there is always a weapon available
// Later could act as a weapon pool so not instantiating and destroying weapons over and over
public class WeaponManager : MonoBehaviour 
{
	public Weapon[] weaponPrefabs; // ref to prefabs to instantiate

	List<Weapon> _weaponInstances = new List<Weapon>(); // actual instances of those prefabs
	public int weaponCount { get { return _weaponInstances.Count; } } // total number of existing weapons


	public Weapon GetWeapon(WeaponName weaponName)
	{
		int i = (int)weaponName;
		GameObject weaponInst = (GameObject)Instantiate(weaponPrefabs[i].gameObject);
		Weapon newWeapon = weaponInst.GetComponent<Weapon>();
		_weaponInstances.Add(newWeapon);
		return newWeapon;
	}

	public void DestroyWeapon(Weapon weapon)
	{
		_weaponInstances.Remove(weapon);
		Destroy (weapon.gameObject);
	}


	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		MiniProfiler.AddMessage("Weapons " + weaponCount);
	}
	



}
