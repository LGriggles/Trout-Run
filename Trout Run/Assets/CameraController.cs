using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    GameObject target;
	public PlayerController player;
	private Mover _playerMover;
	private Transform _playerTransform;
	public Vector3 offset = new Vector3(0, 0, -10);
	public float damping = 0.8f;
	private bool _dampingOn = false;

	void Awake()
	{
        if (player == null) {
            target = GameObject.FindGameObjectWithTag("Player");
            player = target.GetComponent<PlayerController>();
        }
		_playerMover = player.GetComponent<Mover>();
		_playerTransform = player.transform;
	}


	// Update is called once per frame
	void LateUpdate () 
	{
		Vector3 newPos = _playerTransform.position + offset;
        if (player._playerIsShooting)
        {
            newPos.x = Mathf.Lerp(transform.position.x, _playerTransform.position.x + (2 * (player.facingDirection)), 8 * Time.deltaTime);
        }

		if(!_playerMover.isGrounded)
		{
			_dampingOn = true;
            
            newPos.y = Mathf.Lerp(transform.position.y, newPos.y, 0.5f * Time.deltaTime);

            newPos.x = Mathf.Lerp(transform.position.x, _playerTransform.position.x + (2 * (player.facingDirection)), 3 * Time.deltaTime);
		}
        else if (_dampingOn)
        {
            if (Mathf.Abs(transform.position.y - newPos.y) <= 0.01f) _dampingOn = false;

            newPos.y = Mathf.Lerp(transform.position.y, newPos.y, damping * Time.deltaTime);

            newPos.x = Mathf.Lerp(transform.position.x, _playerTransform.position.x + (2 * (player.facingDirection)), 1 * Time.deltaTime);
        }
        else
        {

            newPos.x = Mathf.Lerp(transform.position.x, _playerTransform.position.x + (2 * (player.facingDirection)), 1 * Time.deltaTime);
        }
		transform.position = newPos;
	}
}


