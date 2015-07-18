using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[CustomEditor(typeof(EnemyManager))]
public class EnemySpawnerEditor : Editor
{
    EnemyManager _manager;

    void OnEnable()
    {
        _manager = (EnemyManager)target;
    }


    public override void OnInspectorGUI()
    {
        int enemyCount = (int)Enemy.Type.NUM_ENEMIES;

        // Ensure weapon array is not null and is equal in size to weaponCount
        if (_manager.enemyPrefabs == null)
        {
            _manager.enemyPrefabs = new Enemy[enemyCount];
        }
        else if (_manager.enemyPrefabs.Length < enemyCount)
        {
            Enemy[] newEnemies = new Enemy[enemyCount];
            _manager.enemyPrefabs.CopyTo(newEnemies, 0);
            _manager.enemyPrefabs = newEnemies;
        }
        else if (_manager.enemyPrefabs.Length > enemyCount)
        {
            List<Enemy> newEnemies = new List<Enemy>(_manager.enemyPrefabs);
            newEnemies.RemoveRange(enemyCount, newEnemies.Count - enemyCount);
            _manager.enemyPrefabs = newEnemies.ToArray();
        }


        // For each weapon in enum weapon names, provide a space to assign a prefab
        for (int i = 0; i < enemyCount; i++)
        {
            Enemy.Type type = (Enemy.Type)i;
            EditorGUILayout.PrefixLabel(type.ToString());
            _manager.enemyPrefabs[i] = (Enemy)EditorGUILayout.ObjectField(_manager.enemyPrefabs[i], typeof(Enemy), false);
        }



        // Ensure target changes if any changes in GUI
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}