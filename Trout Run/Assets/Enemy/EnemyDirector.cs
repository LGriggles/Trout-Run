using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//! Enemy director controls the spawning and AI of enemies in a given section of the level, defined by it's boundary collider
// It is also used to inform EnemyManager how many enemies to pool for level. Could use similar method to pool weapons as well, 
// although less foolproof as enemies can respawn when their old weapons may still exist (so bear that in mind)
public class EnemyDirector : MonoBehaviour, IEnemyTaskGiver
{
    //List<EnemyBehaviour> _enemyBehavs; // shud be enemies
    BoxCollider2D _boundary;
    EnemyManager _enemyManager;

    EnemySpawner _spawner;
    //List<EnemyBehaviour> _behaviours;

    float _spawnTimer = 0;


    // Stuff very specific to this director so will be sent off to children or controller class or some shit
    HashSet<Enemy> _runningAtPlayer = new HashSet<Enemy>(); // enemies running at player like a loon
    struct Sniper { public Enemy enemy; public Vector2 pos; } // snipers
    Sniper[] _snipers = new Sniper[]
    {
        new Sniper() { enemy = null, pos = new Vector2(-5.25f, 7.2f) },
        new Sniper() { enemy = null, pos = new Vector2(7.97f, 7.2f) },
    };
    


    // So could be virtual or whatnot for a smooth outcome
    public void BuildPools()
    {
        _spawner = new EnemySpawner(new EnemySpawner.EnemyPool[]
        {
            new EnemySpawner.EnemyPool() { type = Enemy.Type.DRONE, weapon = WeaponName.NONE, count = 3},
            new EnemySpawner.EnemyPool() { type = Enemy.Type.DRONE, weapon = WeaponName.PISTOL, count = 1},
            new EnemySpawner.EnemyPool() { type = Enemy.Type.DRONE, weapon = WeaponName.MACHINE_GUN, count = 1},
        });

        
    }


    public int MaxPooledEnemies(Enemy.Type enemyType)
    {
        return _spawner.MaxPooledEnemies(enemyType);
    }




    void Awake()
    {
        _boundary = GetComponent<BoxCollider2D>();
        _enemyManager = FindObjectOfType<EnemyManager>();
    }

	// Use this for initialization
	void Start ()
    {
        /*
        FindContainedEnemyBehavs();

        for (int i = 0; i < _enemyBehavs.Count; ++i)
        {
            _enemyBehavs[i].MoveToPosition(new Vector2(9.3f, 7.1f));
        }
         * */

        /*
        _behaviours = new List<EnemyBehaviour>();
        for (int i = 0; i < _spawner.TotalEnemies; ++i)
        {
            EnemyBehaviour behave = new EnemyBehaviour();
            behave.SetTasks(new ETChargeAtPlayer(Random.Range(3, 7)));
            _behaviours.Add(behave);
        }
         * */
	}

    // Update is called once per frame
    void Update() 
    {
        _spawnTimer += Time.deltaTime;

        if (_spawnTimer >= 0.5f && _spawner.CanSpawn)
        {
            Enemy theNewGuy = _spawner.Spawn(new Vector2(5, 12));
            AssignBehaviour(theNewGuy);

            _spawnTimer = 0.0f;
        }
	}


    // Returns index in snipers array of enemy, or -1 if he's not there
    int EnemyIdxInSnipers(Enemy enemy)
    {
        for (int i = 0; i < _snipers.Length; i++)
        {
            if (_snipers[i].enemy == enemy) return i;
        }
        return -1;
    }

    // Returns next index of free sniper spot, or -1 if fully booked
    int GetNextFreeSniperIdx()
    {
        for (int i = 0; i < _snipers.Length; ++i)
        {
            if (_snipers[i].enemy == null) return i;
        }
        return -1;
    }
            

    // Task state changes event
    TaskStateChangeEventHandler IEnemyTaskGiver.EventHandler { get { return OnTaskStateChanged; } }
    void OnTaskStateChanged(object sender, TaskStateChangedEventArgs e)
    {
        Enemy enemy = sender as Enemy;

        switch (e.NewState)
        {
            case EnemyTask.State.TASK_CHANGING:
                int sniperIndex = EnemyIdxInSnipers(enemy);
                if (sniperIndex != -1)
                {
                    _snipers[sniperIndex].enemy = null;
                }
                else if (_runningAtPlayer.Contains(enemy))
                {
                    _runningAtPlayer.Remove(enemy);
                }

                break;

            case EnemyTask.State.COMPLETE:
                enemy.GiveTask(new ETDoNothing(), this);
                break;
        }

        Debug.Log("Event says " + e.NewState.ToString());
    }



    void AssignBehaviour(Enemy enemy)
    {
        if (_runningAtPlayer.Count < 2)
        {
            enemy.GiveTask(new ETChargeAtPlayer(Random.Range(3, 7)), this);
            _runningAtPlayer.Add(enemy);
            return;
        }
        
        
        if (enemy.weapon != null && enemy.weapon.WeaponType == Weapon.Type.PROJECTILE)
        {
            int idx = GetNextFreeSniperIdx();
            if (idx != -1)
            {
                _snipers[idx].enemy = enemy;
                enemy.GiveTask(new ETWalkTo(_snipers[idx].pos), this);
                return;
            }
        }


        enemy.GiveTask(new ETDoNothing(), this);
    }

    


   

    /*
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
     * */
}
