using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeinParticles : MonoBehaviour
{
	public int bulletCount;
	private Mesh _mesh;
	public Material mat;
	public Texture2D tex;
	private List<Bullet> _bullets = new List<Bullet>();
	public float speed = 5;
	public float size = 1;
	public float lifetime = 5;
	private float _halfSize;

	public int numParts { get { return _bullets.Count; } }

	class Bullet
	{
		public Vector3 _pos;
		private Vector3 _vel;
		private float _time = 0;
		public Bullet(Vector3 pos, Vector3 vel)
		{
			_pos = pos;
			_vel = vel;
		}

		public void Update()
		{ 
			_pos += _vel * Time.deltaTime;
			_time += Time.deltaTime;
		}

		public float timeAlive { get { return _time; } }
	}


	// Use this for initialization
	void Awake () 
	{
		_halfSize = size * 0.5f;

		_mesh = new Mesh();
		Vector3[] verts = new Vector3[4] { new Vector3(-_halfSize, _halfSize, 0), new Vector3(_halfSize, _halfSize, 0), new Vector3(-_halfSize, -_halfSize, 0), new Vector3(_halfSize, -_halfSize, 0) };
		Vector2[] uv = new Vector2[4] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0) };
		int[] tris = new int[6] { 0, 1, 2, 3, 2, 1 };
		_mesh.vertices = verts;
		_mesh.uv = uv;
		_mesh.triangles = tris;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			if(bulletCount > 0)
			{
				float inc = 90/bulletCount;

				for(int i = 0; i < bulletCount; i++)
				{
					float ang = Mathf.Deg2Rad * (-45 + (inc * i));
					Emit(transform.position, new Vector3(Mathf.Cos (ang), Mathf.Sin (ang), 0) * speed);
				}
			}
		}



		UpdateBullets();
	}

	void Emit(Vector3 pos, Vector3 vel)
	{
		Bullet bullet = new Bullet(pos, vel);
		_bullets.Add(bullet);
	}

	void UpdateBullets()
	{
		for(int i = _bullets.Count -1; i >= 0; i--)
		{
			_bullets[i].Update();
			Graphics.DrawMesh(_mesh, _bullets[i]._pos, Quaternion.Euler(0, 0, 0), mat, 0);

			if(_bullets[i].timeAlive >= lifetime) _bullets.RemoveAt(i);
			else if(Physics.CheckSphere(_bullets[i]._pos, _halfSize)) _bullets.RemoveAt(i);
		}
	}






}
