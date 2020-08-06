using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyMe : MonoBehaviour
{
    [SerializeField] private float lifetime = 0f;

    public void SetLifetime(float _newLifetime)
    {
        lifetime = _newLifetime;
    }

    // Update is called once per frame
    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
