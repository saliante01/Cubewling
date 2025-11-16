using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private int score = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void AddPoints(int amount)
    {
        score += amount;
        Debug.Log("PUNTOS + " + amount + "  | TOTAL = " + score);
    }

    public int GetScore()
    {
        return score;
    }

    public void ResetScore()
    {
        score = 0;
    }
}
