
using UnityEngine;

public interface OptionData<T>
{
    T GetData();
    void SetData(T _data);
    void Deserialise(T _data);
    void Serialise();
}

public class OptionObject : MonoBehaviour
{
    [SerializeField] protected string key;
    private void Start()
    {
        key = transform.name.ToUpper();
    }
    public string GetKey()
    {
        return key;
    }
}
