using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Batch
{
    public float age;
    public int amount;
    public ResourceType type;

    public Batch(int _amount, ResourceType _type)
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

public struct PlayerData
{
    private int rWood;
    private int rMetal;
    private int rFood;

    private int rWoodMax;
    private int rMetalMax;
    private int rFoodMax;

    public PlayerData(int _startAmount, int _maxAmount)
    {
        rWood = _startAmount;
        rMetal = _startAmount;
        rFood = _startAmount;

        rWoodMax = _maxAmount;
        rMetalMax = _maxAmount;
        rFoodMax = _maxAmount;
    }

    public void AddBatch(Batch _batch)
    {
        switch (_batch.type)
        {
            case ResourceType.wood:
                rWood += _batch.amount;
                if (rWood > rWoodMax) { rWood = rWoodMax; }
                break;
            case ResourceType.metal:
                rMetal += _batch.amount;
                if (rMetal > rMetalMax) { rMetal = rMetalMax; }
                break;
            case ResourceType.food:
                rFood += _batch.amount;
                if (rFood > rFoodMax) { rFood = rFoodMax; }
                if (rFood < 0)
                {
                    //lose condition
                }
                break;
        }
    }

    public bool ResourceIsFull(ResourceType _type)
    {
        switch(_type)
        {
            case ResourceType.wood:
                return (rWood == rWoodMax);
            case ResourceType.metal:
                return (rMetal == rMetalMax);
            case ResourceType.food:
                return (rFood == rFoodMax);
        }
        return false;
    }

    public int GetResource(ResourceType _type)
    {
        int value = 0;
        switch (_type)
        {
            case ResourceType.wood:
                value = rWood;
                break;
            case ResourceType.metal:
                value = rMetal;
                break;
            case ResourceType.food:
                value = rFood;
                break;
        }
        return value;
    }

    public void SetMaximum(ResourceType _type, int _newMax)
    {
        switch (_type)
        {
            case ResourceType.wood:
                rWoodMax = _newMax;
                break;
            case ResourceType.metal:
                rMetalMax = _newMax;
                break;
            case ResourceType.food:
                rFoodMax = _newMax;
                break;
        }
    }

    public void DeductResource(ResourceType _type, int _deduction)
    {
        switch (_type)
        {
            case ResourceType.wood:
                rWood -= _deduction;
                break;
            case ResourceType.metal:
                rMetal -= _deduction;
                break;
            case ResourceType.food:
                rFood -= _deduction;
                break;
        }
    }

    public bool CanAfford(ResourceBundle _cost)
    {
        return rWood >= _cost.woodCost && rMetal >= _cost.metalCost && rFood >= _cost.foodCost;
    }

    public bool AttemptPurchase(ResourceBundle _cost)
    {
        if (CanAfford(_cost))
        {
            rWood -= _cost.woodCost;
            rMetal -= _cost.metalCost;
            rFood -= _cost.foodCost;
            return true;
        }
        else return false;
    }
}

public class GameManager : MonoBehaviour
{
    private float batchMaxAge = 3.0f;
    private float gameoverTimer = 5.0f;
    private List<Batch> recentBatches;

    private static Dictionary<string, AudioClip> audioClips;

    int recentWood
    {
        get
        {
            int runningTotal = 0;
            foreach (Batch batch in recentBatches)
            {
                if (batch.type == ResourceType.wood)
                {
                    runningTotal += batch.amount;
                }
            }
            return runningTotal;
        }
    }
    int recentMetal
    {
        get
        {
            int runningTotal = 0;
            foreach (Batch batch in recentBatches)
            {
                if (batch.type == ResourceType.metal)
                {
                    runningTotal += batch.amount;
                }
            }
            return runningTotal;
        }
    }
    int recentFood
    {
        get
        {
            int runningTotal = 0;
            foreach (Batch batch in recentBatches)
            {
                if (batch.type == ResourceType.food)
                {
                    runningTotal += batch.amount;
                }
            }
            return runningTotal;
        }
    }

    public PlayerData playerData;

    private void Awake()
    {
        audioClips = new Dictionary<string, AudioClip>
        {
            { "horn", Resources.Load("Audio/SFX/sfxHorn") as AudioClip },
            { "build", Resources.Load("Audio/SFX/sfxBuild") as AudioClip },
            { "arrow", Resources.Load("Audio/SFX/sfxArrow") as AudioClip },
            { "buildingHit", Resources.Load("Audio/SFX/sfxBuildingHit") as AudioClip },
            { "buildingDestroy", Resources.Load("Audio/SFX/sfxBuildingDestroy") as AudioClip },
            { "catapultFire", Resources.Load("Audio/SFX/sfxCatapultFire") as AudioClip },
            { "lose", Resources.Load("Audio/SFX/sfxLose") as AudioClip },
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        playerData = new PlayerData(200, 500);
        CalculateStorageMaximum();
        recentBatches = new List<Batch>();
    }

    // Update is called once per frame
    void Update()
    {
        List<Batch> batchesToRemove = new List<Batch>();
        for (int i = 0; i < recentBatches.Count; i++)
        {
            recentBatches[i].AddTime(Time.deltaTime);
            if (recentBatches[i].age >= batchMaxAge)
            {
                batchesToRemove.Add(recentBatches[i]);
            }
        }
        foreach (Batch batch in batchesToRemove)
        {
            recentBatches.Remove(batch);
        }
        if (!FindObjectOfType<Longhaus>())
        {
            gameoverTimer -= Time.deltaTime;
            if (gameoverTimer < 0f)
            {
                FindObjectOfType<SceneSwitcher>().SceneSwitch("TitleScreen");
            }
        }
    }

    public void AddBatch(Batch _newBatch)
    {
        playerData.AddBatch(_newBatch);
        recentBatches.Add(_newBatch);
    }

    public float GetWoodVelocity(int _seconds)
    {
        return recentWood * _seconds / batchMaxAge;
    }

    public float GetMetalVelocity(int _seconds)
    {
        return recentMetal * _seconds / batchMaxAge;
    }

    public float GetFoodVelocity(int _seconds)
    {
        return recentFood * _seconds / batchMaxAge;
    }

    public void CalculateStorageMaximum()
    {
        int newWoodMax = Longhaus.woodStorage;
        int newMetalMax = Longhaus.metalStorage;
        int newFoodMax = Longhaus.foodStorage;

        // gets every structure and adds to find totals.
        StorageStructure[] storageStructures = FindObjectsOfType<StorageStructure>();
        foreach (StorageStructure storageStructure in storageStructures)
        {
            switch (storageStructure.GetResourceType())
            {
                case ResourceType.wood:
                    newWoodMax += storageStructure.storage;
                    break;
                case ResourceType.metal:
                    newMetalMax += storageStructure.storage;
                    break;
                case ResourceType.food:
                    newFoodMax += storageStructure.storage;
                    break;
            }
        }

        playerData.SetMaximum(ResourceType.wood, newWoodMax);
        playerData.SetMaximum(ResourceType.metal, newMetalMax);
        playerData.SetMaximum(ResourceType.food, newFoodMax);
    }

    public void OnStructurePlace()
    {
        CalculateStorageMaximum();

        foreach (ResourceStructure resourceStructure in FindObjectsOfType<ResourceStructure>())
        {
            resourceStructure.OnPlace();
        }

        // Update all enemies
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            enemy.Next();
        }
    }

    public static void CreateAudioEffect(string _sfxName, Vector3 _positon, float _volume = 1.0f)
    {
        GameObject spawnAudio = new GameObject("TemporarySoundObject");
        spawnAudio.transform.position = _positon;
        AudioSource spawnAudioComp = spawnAudio.AddComponent<AudioSource>();
        DestroyMe spawnAudioDestroy = spawnAudio.AddComponent<DestroyMe>();
        spawnAudioDestroy.SetLifetime(audioClips[_sfxName].length);
        spawnAudioComp.spatialBlend = 1.0f;
        spawnAudioComp.rolloffMode = AudioRolloffMode.Linear;
        spawnAudioComp.maxDistance = 100f;
        spawnAudioComp.clip = audioClips[_sfxName];
        spawnAudioComp.Play();
        spawnAudioComp.volume = _volume;
    }
}
