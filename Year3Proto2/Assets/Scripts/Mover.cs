using UnityEngine;

public class Mover : MonoBehaviour
{
    private float radius = 10f;
    private float angle = 0f;

    void Update()
    {
        angle += Time.deltaTime * 0.1f;
        if (angle > 180f)
        {
            angle = -180f;
        }
        transform.position = new Vector3(Mathf.Sin(angle) * radius, 0.0f, Mathf.Cos(angle) * radius);
    }
}
