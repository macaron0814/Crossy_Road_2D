using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("UI要素")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public GameObject gameOverPanel;
    public Button restartButton;
    public Slider sliderStamina;

    [Header("スコア表示設定")]
    public string scoreFormat = "Score: {0}";
    public string highScoreFormat = "High Score: {0}";

    private void Start()
    {
        // GameManagerのイベントを購読
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStaminaUpdated += UpdateStaminaDisplay;
            GameManager.Instance.OnScoreUpdated += UpdateScoreDisplay;
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnGameRestart += HideGameOver;
        }

        // リスタートボタンの設定
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        // 初期表示
        UpdateScoreDisplay(0);
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // ハイスコア表示
        UpdateHighScoreDisplay();
    }

    private void OnDestroy()
    {
        // イベントの購読を解除
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStaminaUpdated -= UpdateStaminaDisplay;
            GameManager.Instance.OnScoreUpdated -= UpdateScoreDisplay;
            GameManager.Instance.OnGameOver -= ShowGameOver;
            GameManager.Instance.OnGameRestart -= HideGameOver;
        }
    }

    void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, score);
        }
    }

    void UpdateStaminaDisplay(int stamina)
    {
        if (sliderStamina != null)
        {
            float staminaValue = stamina / 100f;
            sliderStamina.value = staminaValue;
        }
    }

    void UpdateHighScoreDisplay()
    {
        if (highScoreText != null && GameManager.Instance != null)
        {
            highScoreText.text = string.Format(highScoreFormat, GameManager.Instance.highScore);
        }
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        UpdateHighScoreDisplay();
    }

    void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void RestartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}

