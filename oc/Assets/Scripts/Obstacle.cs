

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
	public Animator aimAnim;

	public Animator waveAnim;

	public GameObject smallBallPrefab;

	public float smallBallDistanceFromCenter = 1.3f;

	private bool waving;

	private Vector2 nextObstaclePosition;

	private List<GameObject> smallBalls = new List<GameObject>();

	private Color color;

	private int id;

	public void SetObstacle(Color _color, int obstacleId)
	{
		aimAnim.gameObject.GetComponent<SpriteRenderer>().color = _color;
		waveAnim.gameObject.GetComponent<SpriteRenderer>().color = _color;
		base.gameObject.GetComponent<SpriteRenderer>().color = _color;
		color = _color;
		id = obstacleId;
		if (obstacleId == 1)
		{
			CreateSmallBalls(10);
		}
		else if (obstacleId > 1)
		{
			if (ScoreManager.Instance.currentScore < 10)
			{
				CreateSmallBalls(UnityEngine.Random.Range(6, 9));
			}
			else if (ScoreManager.Instance.currentScore < 20)
			{
				CreateSmallBalls(UnityEngine.Random.Range(7, 11));
			}
			else if (ScoreManager.Instance.currentScore < 40)
			{
				CreateSmallBalls(UnityEngine.Random.Range(8, 12));
			}
			else if (ScoreManager.Instance.currentScore < 60)
			{
				CreateSmallBalls(UnityEngine.Random.Range(5, 12));
			}
		}
		StartCoroutine(CheckPosition());
	}

	private IEnumerator CheckPosition()
	{
		while (true)
		{
			if (base.transform.position.y < Camera.main.transform.position.y - 8f && GameManager.Instance.uIManager.gameState == GameState.PLAYING)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			yield return new WaitForSecondsRealtime(0.5f);
		}
	}

	public void SetNextObstaclePosition(Vector2 _nextObstaclePosition)
	{
		nextObstaclePosition = _nextObstaclePosition;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!collision.CompareTag("Player"))
		{
			return;
		}
		if (collision.gameObject.GetComponent<SpriteRenderer>().color == color)
		{
			PlayWaveIn();
			ActivateAim();
			for (int i = 0; i < smallBalls.Count; i++)
			{
				if (smallBalls[i] != null)
				{
					smallBalls[i].GetComponent<SmallBall>().HideBall();
				}
			}
		}
		else
		{
            //PlayWaveOut();
            //GameManager.Instance.PlayerDeath();
            StartCoroutine(PauseTime());
            
			
        }
	}
	IEnumerator PauseTime()
	{
        GameManager.Instance.GameOver();
        GameManager.Instance.camObject.GetComponent<CameraFollowTarget>().ShakeCamera();
		yield return new WaitForSeconds(0.5f);
        Time.timeScale = 0.0f;
    }
	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			PlayWaveOut();
			DeActivateAim();
		}
	}

	public void ActivateAim()
	{
		aimAnim.transform.parent.gameObject.SetActive(value: true);
		if (id != 0)
		{
			Vector2 vector = nextObstaclePosition - (Vector2)base.transform.position;
			vector.Normalize();
			float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			aimAnim.transform.parent.gameObject.transform.localEulerAngles = new Vector3(0f, 0f, num - 90f);
		}
	}

	public void DeActivateAim()
	{
		aimAnim.transform.parent.gameObject.SetActive(value: false);
	}

	public void PlayWaveIn()
	{
		if (!waving)
		{
			waving = true;
			waveAnim.Play("WaveIn");
			StartCoroutine(ResetWaving(0.3f));
		}
	}

	public void PlayWaveOut()
	{
		if (!waving)
		{
			waving = true;
			waveAnim.Play("WaveOut");
			StartCoroutine(ResetWaving(0.3f));
		}
	}

	private IEnumerator ResetWaving(float delay)
	{
		yield return new WaitForSeconds(delay);
		waving = false;
	}

	private void CreateSmallBalls(int numOfBalls)
	{
		int num = 0;
		int num2 = 360 / numOfBalls;
		bool flag = false;
		int ballSpeed = (UnityEngine.Random.Range(0, 2) == 1) ? ((ScoreManager.Instance.currentScore >= 20) ? UnityEngine.Random.Range(60, 150) : UnityEngine.Random.Range(45, 120)) : ((ScoreManager.Instance.currentScore >= 20) ? UnityEngine.Random.Range(-150, -60) : UnityEngine.Random.Range(-120, -45));
		for (int i = 0; i < numOfBalls; i++)
		{
			smallBalls.Add(UnityEngine.Object.Instantiate(smallBallPrefab, base.transform));
			smallBalls[i].transform.localEulerAngles = new Vector3(0f, 0f, num);
			smallBalls[i].GetComponent<SmallBall>().SetBallSpeed(ballSpeed);
			num += num2;
			Color lhs = GameManager.Instance.colorTable[Random.Range(0, GameManager.Instance.colorTable.Length)];
			if (lhs == color)
			{
				flag = true;
			}
			if ((!flag && i == numOfBalls - 1) || id == 1)
			{
				lhs = color;
			}
			smallBalls[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = lhs;
		}
	}
}
