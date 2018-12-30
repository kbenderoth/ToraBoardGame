using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JeopardyManager : MonoBehaviour
{

    // NOTE: players should be able to turn off Jeopardy without it affecting the overall game (proof that it's an independent module)   

    // Colorize the board pieces based on threat level

    // Jeopardy can affect movement for game pieces (this needs to be implemented)

    // Any time there's been a change on the board state, Jeopardy needs to be updated (this needs to be implemented)

    // For special pieces (such as cannon) threat can be different (this needs to be implemented)

    // Threat is determined by how many pieces that belongs to opposition or challenger can land (or attack??) that tile


    // MORE NOTES ABOUT JEOPARDY
    //      - right now, there's a dark middle line; it should be light colored
    //      - possibly due to threat divided by total; I should do the main guy's threat divided by total
    //      - making jeopardy dynamic should be after gutting
    //      - the cannon piece should be done last
    //      - game needs to prevent the general from moving unless he's wounded

    private BoardPiece[] _boardPieces; // Reference to board pieces

    public Material HighlightMaterial; // Material to apply on each highlight piece for coloring the threat level

    private GameObject[] _highlightPieces; // Pieces to color for showing threat levels
    private ThreatPiece[] _threatLevels; // How high a threat is for each player plus the total; index: board piece position;

    private Color _noThreatColor = new Color(0, 0, 0, 0); // Color for no threat (basically invisible)
    private Color _fullOppositionColor = new Color(0, 1, 0);
    private Color _fullChallengerColor = new Color(1, 0, 1);



    // FOR DEBUGGING
    public GameObject ThreatText;
    private Text[] _oppositionThreatText;
    private Text[] _challengerThreatText;



    // Use this for initialization
    void Start()
    {

    }

    /// <summary>
    /// Called explicitly if Jeopardy is active. Jeopardy will not be used if Init is not called
    /// </summary>
    public void Init(BoardPiece[] boardPieces)
    {
        _boardPieces = boardPieces;

        // Initialize threat
        _threatLevels = new ThreatPiece[_boardPieces.Length];
        // Create a layer above the board pieces for highlighting
        _highlightPieces = new GameObject[_boardPieces.Length];
        _oppositionThreatText = new Text[_boardPieces.Length];
        _challengerThreatText = new Text[_boardPieces.Length];
        for (var index = 0; index < _threatLevels.Length; ++index)
        {
            _threatLevels[index] = new ThreatPiece();
            _highlightPieces[index] = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _highlightPieces[index].transform.position = _boardPieces[index].transform.position + Vector3.up * 0.05f;
            _highlightPieces[index].transform.rotation = _boardPieces[index].transform.rotation;
            _highlightPieces[index].transform.parent = _boardPieces[index].transform;
            _highlightPieces[index].transform.GetComponent<Collider>().enabled = false;
            _highlightPieces[index].transform.GetComponent<Renderer>().material = HighlightMaterial; // TODO: make the prefab color have 0 alpha for simplicity


            // TEXT (just to get it working)
            var oppositionGameText = Instantiate(ThreatText, GameObject.Find("World UI").transform);
            oppositionGameText.name = "Opposition Threat";
            oppositionGameText.transform.position = _highlightPieces[index].transform.position + Vector3.up * 0.05f;

            _oppositionThreatText[index] = oppositionGameText.GetComponent<Text>();
            _oppositionThreatText[index].text = "0";
            _oppositionThreatText[index].alignment = TextAnchor.UpperRight;

            
            var challengerGameText = Instantiate(ThreatText, GameObject.Find("World UI").transform);
            challengerGameText.name = "Challenger Threat";
            challengerGameText.transform.position = _highlightPieces[index].transform.position + Vector3.up * 0.05f;

            _challengerThreatText[index] = challengerGameText.GetComponent<Text>();
            _challengerThreatText[index].text = "1";
            _challengerThreatText[index].alignment = TextAnchor.UpperLeft;

        }


        // TODO: may need to activate a listener
    }

    public void CalculateJeopardy(List<TokenPiece> oppositionTokens, List<TokenPiece> challengerTokens)
    {
        // Go through EACH piece
        foreach (var oppositionToken in oppositionTokens)
        {
            // Ignore the cannon for now, that one is special with its threat
            if (oppositionToken.TokenID == ID.ID_CANNON) continue;
            // Find the possible locations
            var validTiles = GameHelper.FindApproachableTiles(oppositionToken, oppositionTokens, challengerTokens, _boardPieces, true); // NOTE: I think it should always be true to check THREAT

            foreach (var tile in validTiles)
            {
                // Add to the threat level
                _threatLevels[tile].OppositionThreatLevel++;
            }
        }

        foreach (var challengerToken in challengerTokens)
        {
            // Ignore the cannon for now, that one is special with its threat
            if (challengerToken.TokenID == ID.ID_CANNON) continue;
            // Find the possible locations
            var validTiles = GameHelper.FindApproachableTiles(challengerToken, challengerTokens, oppositionTokens, _boardPieces, true); // NOTE: I think it should always be true to check THREAT

            foreach (var tile in validTiles)
            {
                // Add to the threat level
                _threatLevels[tile].ChallengerThreatLevel++;
            }
        }

        // Set the board
        for(var index = 0; index < _highlightPieces.Length; ++index)
        {
            var renderer = _highlightPieces[index].GetComponent<Renderer>();
            var threatSum = _threatLevels[index].TotalThreatLevel;

            _oppositionThreatText[index].enabled = _threatLevels[index].OppositionThreatLevel > 0;
            _oppositionThreatText[index].text = _threatLevels[index].OppositionThreatLevel.ToString();
            _challengerThreatText[index].enabled = _threatLevels[index].ChallengerThreatLevel > 0;
            _challengerThreatText[index].text = _threatLevels[index].ChallengerThreatLevel.ToString();

            if (threatSum == 0)            
                renderer.material.color = _noThreatColor;
            else
            {
                var oppositionThreatColor = _fullOppositionColor * _threatLevels[index].OppositionThreatRatio;
                var challengerThreatColor = _fullChallengerColor * _threatLevels[index].ChallengerThreatLevel;
                var threatColor = (oppositionThreatColor + challengerThreatColor)/2;

                var highestThreatLevel = Mathf.Max(_threatLevels[index].OppositionThreatLevel, _threatLevels[index].ChallengerThreatLevel);

                // experiment
                renderer.material.color = new Color(threatColor.r, threatColor.g, threatColor.b, highestThreatLevel / 6f);
            }
        }

    }


    // Update is called once per frame
    void Update()
    {

    }
}
