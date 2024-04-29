using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.BossRoom.Infrastructure;

public class Food : NetworkBehaviour
{

    [SerializeField]public GameObject prefab;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player"))
        {
            return;
        }

        if(!NetworkManager.Singleton.IsServer) 
        {
            return;
        }

        if(col.TryGetComponent(out PlayerLength playerLength))
        {
            playerLength.AddLength();
        }
        //NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
        NetworkObject.Despawn();
    }

}
