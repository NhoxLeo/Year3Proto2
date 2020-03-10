using UnityEngine;

public class Mover : MonoBehaviour
{
    public Vector3 moveSpeed;
    public float startDelay;

    private float age;

    void Update()
    {
        age += Time.deltaTime;

        if (age >= startDelay)
        {
            transform.position += moveSpeed * Time.deltaTime;
        }
    }
}
