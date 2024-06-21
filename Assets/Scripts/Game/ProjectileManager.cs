using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    private void Start()
    {
        Invoke(nameof(Destroy), 5);
    }

    private void Destroy()
    {
        FindObjectOfType<GameManager>().OnProjectileHit();
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        CancelInvoke();
        //Blast Effect
        Destroy();
    }
}