using UnityEngine;
using System.Collections;

public class Ticket : Pickup {

    SpriteRenderer _myRenderer;
    bool _bigTicket = false; //false is 1 ticket, true is 5
    Animator _myAnimator;

	void Start () {
	}


	void OnEnable () {
        _myAnimator = GetComponent<Animator>();
        _myRenderer = GetComponent<SpriteRenderer>();
        StopCoroutine("CountdownToDeath");
        StopCoroutine("Death");
        _myRenderer.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
        RandomTicket();
        StartCoroutine(CountdownToDeath(5.0f + (Random.value * 2)));
    }

    private void RandomTicket() // Will the ticket be a 1 token ticket, or a 5 token ticket??
    {
        if (Random.value > 0.95f)
        {
            _bigTicket = true;
            _myAnimator.Play("FiveTicket");
        }
        else
        {
            _bigTicket = false;
            _myAnimator.Play("OneTicket");
        }
    }

    private IEnumerator CountdownToDeath (float deathClock) //sounds pretty sinister, don't it
    {
        yield return new WaitForSeconds (deathClock);
        StartCoroutine(Death());
    }

    private IEnumerator Death () //When the ticket has been in play for too long, it flickers and fades away
    {
            for (int i = 0; i < 16; i++)
            {
                _myRenderer.color = new Color (1.0f, 1.0f, 1.0f, 0.4f);
                yield return new WaitForSeconds(0.16f);
                _myRenderer.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
                yield return new WaitForSeconds(0.16f);
            }
        gameObject.SetActive(false);
    }

    public override void Collected (PlayerController pc) {
        gameObject.SetActive(false);
    }
}
