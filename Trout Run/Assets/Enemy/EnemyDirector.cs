using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//! Enemy director controls the spawning and AI of enemies in a given section of the level, defined by it's boundary collider
// It is also used to inform EnemyManager how many enemies to pool for level. Could use similar method to pool weapons as well, 
// although less foolproof as enemies can respawn when their old weapons may still exist (so bear that in mind)
public class EnemyDirector : MonoBehaviour 
{
    List<EnemyBehaviour> _enemyBehavs;
    BoxCollider2D _boundary;
    EnemyManager _enemyManager;

    public struct EnemyPool
    {
        public Enemy.Type type;
        public WeaponName weapon;
        public int count;
    }
    EnemyPool[] _enemyPools;
    int[] _numEnemies;

    float _spawnTimer = 0;


    // So could be virtual or whatnot for a smooth outcome
    public void BuildPools()
    {
        _enemyPools = new EnemyPool[]
        {
            new EnemyPool() { type = Enemy.Type.DRONE, weapon = WeaponName.NONE, count = 3},
            new EnemyPool() { type = Enemy.Type.DRONE, weapon = WeaponName.PISTOL, count = 1},
            new EnemyPool() { type = Enemy.Type.DRONE, weapon = WeaponName.MACHINE_GUN, count = 1},
        };

        _numEnemies = new int[_enemyPools.Length];
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




    void Awake()
    {
        _boundary = GetComponent<BoxCollider2D>();
        _enemyManager = FindObjectOfType<EnemyManager>();
    }

	// Use this for initialization
	void Start ()
    {
        FindContainedEnemyBehavs();

        for (int i = 0; i < _enemyBehavs.Count; ++i)
        {
            _enemyBehavs[i].MoveToPosition(new Vector2(9.3f, 7.1f));
        }
	
	}

    // Update is called once per frame
    void Update() 
    {
        _spawnTimer += Time.deltaTime;

        if (_spawnTimer > 0.5f && _numEnemies[0] < _enemyPools[0].count)
        {
            Enemy theNewGuy = _enemyManager.SpawnEnemy(Enemy.Type.DRONE, WeaponName.PISTOL, new Vector2(5, 12));
            _numEnemies[0] += 1;
            theNewGuy.GetComponent<EnemyBehaviour>().ChargeAtPlayer();
            _spawnTimer = 0;
        }
	}


   


    //! Repopulate the enemy behaviour list with all behaviours of all enemies in the game. Slow so only do at start to determine existing enemies (populate dynamically with in-game created enemies)
    void FindAllEnemyBehavs()
    {
        _enemyBehavs = new List<EnemyBehaviour>(FindObjectsOfType<EnemyBehaviour>());
    }

    //! Repopulate the enemy behaviour list with all behaviours of all enemies within boundary. Slow so only do at start to determine existing enemies (populate dynamically with in-game created enemies)
    void FindContainedEnemyBehavs()
    {
        _enemyBehavs = new List<EnemyBehaviour>();
        Collider2D[] colliders = Physics2D.OverlapAreaAll(_boundary.bounds.min, _boundary.bounds.max);


        foreach (Collider2D c in colliders)
        {
            EnemyBehaviour b = c.GetComponent<EnemyBehaviour>();
            if (b != null && !_enemyBehavs.Contains(b))
            {
                _enemyBehavs.Add(b);
            }
        }


    }







}
