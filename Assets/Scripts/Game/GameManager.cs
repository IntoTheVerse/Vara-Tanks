using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private Player spawnedPlayer = null;
    private Player spawnedOpponent = null;
    private List<Hex> moveableHexes = new();
    private bool currentlyMoving = false;
    private bool currentlyTargeting = false;
    private Hex currentHoverHex = null;
    private Transform launchPoint;

    [Header("Fuel")]
    public int fuel = 5;
    public TextMeshProUGUI fuelText;

    [Header("Player")]
    public Player player;

    [Header("Cameras")]
    public Camera topDownCam;
    public Camera drownCam;

    [Header("Masks")]
    public LayerMask hexMask;

    [Header("UI")]
    public GameObject moveButton;
    public GameObject skipMoveButton;
    public GameObject cancelMoveButton;
    public GameObject projectileUI;
    public TextMeshProUGUI speedValue;
    public TextMeshProUGUI angleValue;

    [Header("Projectile")]
    public GameObject projectile;
    public float projectileSpeed = 10f;

    private void Update()
    {
        if (currentlyMoving)
        {
            if (Physics.Raycast(topDownCam.ScreenToWorldPoint(Input.mousePosition), topDownCam.transform.forward, out RaycastHit hit, Mathf.Infinity, hexMask))
            {
                Hex hitHex = hit.transform.GetComponent<Hex>();
                if (Input.GetMouseButtonDown(0)) MoveTo(currentHoverHex);
                if (currentHoverHex != hitHex)
                { 
                    if(currentHoverHex != null) currentHoverHex.ChangeToMoveableColor();
                    if (moveableHexes.Contains(hitHex))
                    {
                        currentHoverHex = hitHex;
                        currentHoverHex.ChangeToHoverColor();
                        fuelText.text = $"Fuel: {fuel}-{GetDistanceFromCurrentHex(currentHoverHex)}";
                    }
                    else
                    {
                        currentHoverHex = null;
                        fuelText.text = $"Fuel: {fuel}";
                    }
                }
            }
        }

        if (currentlyTargeting)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GameObject launchedProjectile = Instantiate(projectile, launchPoint.position, launchPoint.rotation);
                launchedProjectile.GetComponent<Rigidbody>().velocity = projectileSpeed * launchPoint.up;
                //currentlyTargeting = false;
            }
        }

        GetHorizontalRange((projectileSpeed * launchPoint.up).magnitude);
    }

    public void GetHorizontalRange(float velocity)
    {
        float angle = launchPoint.localEulerAngles.x;
        // Debug.Log(angle);
        float range = velocity * velocity * Mathf.Sin(2 * angle) / Physics.gravity.magnitude;
        Debug.Log($"Range: {range}");
        Debug.Log($"Distance: {Vector3.Distance(spawnedPlayer.GetProjectilePoint().position, spawnedOpponent.GetProjectilePoint().position)}");
    }

    public void GameInit()
    {
        fuelText.text = $"Fuel: {fuel}";
        Vector3 hexSize = GetComponent<HexManager>().size;
        HexData playerSpawnHex = FindObjectOfType<HexManager>().GetHexDataByPos((int)(hexSize.y / 2 * hexSize.x));
        HexData opponentSpawnHex = FindObjectOfType<HexManager>().GetHexDataByPos((int)(hexSize.y / 2 * hexSize.x + (hexSize.y - 1)));
        spawnedPlayer = Instantiate(player, GetSpawnPosFromHexData(playerSpawnHex, player.transform.localScale), Quaternion.identity);
        spawnedOpponent = Instantiate(player, GetSpawnPosFromHexData(opponentSpawnHex, player.transform.localScale), Quaternion.identity);
        spawnedPlayer.InitPlayer(playerSpawnHex);
        spawnedOpponent.InitPlayer(opponentSpawnHex);

        spawnedPlayer.transform.LookAt(spawnedOpponent.transform);
        launchPoint = spawnedPlayer.GetProjectilePoint();
    }

    private int GetDistanceFromCurrentHex(Hex otherHex)
    {
        return (int)(spawnedPlayer.GetCurrentHex().coordinate - otherHex.GetHexData().coordinate).magnitude;
    }

    private Vector3 GetSpawnPosFromHexData(HexData data, Vector3 playerScale)
    {
        Vector3 position = data.transform.position;
        position.y += data.transform.localScale.z;
        position.y += playerScale.y / 2;

        return position;
    } 

    public void Move()
    {
        if (fuel == 0)
        {
            SetUI(false, true, false, false);
            return;
        }
        if (spawnedPlayer != null)
        {
            SetUI(false, false, true, false);
            currentlyMoving = true;
            moveableHexes = new(){ spawnedPlayer.GetCurrentHex().hex };
            HashSet<Hex> visitedHexes = new(){ spawnedPlayer.GetCurrentHex().hex };

            for (int i = 0; i < fuel; i++)
            {
                List<Hex> nextLevelHexes = new();

                foreach (Hex hex in moveableHexes)
                {
                    Hex[] neighbours = hex.GetNeighbours();
                    foreach (Hex neighbour in neighbours)
                    {
                        if (!visitedHexes.Contains(neighbour))
                        {
                            visitedHexes.Add(neighbour);
                            nextLevelHexes.Add(neighbour);
                        }
                    }
                }

                moveableHexes.AddRange(nextLevelHexes);
            }

            spawnedPlayer.transform.LookAt(spawnedOpponent.transform);
            foreach (Hex h in moveableHexes)
            {
                h.ChangeToMoveableColor();
            }
        }
        else Debug.LogError("Spawned Player is Null");
    }

    private void MoveTo(Hex hex)
    {
        foreach (Hex h in moveableHexes)
        {
            h.ChangeToOriginalColor();
        }
        fuel -= GetDistanceFromCurrentHex(hex);
        fuelText.text = $"Fuel: {fuel}";
        spawnedPlayer.transform.position = GetSpawnPosFromHexData(hex.GetHexData(), spawnedPlayer.transform.localScale);
        spawnedPlayer.SetCurrentHex(hex.GetHexData());
        SkipMove();
        currentlyMoving = false;
    }

    private void SetUI(bool move, bool skip, bool cancel, bool projectile)
    {
        skipMoveButton.SetActive(skip);
        cancelMoveButton.SetActive(cancel);
        moveButton.SetActive(move);
        projectileUI.SetActive(projectile);
    }

    public void SkipMove()
    {
        SetUI(false, false, false, false);
        SetTarget();
    }

    public void CancelMove()
    {
        currentlyMoving = false;
        foreach (Hex h in moveableHexes)
        {
            h.ChangeToOriginalColor();
        }
        fuelText.text = $"Fuel: {fuel}";
        SkipMove();
    }

    public void SetTarget()
    {
        topDownCam.gameObject.SetActive(false);
        drownCam.gameObject.SetActive(true);
        SetUI(false, false, false, true);
        currentlyTargeting = true;
    }

    public void OnProjectileHit()
    { 

    }

    public void OnSpeedSliderChange(float val)
    {
        projectileSpeed = val;
        speedValue.text = val.ToString("F2");
    }

    public void OnAngleSliderChange(float angle)
    {
        launchPoint.localEulerAngles = new Vector3(angle, 0, 0);
        angleValue.text = angle.ToString("F2");
    }
}