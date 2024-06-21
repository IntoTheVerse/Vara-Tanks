using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform projectilePoint;
    private HexData currentHex;

    public void InitPlayer(HexData data)
    {
        SetCurrentHex(data);
    }

    public HexData GetCurrentHex()
    {
        return currentHex;
    }

    public Transform GetProjectilePoint()
    {
        return projectilePoint;
    }

    public void SetCurrentHex(HexData currentHex)
    {
        this.currentHex = currentHex; 
    }
}