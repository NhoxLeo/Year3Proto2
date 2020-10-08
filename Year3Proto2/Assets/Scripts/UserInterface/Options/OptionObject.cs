using UnityEngine;
using TMPro;
using System.Globalization;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : OptionObject.cs
// Description  : Contains interface for data manipulation along with base class for objects.
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

public abstract class OptionObject : MonoBehaviour
{
    protected bool deserialised = false;
    [SerializeField] protected string key;
    [SerializeField] private TMP_Text displayName; 

    /**************************************
    * Name of the Function: Start
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void SetKey(string _key)
    {
        transform.name = _key;
        key = _key;

        TextInfo cultInfo = new CultureInfo("en-US", false).TextInfo;
        displayName.text = cultInfo.ToTitleCase(_key.ToLower().Replace("_", " "));
    }

    /**************************************
    * Name of the Function: GetKey
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: String
    ***************************************/
    public string GetKey()
    {
        return key;
    }

    public abstract OptionData GetData();
    public abstract void SetData(OptionData _data);
    public abstract void Deserialize();
    public abstract void Serialize();
}
