using UnityEngine;
using System.Collections;

public class SpriteBullet : MonoBehaviour 
{

	BulletTester _daddy;

	public void SetParent(BulletTester daddy)
	{
		_daddy = daddy;
	}


	// Use this for initialization
	void Start ()
	{
		GetComponent<Rigidbody>().velocity = new Vector3(3, 0, 0);
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnTriggerStay(Collider other)
	{
		_daddy._numSprites -= 1;
		gameObject.SetActive(false);
	}
}
