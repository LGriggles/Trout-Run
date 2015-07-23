using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour 
{
    public Enemy[] enemyPrefabs; // ref to prefabs to instantiate
    private int[] _numEnemies = new int[(int)Enemy.Type.NUM_ENEMIES]; //!< Each int is number of enemies of each type to pool for this scene
    private List<GameObject>[] _instances = new List<GameObject>[(int)Enemy.Type.NUM_ENEMIES];

    void Awake()
    {
        EnemyDirector[] directors = FindObjectsOfType<EnemyDirector>();
        foreach (EnemyDirector director in directors)
        {
            director.BuildPools();

            for (Enemy.Type i = 0; i < Enemy.Type.NUM_ENEMIES; ++i)
            {
                int n = director.MaxPooledEnemies(i);
                if (n > _numEnemies[(int)i]) _numEnemies[(int)i] = n;
            }
        }

        InstantiateEnemies();
    }


    



    void InstantiateEnemies()
    {
        for (Enemy.Type i = 0; i < Enemy.Type.NUM_ENEMIES; ++i)
        {
            _instances[(int)i] = new List<GameObject>();

            for (int j = 0; j < _numEnemies[(int)i]; ++j)
            {
                // Note instantiate to different positions as trying to create loads at same pos can crash it!
                Vector3 instPos = new Vector3(j, -(int)i, -5); // instantiate in nice orderly queue!

                GameObject enemy = (GameObject)Instantiate(enemyPrefabs[(int)i].gameObject, instPos, Quaternion.Euler(0, 0, 0));
                enemy.SetActive(false);
                _instances[(int)i].Add(enemy);
            }
        }
    }


    public Enemy SpawnEnemy(Enemy.Type type, WeaponName weapon, Vector2 pos)
    {
        for (int i = 0; i < _instances[(int)type].Count; i++)
        {
            GameObject go = _instances[(int)type][i];
            if(go.activeSelf == false)
            {
                Enemy returnEnemy = go.GetComponent<Enemy>();

                // Init enemy
                go.SetActive(true);
                if ((int)(weapon) != (int)WeaponName.NUMBER_OF_WEAPONS) returnEnemy.startingWeapon = weapon;
                returnEnemy.Spawn(pos);


                // Tell yo boss
                //EnemyBehaviour behave = _enemies[n].GetComponentInChildren<EnemyBehaviour>();
                //if (behave == null) Debug.Log("But mom...");
                //else _enemyManager.EnemySpawned(behave);

                // Return so doesn't bother trawling through other enemies in array
                return returnEnemy;
            }
        }

        return null;
    }








    /*
    EnemyManager _enemyManager;

	public int maxEnemies = 4; // max number at once - number instantiated controlled by each enemy setup's quantity
	[SerializeField] public EnemySetup[] enemySetups;
	public bool positionsRelative = false; // to centre of screen
	public Vector2[] enemyPositions;

	private bool _spawnEnemies = false;

	private List<Enemy> _enemies;

	private Transform _mainCamera; // needed to work out centre of screen
	private int _enemyCount = 0; // how many enemies currently alive?
	public void EnemyDied() { _enemyCount -= 1; } // called from enemy when it dies
	int _nextEnemy = 0; // next enemy we want to spawn if not already alive (so we get round robin of all enemies)

	private int _layPlayer;

	private int _posCounter = -1; // for keeping track of what is next position

	void Awake()
	{
		_mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
		_layPlayer = LayerMask.NameToLayer("Player");
        _enemyManager = FindObjectOfType<EnemyManager>();
	}

	// Use this for initialization
	void Start () 
	{
		// Set up enemies
		_enemies = new List<Enemy>();

		// Set up each enemy based on grunt settings
		for(int i = 0; i < enemySetups.Length; i++) // for each different setup
		{
			for(int q = 0; q < enemySetups[i].quantity; q++) // for each duplicate of that setup
			{
				// Note instantiate to different positions as trying to create loads at same pos can crash it!
				Vector3 instPos = transform.position;
				instPos += new Vector3(q, -i, -5); // instantiate in nice orderly queue!

				GameObject enemy = (GameObject)Instantiate(enemySetups[i].enemyPrefab.gameObject, instPos, Quaternion.Euler(0, 0, 0));
				Enemy enemyScript = enemy.GetComponent<Enemy>();

				//if(enemySetups[i].weaponPrefab != null) enemyScript.AddWeapon(enemySetups[i].weaponPrefab);
				if((int)(enemySetups[i].startingWeapon) < (int)WeaponName.NUMBER_OF_WEAPONS) enemyScript.startingWeapon = enemySetups[i].startingWeapon;

				enemy.transform.parent = transform;
				enemy.SetActive(false);
				_enemies.Add(enemyScript);
			}
		}
	}

	void Update()
	{
		if(_spawnEnemies) SpawnEnemies();
		_spawnEnemies = false;

	}

	// Moved creation logic into OnTriggerStay so we can have it only create them if you're in the spawner's catchment area
	void SpawnNextEnemy()
	{
		for(int i = 0; i < _enemies.Count; i++)
		{
			int n = _nextEnemy + i;
			if(n >= _enemies.Count) n = 0;

			if(_enemies[n].gameObject.activeSelf == false)
			{
				// Calc position
				Vector2 pos = NextPosition();
				
				// Calculate direction
				float dir = 1; // right
				if(pos.x  >= _mainCamera.position.x) dir = -1; // left
				
				// Init enemy
				_enemies[n].gameObject.SetActive(true);
				_enemies[n].Spawn(pos, dir);

				// Add to enemy count
				_enemyCount += 1;
				_nextEnemy += 1;
				if(_nextEnemy >= _enemies.Count) _nextEnemy = 0;

                // Tell yo boss
                EnemyBehaviour behave = _enemies[n].GetComponentInChildren<EnemyBehaviour>();
                if (behave == null) Debug.Log("But mom...");
                else _enemyManager.EnemySpawned(behave);

                // Return so doesn't bother trawling through other enemies in array
				return;
			}
		}
	}

	void SpawnEnemies()
	{	
		for(int i = _enemyCount; i < maxEnemies; i++)
		{
			SpawnNextEnemy();
		}

	}

	private Vector2 NextPosition() // advances pos counter and returns the next position
	{
		Vector2 worldPos = Vector2.zero;
		
		if(enemyPositions.Length > 0)
		{
			_posCounter += 1;
			if(_posCounter >= enemyPositions.Length) _posCounter = 0;
			worldPos = enemyPositions[_posCounter];
		}
		
		if(positionsRelative)
		{
			Vector2 playerPos = _mainCamera.position; // camera's pos as a vector2
			worldPos += playerPos; // make relative to camera's position
		}
		
		return worldPos;
	}


	void OnTriggerStay2D(Collider2D other)
	{
		if(other.gameObject.layer == _layPlayer)
		{
			_spawnEnemies = true;
		}
	}


    */

}
