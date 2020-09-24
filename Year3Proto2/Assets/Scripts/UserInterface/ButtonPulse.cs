using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ButtonPulse : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;
    private bool soundSet;

    private void SetSound()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.825f;
        clickSound = FindObjectOfType<SceneSwitcher>().clickSound;
        audioSource.clip = clickSound;

        soundSet = true;
    }

    public void ButtonClick()
    {
        transform.DOKill(true);
        transform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0.0f), 0.2f, 1, 0.0f);

        if (!soundSet)
        {
            SetSound();
        }

        audioSource.Play();
    }
}
