

using UnityEngine;

public class SmallBall : MonoBehaviour
{
	private int ballSpeed;

	private Animator anim;

	private void Start()
	{
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		if (GameManager.Instance.uIManager.gameState == GameState.PLAYING || GameManager.Instance.uIManager.gameState == GameState.MENU)
		{
			base.transform.Rotate(0f, 0f, (float)ballSpeed * Time.deltaTime);
		}
	}

	public void SetBallSpeed(int _ballSpeed)
	{
		ballSpeed = _ballSpeed;
	}

	public void HideBall()
	{
		ballSpeed = 0;
		anim.Play("SmallBallHide");
	}
}
