using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TMP_Text scoreText;

    private void Start()
    {
        UpdateScore();
    }

    private void Update()
    {
        UpdateScore();
    }

    private void UpdateScore()
    {
        if (ScoreManager.Instance != null)
            scoreText.text = "SCORE: " + ScoreManager.Instance.GetScore();
    }
}
