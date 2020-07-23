using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionCategoryObject : MonoBehaviour
{
    [SerializeField] private Transform panel;

    public void OnClick()
    {
        Transform parent = transform.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform transform = parent.GetChild(i);
            if(transform)
            {
                OptionCategoryObject categoryObject = transform.GetComponent<OptionCategoryObject>();
                categoryObject.GetPanel().gameObject.SetActive(false);
            }
        }

        // Animation do tween.
        // 
        panel.gameObject.SetActive(true);
    }

    public Transform GetPanel()
    {
        return panel;
    }
}
