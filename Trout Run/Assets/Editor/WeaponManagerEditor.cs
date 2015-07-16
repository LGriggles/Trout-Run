using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[CustomEditor(typeof(WeaponManager))]
public class WeaponManagerEditor : Editor 
{
	WeaponManager _weaponMan;

	void OnEnable()
	{
		_weaponMan = (WeaponManager)target;
	}


	public override void OnInspectorGUI()
	{
		int weaponCount = (int)WeaponName.NUMBER_OF_WEAPONS;

		// Ensure weapon array is not null and is equal in size to weaponCount
		if(_weaponMan.weaponPrefabs == null)
		{
			_weaponMan.weaponPrefabs = new Weapon[weaponCount];
		}
		else if(_weaponMan.weaponPrefabs.Length < weaponCount)
		{
			Weapon[] newWeapons = new Weapon[weaponCount];
			_weaponMan.weaponPrefabs.CopyTo(newWeapons, 0);
			_weaponMan.weaponPrefabs = newWeapons;
		}
		else if(_weaponMan.weaponPrefabs.Length > weaponCount)
		{
			List<Weapon> newWeapons = new List<Weapon>(_weaponMan.weaponPrefabs);
			newWeapons.RemoveRange(weaponCount, newWeapons.Count - weaponCount);
			_weaponMan.weaponPrefabs = newWeapons.ToArray();
		}


		// For each weapon in enum weapon names, provide a space to assign a prefab
		for(int i = 0; i < weaponCount; i++)
		{
			WeaponName name = (WeaponName)i;
			EditorGUILayout.PrefixLabel(name.ToString());
			_weaponMan.weaponPrefabs[i] = (Weapon)EditorGUILayout.ObjectField(_weaponMan.weaponPrefabs[i], typeof(Weapon), false);
		}



		// Ensure target changes if any changes in GUI
		if (GUI.changed)
		{
			EditorUtility.SetDirty (target);
		}
	}




}
