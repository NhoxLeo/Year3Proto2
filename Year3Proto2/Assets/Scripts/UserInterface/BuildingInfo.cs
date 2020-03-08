using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BuildingInfo : MonoBehaviour
{
    private Tooltip tool;

    public Sprite defenceSprite;
    public Sprite foodSprite;
    public Sprite woodSprite;
    public Sprite metalSprite;

    private TMP_Text statHeading;
    private TMP_Text statValue;
    private Image statIcon;

    private TMP_Text foodValue;

    public enum Buildings
    {
        None,
        Archer,
        Catapult,
        Farm,
        Granary,
        LumberMill,
        LumberPile,
        Mine,
        MetalStorage
    }



    void Start()
    {
        
    }


    void Update()
    {
        
    }
}
