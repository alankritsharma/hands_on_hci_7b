using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // Singleton (other scripts expect ScoreManager.Instance)
    public static ScoreManager Instance;

    // Reference to the center of your target (assign in Inspector)
    public Transform targetCenter;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterHit(Arrow arrow, Vector3 hitPoint)
    {
        float distance = Vector3.Distance(hitPoint, targetCenter.position);

        int score = CalculateScore(distance);
        UpdateUI(score);
        LogData(score, distance);
    }

    private int CalculateScore(float distance)
    {
        // Example scoring logic — edit as needed
        if (distance < 0.1f) return 10;
        if (distance < 0.2f) return 8;
        if (distance < 0.3f) return 5;
        if (distance < 0.4f) return 3;
        return 1;
    }

    private void UpdateUI(int score)
    {
        // TODO: update your UI elements here
        Debug.Log("Score: " + score);
    }

    private void LogData(int score, float distance)
    {
        // TODO: save/log this somewhere if needed
        Debug.Log($"Logged hit: score={score} distance={distance}");
    }
}
