using UnityEngine;

public class Hex : MonoBehaviour, IScalable
{
    private HexModificationManager modManager;
    private int underInfluenceBy = 0;
    private Hex[] neighbours;
    private HexData hexData;
    private Material material;

    [Header("Colors")]
    public Color originalColor;
    public Color moveableColor;
    public Color hoverColor;

    public void Start()
    {
        modManager = FindObjectOfType<HexModificationManager>();
        material = GetComponent<MeshRenderer>().material;
        material.color = originalColor;
    }

    public void ModifyHeight(float changeStrength = 0, int influencedBy = 0)
    {
        if (!modManager.canEdit) return;
        if (underInfluenceBy == 0) underInfluenceBy = influencedBy;
        if (underInfluenceBy != influencedBy) return;

        if (changeStrength == 0) changeStrength = modManager.changeStrength;

        if (modManager.scaleUp && transform.localScale.z < modManager.maxHeight)
        {
            transform.localScale += new Vector3(0, 0, changeStrength);
        }

        if (!modManager.scaleUp && transform.localScale.z > modManager.minHeight)
        { 
            transform.localScale += new Vector3(0, 0, changeStrength * -1);
        }

        if (transform.localScale.z > modManager.maxHeight) transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, modManager.maxHeight);
        if (transform.localScale.z < modManager.minHeight) transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, modManager.minHeight);

        if (modManager.influence) Influence(changeStrength * modManager.influenceStrength);
    }

    private void Influence(float changeStrength)
    {
        if (changeStrength < modManager.minInfluenceToConsider) return;
        foreach (var neighbour in neighbours)
        {
            neighbour.ModifyHeight(changeStrength, gameObject.GetInstanceID());
        }
    }

    public void InfluenceLost()
    {
        underInfluenceBy = 0;
    }

    public void SetNeighbours(Hex[] neighbours)
    {
        this.neighbours = neighbours;
    }

    public void SetHexData(HexData hexData)
    {
        this.hexData = hexData;
    }

    public HexData GetHexData()
    {
        return hexData;
    }

    public Hex[] GetNeighbours()
    {
        return neighbours;
    }

    public void ChangeToMoveableColor()
    {
        material.color = moveableColor;
    }

    public void ChangeToOriginalColor()
    {
        material.color = originalColor;
    }

    public void ChangeToHoverColor()
    {
        material.color = hoverColor;
    }
}