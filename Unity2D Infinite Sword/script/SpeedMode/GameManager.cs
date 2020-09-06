using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    public static GameManager GM;

    //balance
    private const float MAX_TIME = 100f; //시간의 최대치
    private const float BONUS_TIME_VALUE = 15f; //입력에 성공했을 때 증가하는 시간
    private const float MIN_TIME_SPEED = 20f; //게임속도 - 시작값 (단위는 1초동안 감소하는 시간의 양)
    private const float MAX_TIME_SPEED = 60f; //게임속도 - 최댓값
    private const float INCREASE_TIME_SPEED = 1f; //term마다 증가하는 게임속도
    private const float TERM_TIME_SPEED = 2f; //게임속도가 증가하는 주기 (단위 초)

    /* 고블린 간의 거리 간격이 2이고 속도가 10이므로 이론상 1초에 최대 5마리의 적을 처치할 수 있음
     * 게임속도가 60f까지 올라가고 입력에 성공했을 때 증가하는 시간이 15f이므로
     * 초당 4번 이상 입력에 성공할 경우 게임을 무한히 진행할 수 있음
     * 게임속도가 최대까지 증가하는데 1분 20초가 걸림 */

    private const float RED_GOBLINE_RATE = 0.35f; //전체 enemy 중 빨간 고블린의 비율
    private const float MIN_ATTACK_GOBLINE_RATE = 0.05f; //초록 고블린 중 공격 고블린의 비율 - 시작값
    private const float MAX_ATTACK_GOBLINE_RATE = 0.2f; //초록 고블린 중 공격 고블린의 비율 - 최댓값
    private const float INCREASE_ATTACK_GOBLINE_RATE = 0.025f; //term마다 증가하는 공격 고블린의 비율
    private const float TERM_ATTACK_GOBLINE_RATE = 10f; //공격 고블린의 비율이 증가하는 주기 (단위 초)
    //공격 고블린의 비율이 최대까지 증가하는데 1분이 걸림

    //position
    public static Vector3 createPos = new Vector3(12f, -3.6f, 0);
    public static Vector3 battlePos = new Vector3(-7f, -3.6f, 0);

    //Timer
    public static Slider time;
    public Slider timeWrapper;
    private float timeSpeed;
    private float timeCount;
    private float rateCount;

    //score
    public static int score;
    public static int bestScore;

    //draw arrow
    public static int expectScore;
    public static bool isArrowDrawed;

    //other value
    public static bool isStart;
    private bool scoreUpdate;
    public static float AttackGoblinRate;
    public static float RedGoblinRate;

    //GameOver
    public GameObject notice;
    public GameObject scoreBoard;
    public Text nowScore;
    public Text scoreText;
    public Text bestScoreText;
    public Text nowBestScoreText;

    //Guide UI
    public GameObject guide;

    void Awake()
    {
        GM = this;
        time = timeWrapper;
        bestScore = PlayerPrefs.GetInt("SpeedBestScore", 0);
    }
	
    void Start()
    {
        notice.SetActive(false);
        GM.nowScore.text = score.ToString();
        GM.nowBestScoreText.text = bestScore.ToString();
        StartCoroutine("StartStage");
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if(Input.GetKey(KeyCode.Escape))
                EndGame();
        }
    }

    IEnumerator StartStage()
    {
        Init();
        while(!isStart)
            yield return null;

        while(true)
        {
            //timer down
            time.value -= timeSpeed * Time.deltaTime;
            timeCount += 1f * Time.deltaTime;
            rateCount += 1f * Time.deltaTime;

            if (time.value <= 0)
            {
                GameOver();
                yield break;
            }

            yield return null;

            //속도증가
            if (timeCount >= TERM_TIME_SPEED) 
            {
                timeCount -= TERM_TIME_SPEED;
                if(timeSpeed < MAX_TIME_SPEED)
                    timeSpeed += INCREASE_TIME_SPEED;
            }

            //공격 고블린 비율 증가
            if (rateCount >= TERM_ATTACK_GOBLINE_RATE) 
            {
                rateCount -= TERM_ATTACK_GOBLINE_RATE;
                if (AttackGoblinRate < MAX_ATTACK_GOBLINE_RATE)
                    AttackGoblinRate += INCREASE_ATTACK_GOBLINE_RATE;
            }

            GM.CheckBestScore();
        }
    }

    void CheckBestScore()
    {
        if(GM.scoreUpdate && score > bestScore)
        {
            SoundManager.PlayBestScoreUpdate();
            GM.scoreUpdate = false;
        }
    }

    private void Init()
    {
        time.value = MAX_TIME;
        timeSpeed = MIN_TIME_SPEED;
        timeCount = 0f;
        rateCount = 0f;
        AttackGoblinRate = MIN_ATTACK_GOBLINE_RATE;
        RedGoblinRate = RED_GOBLINE_RATE;
        score = 0;
        expectScore = 0;
        
        isArrowDrawed = false;
        isStart = false;
        GM.scoreUpdate = true;

        for (int i = 0; i < 12; i++)
            Battle.CreateEnemy();
    }

    public static void TimeUp()
    {
        time.value += BONUS_TIME_VALUE;
        score += 1;
        GM.nowScore.text = score.ToString();
    }

    public static void setTime(float time)
    {
        GameManager.time.value = time;
    }

    public static void GameOver()
    {
        Swordman.setPlayerState(4);

        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("SpeedBestScore", score);
            PlayerPrefs.Save();
        }

        GM.bestScoreText.text = bestScore.ToString();
        GM.scoreText.text = score.ToString();
        GM.scoreBoard.SetActive(false);
        GM.notice.SetActive(true);

        SoundManager.PlayGameOverSound();
        ParticleManager.CreateBrokenHeartParticle();
    }

    public void RestartGame()
    {
        Battle.ClearEnemy();
        notice.SetActive(false);
        scoreBoard.SetActive(true);
        Swordman.AnimationInit();
        GM.nowScore.text = "0";
        SoundManager.BGMStart();
        StartCoroutine("StartStage");
    }

    public void EndGame()
    {
        Battle.ClearEnemy();
        SceneManager.LoadScene("Main");
        notice.SetActive(false);
    }

    public void HideGuide()
    {
        guide.SetActive(false);
    }
}
