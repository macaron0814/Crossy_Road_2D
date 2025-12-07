using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("ゲーム設定")]
    public bool isGameOver = false;
    public int currentScore = 0;
    public int highScore = 0;
    public int hp = 100;
    public float staminaInterval;

    public event Action<int> OnStaminaUpdated;
    public event Action<int> OnScoreUpdated;
    public event Action OnGameOver;
    public event Action OnGameOverPlayer;
    public event Action OnGameRestart;

    private Transform playerTransform;
    private float startPlayerPosY;
    private int maxReachedY = 0;
    private Coroutine staminaCoroutine;

    void Awake()
    {
        // シングルトンパターン
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ハイスコア読み込み
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // シーンロード時に初期化
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeGame();
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        // プレイヤーを検索
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            startPlayerPosY = playerTransform.position.y;
        }

        // スタミナコルーチンを停止
        if (staminaCoroutine != null)
        {
            StopCoroutine(staminaCoroutine);
        }
        
        hp = 100;
        staminaCoroutine = StartCoroutine(UpdateStamina(staminaInterval));
        
        // 初期値をUIに通知
        OnStaminaUpdated?.Invoke(hp);
        OnScoreUpdated?.Invoke(currentScore);
    }

    void Update()
    {
        if (isGameOver || playerTransform == null) return;

        // プレイヤーの最大Y座標を追跡してスコア更新
        int currentY = Mathf.FloorToInt(playerTransform.position.y - startPlayerPosY);
        if (currentY > maxReachedY)
        {
            maxReachedY = currentY;
            UpdateScore(maxReachedY);
        }
    }

    public void UpdateScore(int newScore)
    {
        currentScore = newScore;
        OnScoreUpdated?.Invoke(currentScore);

        // ハイスコア更新
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    private IEnumerator UpdateStamina(float interval)
    {
        while (hp > 0)
        {
            if (isGameOver) yield break;

            yield return new WaitForSeconds(interval);
            hp--;
            OnStaminaUpdated?.Invoke(hp);
        }
        hp = 0;
        GameOver();
    }

    public void AddStamina(int addHP)
    {
        hp += addHP;
        if (hp > 100) hp = 100;
        OnStaminaUpdated?.Invoke(hp);
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        OnGameOver?.Invoke();
        
        // プレイヤーが破棄される前にイベントを呼び出す
        // イベント内でプレイヤーが破棄される可能性があるため、安全に呼び出す
        if (OnGameOverPlayer != null)
        {
            OnGameOverPlayer.Invoke();
        }
    }

    public void RestartGame()
    {
        isGameOver = false;
        currentScore = 0;
        maxReachedY = 0;
        hp = 100;
        
        // スタミナコルーチンを停止
        if (staminaCoroutine != null)
        {
            StopCoroutine(staminaCoroutine);
        }
        
        OnGameRestart?.Invoke();

        // シーンをリロード（またはプレイヤーを初期位置に戻す）
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}

