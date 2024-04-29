using System;
using System.Collections;
using System.Collections.Generic;
using Unity.BossRoom.Infrastructure;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [SerializeField]private GameObject prefab;
    [SerializeField] private int MaxPrefabCount = 50;

    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnFoodStart;

    }

    private void SpawnFoodStart()
    {
        NetworkManager.Singleton.OnServerStarted -= SpawnFoodStart;

        //NetworkObjectPool.Singleton.InitializePool();
        for(int i = 0; i < 30; i++)
        {
            SpawnFood();
        }

        StartCoroutine(SpawnOverTime());
    }

    IEnumerator SpawnOverTime()
    {
        while(NetworkManager.Singleton.ConnectedClients.Count > 0)
        {

            if(NetworkObjectPool.Singleton.GetCurrentPrefabCount(prefab) < MaxPrefabCount)
            {
                SpawnFood();
                Debug.Log("Spawned Food");
            }            
            yield return new WaitForSeconds(2f);
        }
    }

    private void SpawnFood()
    {
        NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(prefab, GetRandomPositionOnMap(), Quaternion.identity);
        obj.GetComponent<Food>().prefab = prefab;
        obj.Spawn(false);
        if(!obj.IsSpawned) obj.Spawn(true);
    }

    private Vector3 GetRandomPositionOnMap()
    {
        return new Vector3(UnityEngine.Random.Range(-17,17), UnityEngine.Random.Range(-9,9), 0);
    }
}
