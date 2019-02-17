using UnityEngine;

public delegate void OnSelectedBoardPiece(GameObject piece);

public class BoardPiece : MonoBehaviour
{
	public OnSelectedBoardPiece OnPieceSelected;
	public ClickHandler MouseClickHandler;

	public Material MoveableMaterial;
	public Material AttackableMaterial;
    public Material AbilityMaterial;
	private Material _normalMaterial;
	private bool _isSelectable = false;
    private bool _isAbilitySelectable = false;
	private GameObject HighlightedObjectCopy;

    [HideInInspector]
    public TokenPiece Piece;

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
}