using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    GameObject target;
    private PlayerController _player;
    private Mover _playerMover;
    private Transform _playerTransform;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float damping = 0.8f;
    private bool _dampingOn = false;

    public Rect constraints;

    void Awake()
    {
        if (_player == null)
        {
            target = GameObject.FindGameObjectWithTag("Player");
            _player = target.GetComponent<PlayerController>();
        }
        _playerMover = _player.GetComponent<Mover>();
        _playerTransform = _player.transform;
    }


    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 newPos = _playerTransform.position + offset;



        if (!_playerMover.isGrounded)
        {
            _dampingOn = true;
            newPos.y = Mathf.Lerp(transform.position.y, newPos.y, 0.5f * Time.deltaTime);

        }
        else if (_dampingOn)
        {
            if (Mathf.Abs(transform.position.y - newPos.y) <= 0.01f) _dampingOn = false;

            newPos.y = Mathf.Lerp(transform.position.y, newPos.y, damping * Time.deltaTime);

        }



        newPos.x = Mathf.Clamp(newPos.x, constraints.xMin, constraints.xMax);
        newPos.y = Mathf.Clamp(newPos.y, constraints.yMin, constraints.yMax);
        transform.position = newPos;
    }
}


