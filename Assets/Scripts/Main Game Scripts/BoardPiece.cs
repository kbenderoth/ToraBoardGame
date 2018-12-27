using UnityEngine;
using System.Collections;

public delegate void OnSelectedBoardPiece(GameObject piece);

public class BoardPiece : MonoBehaviour
{
    [HideInInspector]
    public Material TransparentMaterial;
	public OnSelectedBoardPiece OnPieceSelected;
	public ClickHandler MouseClickHandler;
    [HideInInspector]
    public int OppositionThreatLevel { get { return _oppositionThreatLevel; } set { _oppositionThreatLevel = value; UpdateTileColor(); } }
    [HideInInspector]
    public int ChallengerThreatLevel { get { return _challengerThreatLevel; } set { _challengerThreatLevel = value; UpdateTileColor(); } }
    private Color _fullOppositionColor = new Color(0, 0, 1);
    private Color _fullChallengerColor = new Color(1, 0, 0);
    private Color _noThreatColor = new Color(0f, 0f, 0f, 0f);
	public Material MoveableMaterial;
	public Material AttackableMaterial;
    public Material AbilityMaterial;
	private Material _normalMaterial;
	private bool _isSelectable = false;
    private bool _isAbilitySelectable = false;
	private GameObject HighlightedObjectCopy;
    private GameObject ThreatObjectCopy;

    private int _oppositionThreatLevel = 0;
    private int _challengerThreatLevel = 0;

	void Start () {	}

    public void Init()
    {
        MouseClickHandler.OnPressed += OnClicked;

        // instantiate the tile prefab and make it invisible
        HighlightedObjectCopy = GameObject.CreatePrimitive(PrimitiveType.Quad);
        HighlightedObjectCopy.transform.position = transform.position;
        HighlightedObjectCopy.transform.rotation = transform.rotation;

        HighlightedObjectCopy.transform.parent = transform;
        HighlightedObjectCopy.GetComponent<Collider>().enabled = false;
        _normalMaterial = new Material(HighlightedObjectCopy.GetComponent<Renderer>().material);

        ThreatObjectCopy = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ThreatObjectCopy.transform.position = transform.position + Vector3.up * 0.2f;
        ThreatObjectCopy.transform.rotation = transform.rotation;

        ThreatObjectCopy.transform.parent = transform;
        ThreatObjectCopy.GetComponent<Collider>().enabled = false;        
        ThreatObjectCopy.GetComponent<Renderer>().material = TransparentMaterial;
        ThreatObjectCopy.GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, 0f);
    }

	public void ToggleSelectable(bool isSelectable, bool isAttacking = false)
	{
		_isSelectable = isSelectable;
        _isAbilitySelectable = false;
        var placementMaterial = isAttacking ? AttackableMaterial : MoveableMaterial;
        HighlightedObjectCopy.GetComponent<Renderer>().material = isSelectable ? placementMaterial : _normalMaterial;
	}

    public void ToggleAbilitySelectable(bool isSelectable)
    {
        _isAbilitySelectable = isSelectable;
        _isSelectable = false;
        HighlightedObjectCopy.GetComponent<Renderer>().material = isSelectable ? AbilityMaterial : _normalMaterial;
    }

	public void OnClicked(GameObject sender)
	{
		if ((_isAbilitySelectable || _isSelectable) && OnPieceSelected != null) 
			OnPieceSelected(gameObject);
	}

	public bool Selectable { get {return _isSelectable;} }

    public bool AbilitySelectable { get { return _isAbilitySelectable; } }
   
    public void UpdateTileColor()
    {
        // Color the tile based on the threat level
        var renderer = ThreatObjectCopy.GetComponent<Renderer>();
        float threatSum = _oppositionThreatLevel + _challengerThreatLevel;


        if (threatSum == 0)
        {
            renderer.material.color = _noThreatColor;
         }
        else
        {
            var oppositionRatio = _oppositionThreatLevel / threatSum;
            var challengerRatio = _challengerThreatLevel / threatSum;

            var oppositionThreatColor = _fullOppositionColor * oppositionRatio;
            var challengerThreatColor = _fullChallengerColor * challengerRatio;
            var threatColor = (oppositionThreatColor + challengerThreatColor) / 2;

            // experiment
            renderer.material.color = new Color(threatColor.r, threatColor.g, threatColor.b, threatSum / 7f);


            //renderer.material.color = _fullOppositionColor;
        }        


        
        //ThreatObjectCopy.GetComponent<Renderer>().sharedMaterial = TransparentMaterial;
        

    }
}