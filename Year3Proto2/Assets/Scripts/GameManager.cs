using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public struct PlayerData
    {
        public int rFood;
        public int rWood;
        public int rMetal;

        public PlayerData(int _startAmount)
        {
            rFood = _startAmount;
            rWood = _startAmount;
            rMetal = _startAmount;
        }
    }

    public PlayerData playerData;

    // Start is called before the first frame update
    void Start()
    {
        playerData = new PlayerData(1000);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
