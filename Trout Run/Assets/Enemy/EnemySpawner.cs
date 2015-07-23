using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EnemySpawner
{
    //! Linear goes through in order, random just sets to random enemy each spawn, shuffle randomizes order at the start but then spawns linear
    public enum SpawnType { LINEAR, RANDOM, SHUFFLED }
    SpawnType _spawnType;

    EnemyManager _manager;
    int _curPool = 0;
    int _curPoolIdx = 0;
    int _totalEnemies;
    int _spawnCount; //!< Number of enemies currently spawned by this spawner

    int[] _shuffledOrder;


    // David Gettaz 'n' Setterz
    public bool CanSpawn { get { return _spawnCount < _totalEnemies; } }
    public int SpawnCount { get { return _spawnCount; } }
    public int TotalEnemies { get { return _totalEnemies; } }


    public struct EnemyPool
    {
        public Enemy.Type type;
        public WeaponName weapon;
        public int count;
    }
    private EnemyPool[] _enemyPools;

    public EnemySpawner(EnemyPool[] enemyPools)
    {
        _manager = GameObject.FindObjectOfType<EnemyManager>();
        _enemyPools = enemyPools;

        _totalEnemies = 0;
        _spawnCount = 0;
        foreach (EnemyPool pool in _enemyPools) { _totalEnemies += pool.count; }

        //DebugPrint(); //~jus fo debug
        SetSpawnType(SpawnType.LINEAR);

    }

    public void SetSpawnType(SpawnType spawnType)
    {
        _spawnType = spawnType;
        switch (_spawnType)
        {
            case SpawnType.LINEAR:
                _curPool = 0;
                _curPoolIdx = 0;

                break;

            case SpawnType.RANDOM:
                EnemyIndex = Random.Range(0, _totalEnemies);

                break;

            case SpawnType.SHUFFLED:
                BuildShuffle();
                EnemyIndex = _shuffledOrder[0];

                break;

        }

    }

    void DebugPrint()
    {
        Debug.Log("Testing enemy idx set:");
        for (int i = 0; i < _totalEnemies; ++i)
        {
            EnemyIndex = i;
            Debug.Log("Enemy " + i + ": pool = " + _curPool + ", idx = " + _curPoolIdx);
        }


        Debug.Log("Testing enemy idx get:");
        _curPool = 0;
        _curPoolIdx = 0;
        for (int i = 0; i < _totalEnemies; ++i)
        {
            Debug.Log("Pool = " + _curPool + ", idx = " + _curPoolIdx + " = " + EnemyIndex);

            _curPoolIdx++;
            if (_curPoolIdx >= _enemyPools[_curPool].count)
            {
                _curPoolIdx = 0;
                _curPool++;
            }
            if (_curPool >= _enemyPools.Length)
            {
                _curPool = 0;
                _curPoolIdx = 0;
            }
        }
    }


    // This needs testing a lot because math
    int EnemyIndex
    {
        get
        {
            int returnValue = 0;
            for (int i = 0; i < _curPool; ++i)
            {
                returnValue += _enemyPools[i].count;
            }
            return returnValue + _curPoolIdx;
        }
        set
        {
            int count = 0;
            _curPoolIdx = 0;
            _curPool = 0;
            for (int i = 0; i < _enemyPools.Length; ++i)
            {
                int newCount = count + _enemyPools[i].count;
                if (value >= newCount)
                {
                    //then it is at least in the next pool
                    count = newCount;
                }
                else
                {
                    // it is in this pool
                    _curPool = i;
                    _curPoolIdx = value - count;
                    return;
                }
            }
        }
    }




    public int MaxPooledEnemies(Enemy.Type enemyType)
    {
        int n = 0;
        foreach (EnemyPool pool in _enemyPools)
        {
            if (pool.type == enemyType)
            {
                n += pool.count;
            }
        }
        return n;
    }




    void BuildShuffle()
    {
        _shuffledOrder = new int[_totalEnemies];
        for (int i = 0; i < _totalEnemies; ++i) { _shuffledOrder[i] = -1; }

        for (int i = 0; i < _totalEnemies; ++i)
        {
            int idx = Random.Range(0, _totalEnemies);
            bool ok = false;

            while (!ok)
            {
                if (_shuffledOrder[idx] == -1)
                {
                    _shuffledOrder[idx] = i;
                    ok = true;
                }
                else
                {
                    idx += 1;
                    if (idx >= _totalEnemies) idx = 0;
                }
            }
        }
    }


    



    public Enemy Spawn(Vector2 position)
    {
        Enemy returnEnemy = null;
        

        if (_enemyPools[_curPool].count > 0)
        {
            returnEnemy = _manager.SpawnEnemy(_enemyPools[_curPool].type, _enemyPools[_curPool].weapon, position);
            if (returnEnemy == null) return null;
            _spawnCount++;
            _enemyPools[_curPool].count--;
        }
        else
        {
            switch (_spawnType)
            {
                case SpawnType.LINEAR:
                case SpawnType.RANDOM:
                    // try to find enemy pool with enemies left in it
                    for (int i = 0; i < _enemyPools.Length; ++i)
                    {
                        _curPool++;
                        if (_curPool >= _enemyPools.Length) _curPool = 0;

                        if (_enemyPools[_curPool].count > 0)
                        {
                            _curPoolIdx = 0;
                            return Spawn(position);
                        }
                    }
                    return null;


                case SpawnType.SHUFFLED:


                    break;
            }
        }


        // Change pool / idx for next spawn
        switch (_spawnType)
        {
            case SpawnType.LINEAR:
                _curPoolIdx++;
                if (_curPoolIdx >= _enemyPools[_curPool].count)
                {
                    _curPoolIdx = 0;
                    _curPool++;
                }
                if (_curPool >= _enemyPools.Length)
                {
                    _curPool = 0;
                    _curPoolIdx = 0;
                }

                break;

            case SpawnType.RANDOM:
                EnemyIndex = Random.Range(0, _totalEnemies);

                break;

            case SpawnType.SHUFFLED:
                int i = EnemyIndex;
                i++;
                if (i >= _totalEnemies) i = 0;
                EnemyIndex = _shuffledOrder[i];

                break;

        }

        if (returnEnemy != null) returnEnemy.SetSpawner(this);
        return returnEnemy;
    }

    public void EnemyDied(Enemy enemy)
    {
        for (int i = 0; i < _enemyPools.Length; ++i)
        {
            if (_enemyPools[i].type == enemy.EnemyType && _enemyPools[i].weapon == enemy.startingWeapon)
            {
                _enemyPools[i].count++;
                _spawnCount--;
                return;
            }
        }

        // If reached here we're in the shit because no pools match this enemy so how did we spawn him??
    }

}