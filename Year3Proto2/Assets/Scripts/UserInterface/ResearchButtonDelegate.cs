using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchButtonDelegate : MonoBehaviour
{
    public int ID;

    public void AttemptResearch()
    {
        FindObjectOfType<ResearchScreen>().ResearchButton(ID);
    }
}
