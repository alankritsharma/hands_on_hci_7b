public void RegisterHit(Arrow arrow, Vector3 hitPoint)
{
    float distance = Vector3.Distance(hitPoint, targetCenter.position);

    int score = CalculateScore(distance);
    UpdateUI(score);
    LogData(score, distance);
}
