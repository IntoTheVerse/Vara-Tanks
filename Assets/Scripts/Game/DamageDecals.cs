using UnityEngine;

public class DamageDecals : MonoBehaviour
{
    private Camera cam;

    public GameObject decal;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Transform trans = Instantiate(decal, hit.transform).transform;
                trans.SetPositionAndRotation(hit.point, Quaternion.FromToRotation(fromDirection: -Vector3.forward, hit.normal));
            }
        }
    }
}