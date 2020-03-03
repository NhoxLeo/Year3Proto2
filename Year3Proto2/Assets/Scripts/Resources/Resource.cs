using System;
using UnityEngine;

public enum ResourceType
{
    WOOD,
    ORE,
    FOOD
}

public class Resource : MonoBehaviour
{
    private ResourceType resourceType;
}