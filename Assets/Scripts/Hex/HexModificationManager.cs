using TMPro;
using UnityEngine;

public class HexModificationManager : MonoBehaviour
{
    [Range(0, 1)] public float changeStrength;
    [Range(0, 10)] public float minHeight;
    [Range(0, 10)] public float maxHeight;
    [Range(0, 1)] public float influenceStrength;
    [Range(0.001f, 10)] public float minInfluenceToConsider;

    [HideInInspector] public bool scaleUp = true;
    [HideInInspector] public bool canEdit = false;
    [HideInInspector] public bool influence = false;

    private HexManager manager;

    private void Start()
    {
        manager = GetComponent<HexManager>();
    }

    public void ChangeStrength(float val)
    {
        changeStrength = val;
    }

    public void ChangeScaleDirection(bool val)
    {
        scaleUp = val;
    }

    public void EditButton(GameObject editPanel)
    {
        editPanel.SetActive(!editPanel.activeSelf);
        canEdit = editPanel.activeSelf;
    }

    public void LoadMap(TMP_InputField data)
    {
        manager.SetData(data.text);
    }

    public void Influence(bool toggle)
    {
        influence = toggle;
    }
}