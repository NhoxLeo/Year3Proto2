using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSoundManager : MonoBehaviour
{
    public void PlayUIClick(int _variation)
    {
        GameManager.CreateAudioEffect("UIClick" + _variation.ToString(), Camera.main.transform.position, SoundType.SoundEffect, 1f, false);
    }

    public void PlayUITap()
    {
        GameManager.CreateAudioEffect("UITap", Camera.main.transform.position, SoundType.SoundEffect, 1f, false);
    }
}
