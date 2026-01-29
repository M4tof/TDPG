using UnityEngine;
using System;
using System.Collections.Generic;

public class ResourceSystem : MonoBehaviour
{

    public static ResourceSystem Instance { get; set; }

    public class Resource
    {
        private float value;
        private bool canGoIntoDebt;

        private float maxValue;
        private float regenRate;

        private readonly Action<float> onChange;

        public float Value => value;
        public bool Debt => canGoIntoDebt;
        public float Max => maxValue;
        public float Regen => regenRate;

        // Checks resource availability for withdrawal
        // On success reduces value and returns True; else returns False
        public bool Claim(float amount)
        {
            float compTarget = canGoIntoDebt ? 0.0f : amount;

            if (value < compTarget)
            {
                return false;
            }
            else
            {
                value -= amount;
                onChange?.Invoke(value);
                return true;
            }
        }

        public bool Grant(float amount)
        {
            if (value + amount < maxValue)
            {
                value += amount;
                onChange?.Invoke(value);
                return true;
            }
            else
            {
                return false;
            }
        }

        // If non-bool value needed use Resource.Value
        public bool CanAfford(float amount)
        {
            float compTarget = canGoIntoDebt ? 0.0f : amount;
            return value >= compTarget;
        }


        public void Update()
        {
            float currVal = value;
            if (value + regenRate < maxValue)
            {
                value += regenRate;
            }
            else
            {
                value = maxValue;
            }
            if (currVal != value)
            {
                onChange?.Invoke(value);
            }
        }

        public Resource(float initVal, float max, float regen, bool enableDebt, Action<float> callback)
        {
            value = initVal;
            maxValue = max;
            regenRate = regen;
            canGoIntoDebt = enableDebt;
            onChange = callback;
            onChange?.Invoke(value);
        }
    }

    [SerializeField] private float maxValue = 4000f;

    public Resource money;
    public Resource mana;
    List<Resource> updateList;

    public static event Action<float> onMoneyChange;
    public static event Action<float> onManaChange;
    void Start()
    {

    }
    void Update()
    {
        foreach (var res in updateList)
        {
            res.Update();
        }

    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Duplicate ResourceSystem destroyed. Only one instance allowed.");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ResourceSystem created and set to not destroy on load.");
            ResetResources();

        }
    }

    public ResourceSaveData GetData()
    {
        return new ResourceSaveData
        {
            MoneyValue = money.Value,
            MoneyMax = money.Max,
            MoneyDebt = money.Debt,
            MoneyRegen = money.Regen,
            ManaValue = mana.Value,
            ManaMax = mana.Max,
            ManaDebt = mana.Debt,
            ManaRegen = mana.Regen,
        };
    }

    public void LoadData(ResourceSaveData data)
    {
        updateList = new List<Resource>();
        money = new Resource(data.MoneyValue, data.MoneyMax, data.MoneyRegen, data.MoneyDebt, onMoneyChange);
        mana = new Resource(data.ManaValue, data.ManaMax, data.ManaRegen, data.ManaDebt, onManaChange);
        updateList.Add(money);
        updateList.Add(mana);
    }

    public void ResetResources()
    {
        money = new Resource(200f, maxValue, 2f, false, onMoneyChange);
        mana = new Resource(400f, maxValue, 0.0f, false, onManaChange);
        updateList = new List<Resource>();
        updateList.Add(money);
        updateList.Add(mana);
    }
}
