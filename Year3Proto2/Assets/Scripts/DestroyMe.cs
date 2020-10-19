using UnityEngine;

public class DestroyMe : MonoBehaviour
{
    [SerializeField] private float lifetime = 0f;
    private bool unscaledTime = false;

    // Update is called once per frame
    private void Update()
    {
        lifetime -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void SetLifetime(float _newLifetime)
    {
        lifetime = _newLifetime;
    }

    public void SetUnscaledTime(bool _unscaledTime)
    {
        unscaledTime = _unscaledTime;
    }
}
