using UnityEngine;
using System.Collections;

public class EnemyBullets : MonoBehaviour 
{
	private ParticleSystem pSystem;

	private bool colourUp = true;
	private Material material;
	private float flashSpeed = 1600;

	// Use this for initialization
	void Awake () 
	{
		pSystem = GetComponent<ParticleSystem>();
		material = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () 
	{
		Flash ();
	}

	public void ShootBullet(Vector2 position, Vector2 velocity)
	{
		pSystem.Emit(position, velocity, pSystem.startSize, pSystem.startLifetime, pSystem.startColor);
	}

	private void Flash()
	{
		Color32 colour = material.GetColor("_TintColor");
		if(colourUp) colour.g += (byte)(flashSpeed * Time.deltaTime);
		else colour.g -= (byte)(flashSpeed * Time.deltaTime);
		
		if(colour.g >= 250) colourUp = false;
		else if(colour.g <= 50) colourUp = true;
		
		material.SetColor("_TintColor", colour);
	}
}
