using UnityEngine;

public class Mover : MonoBehaviour
{
    public Vector3 moveSpeed = new Vector3(0.5f, 0f, 0.5f);
    public float startDelay = 0.5f;

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
