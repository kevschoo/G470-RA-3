using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;
using System;
using JetBrains.Annotations;

public class PlayerLength : NetworkBehaviour
{
    [SerializeField] private GameObject bodyLengthPrefab; 
    
    public NetworkVariable<int> length = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [CanBeNull] public static event System.Action<int> ChangedLengthEvent;

    private List<GameObject> _tails;
    private Transform _lastTail;
    private Collider2D _lastTailCollider;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _tails = new List<GameObject>();
        _lastTail = transform;
        _lastTailCollider = GetComponent<Collider2D>();
        //if we are not the server, notify others that we increased in size
        if(!IsServer) length.OnValueChanged += LengthChangedEvent;

        if (IsOwner) return;
        for (int i = 0; i < length.Value - 1; ++i)
            InstantiateTails();
    }

    //create a tail for the snake, then tell other systems
    private void LengthChangedEvent(int previousValue, int newValue)
    {
        //InstantiateTails();

        LengthChanged();

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        DestroyTails();
    }

    private void DestroyTails()
    {
        while( _tails.Count != 0)
        {
            GameObject tail = _tails[0];
            _tails.RemoveAt(0);
            Destroy(tail);
        }
    }

    //Called only by server
    [ContextMenu("Add Length")]
    public void AddLength()
    {
        length.Value += 1; 

        LengthChanged();
    }

    //tell other systems
    private void LengthChanged()
    {
        InstantiateTails();   
        if(!IsOwner)
        {return;}
        
        ChangedLengthEvent?.Invoke(length.Value);
    }


    private void InstantiateTails()
    {
        GameObject tailGameObject = Instantiate(bodyLengthPrefab, transform.position,quaternion.identity);
        tailGameObject.GetComponent<SpriteRenderer>().sortingOrder = -length.Value ;
        if( tailGameObject.TryGetComponent(out Tail tail))
        {
            tail.networkedOwner = transform;
            tail.followTransform = _lastTail;
            _lastTail = tailGameObject.transform;
            Physics2D.IgnoreCollision(tailGameObject.GetComponent<Collider2D>(),_lastTailCollider);
        }

        _tails.Add(tailGameObject);
    }
}
