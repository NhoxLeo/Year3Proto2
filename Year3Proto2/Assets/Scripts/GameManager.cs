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

    public class RecentBatch
    {
        public float age;
        public int amount;
        public ResourceType type;

        public RecentBatch(int _amount, ResourceType _type)
        {
            age = 0f;
            amount = _amount;
            type = _type;
        }

        public void AddTime(float _time)
        {
            age += _time;
        }
    }

    private float batchMaxAge = 10.0f;

    private List<RecentBatch> recentBatches;

    int recentWood
    {
        get
        {
            int runningTotal = 0;
            foreach (RecentBatch batch in recentBatches)
            {
                if (batch.type == ResourceType.wood)
                {
                    runningTotal += batch.amount;
                }
            }
            return runningTotal;
        }
    }

    public PlayerData playerData;

    // Start is called before the first frame update
    void Start()
    {
        playerData = new PlayerData(1000);
        recentBatches = new List<RecentBatch>();
    }

    // Update is called once per frame
    void Update()
    {
        List<RecentBatch> batchesToRemove = new List<RecentBatch>();
        for (int i = 0; i < recentBatches.Count; i++)
        {
            recentBatches[i].AddTime(Time.deltaTime);
            //Debug.Log(recentBatches[i].age);
            if (recentBatches[i].age >= batchMaxAge)
            {
                batchesToRemove.Add(recentBatches[i]);
            }
        }
        foreach (RecentBatch batch in batchesToRemove)
        {
            recentBatches.Remove(batch);
        }
    }

    public void AddWood(int _batch)
    {
        playerData.rWood += _batch;
        recentBatches.Add(new RecentBatch(_batch, ResourceType.wood));
    }

    public float GetWoodVelocity(int _seconds)
    {
        return recentWood * _seconds / batchMaxAge;
    }
}
