using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotationsPerMinute = 10.0f;

    public void Update()
    {
        transform.Rotate(0, 6.0f * rotationsPerMinute * Time.deltaTime, 0);
    }
}