
using UnityEngine;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using System.Collections.Generic;
using UnityEngine.Analytics;
public class GameManager : MonoBehaviour
{
	public UIManager uIManager;

	public ScoreManager scoreManager;

	[Header("Game settings")]
	[Space(5f)]
	public GameObject camObject;

	[Space(5f)]
	public GameObject player;

	[Space(5f)]
	public int playerSpeed;

	[Space(5f)]
	public Color[] colorTable;

	[Space(5f)]
	public GameObject obstaclePrefab;

	[Space(5f)]
	public float yMinDistanceBetweenObstacles = 5f;

	[Space(5f)]
	public float yMaxDistanceBetweenObstacles = 10f;

	[Space(5f)]
	public float maxXDistanceNextObstacle = 5f;

	[Space(5f)]
	public bool readyToShoot;

	private GameObject previousObstacle;

	private GameObject tempObstacle;

	private Color tempColor;

	private int obstacleId;

	public bool movingPlayer;

	private float step;

	private Vector2 flyDestination;

	public static GameManager Instance
	{
		get;
		set;
	}


    public string clickid;
    private StarkAdManager starkAdManager;
    private void Awake()
	{
		Object.DontDestroyOnLoad(this);
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		Physics2D.gravity = new Vector2(0f, 0f);
		Application.targetFrameRate = 30;
		step = (float)playerSpeed * Time.deltaTime;
		CreateScene();
	}

	private void Update()
	{
		if (uIManager.gameState == GameState.PLAYING && Input.GetMouseButtonDown(0))
		{
			if (!uIManager.IsButton() && readyToShoot && !movingPlayer)
			{
				ShotBall();
			}
		}
		else if (uIManager.gameState == GameState.PLAYING && movingPlayer)
		{
			player.transform.position = Vector2.MoveTowards(player.transform.position, flyDestination, step);
			if (Vector2.Distance(player.transform.position, flyDestination) < 0.001f)
			{
				movingPlayer = false;
				readyToShoot = true;
				flyDestination = tempObstacle.transform.position;
			}
		}
		else if (uIManager.gameState == GameState.PLAYING && Input.GetMouseButtonUp(0))
		{
			readyToShoot = true;
		}
	}

	public void CreateScene()
	{
		ResetPlayerAnimation();
		obstacleId = 0;
		tempColor = colorTable[Random.Range(0, colorTable.Length)];
		player.GetComponent<SpriteRenderer>().color = tempColor;
		previousObstacle = UnityEngine.Object.Instantiate(obstaclePrefab);
		previousObstacle.transform.position = new Vector2(0f, -3f);
		tempObstacle = UnityEngine.Object.Instantiate(obstaclePrefab);
		tempObstacle.transform.position = new Vector2(0f, 3f);
		flyDestination = tempObstacle.transform.position;
		previousObstacle.GetComponent<Obstacle>().SetObstacle(tempColor, obstacleId);
		previousObstacle.GetComponent<Obstacle>().SetNextObstaclePosition(tempObstacle.transform.position);
		obstacleId++;
		tempColor = colorTable[Random.Range(0, colorTable.Length)];
		tempObstacle.GetComponent<Obstacle>().SetObstacle(tempColor, obstacleId);
		camObject.transform.position = new Vector3(0f, 0f, -10f);
		player.transform.position = previousObstacle.transform.position;
		readyToShoot = true;
	}

	public void ShotBall()
	{
		readyToShoot = false;
		movingPlayer = true;
		CreateNextObstacle();
		camObject.GetComponent<CameraFollowTarget>().EnableDisableFollow(status: true);
	}

	private void CreateNextObstacle()
	{
		obstacleId++;
		tempColor = colorTable[Random.Range(0, colorTable.Length)];
		float num = UnityEngine.Random.Range(yMinDistanceBetweenObstacles, yMaxDistanceBetweenObstacles);
		previousObstacle = UnityEngine.Object.Instantiate(obstaclePrefab);
		previousObstacle.transform.position = new Vector2(tempObstacle.transform.position.x + UnityEngine.Random.Range(0f - maxXDistanceNextObstacle, maxXDistanceNextObstacle), tempObstacle.transform.position.y + num);
		previousObstacle.GetComponent<Obstacle>().SetObstacle(tempColor, obstacleId);
		tempObstacle.GetComponent<Obstacle>().SetNextObstaclePosition(previousObstacle.transform.position);
		tempObstacle = previousObstacle;
	}

	public void PlayerDeath()
	{
		camObject.GetComponent<CameraFollowTarget>().ShakeCamera();
		player.GetComponent<Player>().PlayGameOver();
	}

	public void ResetPlayerAnimation()
	{
		player.GetComponent<Player>().ResetPlayer();
	}

	public void RestartGame()
	{
		Time.timeScale= 1.0f;
		if (uIManager.gameState == GameState.PAUSED)
		{
			Time.timeScale = 1f;
		}
		ClearScene();
		CreateScene();
		readyToShoot = false;
		movingPlayer = false;
		scoreManager.ResetCurrentScore();
		uIManager.ShowGameplay();
		camObject.GetComponent<CameraFollowTarget>().EnableDisableFollow(status: false);
	}

	public void ClearScene()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Obstacle");
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.Destroy(array[i]);
		}
	}

	public void GameOver()
	{
		if (uIManager.gameState == GameState.PLAYING)
		{
			movingPlayer = false;
			AudioManager.Instance.PlayEffects(AudioManager.Instance.gameOver);
			uIManager.ShowGameOver();
			scoreManager.UpdateScoreGameover();
            ShowInterstitialAd("1lcaf5895d5l1293dc",
            () => {
                Debug.LogError("--插屏广告完成--");

            },
            (it, str) => {
                Debug.LogError("Error->" + str);
            });
        }
	}
	public void ContinueGame()
	{
        ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {
                    if (uIManager.gameState == GameState.GAMEOVER)
                    {
                        Time.timeScale = 1;
                        movingPlayer = true;
                        uIManager.HideGameOver();
                    }



                    clickid = "";
                    getClickid();
                    apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);


                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
        
		
	}

    public void getClickid()
    {
        var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
        if (launchOpt.Query != null)
        {
            foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                if (kv.Value != null)
                {
                    Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                    if (kv.Key.ToString() == "clickid")
                    {
                        clickid = kv.Value.ToString();
                    }
                }
                else
                {
                    Debug.Log(kv.Key + "<-参数-> " + "null ");
                }
        }
    }

    public void apiSend(string eventname, string clickid)
    {
        TTRequest.InnerOptions options = new TTRequest.InnerOptions();
        options.Header["content-type"] = "application/json";
        options.Method = "POST";

        JsonData data1 = new JsonData();

        data1["event_type"] = eventname;
        data1["context"] = new JsonData();
        data1["context"]["ad"] = new JsonData();
        data1["context"]["ad"]["callback"] = clickid;

        Debug.Log("<-data1-> " + data1.ToJson());

        options.Data = data1.ToJson();

        TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
           response => { Debug.Log(response); },
           response => { Debug.Log(response); });
    }


    /// <summary>
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="closeCallBack"></param>
    /// <param name="errorCallBack"></param>
    public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
        }
    }

    /// <summary>
    /// 播放插屏广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="errorCallBack"></param>
    /// <param name="closeCallBack"></param>
    public void ShowInterstitialAd(string adId, System.Action closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            var mInterstitialAd = starkAdManager.CreateInterstitialAd(adId, errorCallBack, closeCallBack);
            mInterstitialAd.Load();
            mInterstitialAd.Show();
        }
    }
}
