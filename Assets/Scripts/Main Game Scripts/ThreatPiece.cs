public struct ThreatPiece
{
    public int OppositionThreatLevel;
    public int ChallengerThreatLevel;

    public int TotalThreatLevel { get { return OppositionThreatLevel + ChallengerThreatLevel; } }
    public float OppositionThreatRatio { get { return OppositionThreatLevel / (float)TotalThreatLevel; } }
    public float ChallengerThreatRatio { get { return ChallengerThreatLevel / (float)TotalThreatLevel; } }
}
