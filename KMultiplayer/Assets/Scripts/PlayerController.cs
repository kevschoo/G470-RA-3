using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using JetBrains.Annotations;

public class PlayerController : NetworkBehaviour
{

    private Camera mCam;
    private Vector3 mouseInput = Vector3.zero;
    [SerializeField] private float speed = 5;
    private PlayerLength _playerLength;
    private bool _canCollide = true;

    [CanBeNull]public static event System.Action GameOverEvent;

    private readonly ulong[] _targetClientsArray = new ulong[1];

    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        mCam = Camera.main;    
        _playerLength = GetComponent<PlayerLength>();
    }

    void Update()
    {
        if(!IsOwner || !Application.isFocused)
        {
            return;
        }
         MovePlayerServer();

    }

    private void MovePlayerServer()
    {
        Vector2 mousePositionScreen = (Vector2)Input.mousePosition;
        mouseInput.x = mousePositionScreen.x;
        mouseInput.y = mousePositionScreen.y;
        mouseInput.z = mCam.nearClipPlane;
        
        Vector3 mouseWorldCoordinates = mCam.ScreenToWorldPoint(mouseInput);
        mouseWorldCoordinates.z = 0;
        MovePlayerServerRpc(mouseWorldCoordinates);
    }
    
    [ServerRpc]
    private void MovePlayerServerRpc(Vector3 mouseWorldCoordinates)
    {
        
        transform.position = Vector3.MoveTowards(transform.position, mouseWorldCoordinates, Time.deltaTime * speed);

        if(mouseWorldCoordinates != transform.position)
        {
            Vector3 targetDir = mouseWorldCoordinates - transform.position;
            targetDir.z = 0;
            transform.up = targetDir;
        }
    }

    private void MovePlayerClient()
    {
        Vector2 mousePositionScreen = (Vector2)Input.mousePosition;
        mouseInput.x = mousePositionScreen.x;
        mouseInput.y = mousePositionScreen.y;
        mouseInput.z = mCam.nearClipPlane;
        
        Vector3 mouseWorldCoordinates = mCam.ScreenToWorldPoint(mouseInput);
        mouseWorldCoordinates.z = 0;
        transform.position = Vector3.MoveTowards(transform.position, mouseWorldCoordinates, Time.deltaTime * speed);

        if(mouseWorldCoordinates != transform.position)
        {
            Vector3 targetDir = mouseWorldCoordinates - transform.position;
            targetDir.z = 0;
            transform.up = targetDir;
        }
    }


    private IEnumerator CollisionCheckCoroutine()
    {
        _canCollide = false;
        yield return new WaitForSeconds(0.5f);
        _canCollide = true;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("player collided");
        if(!col.gameObject.CompareTag("Player"))
        {
            return;
        }
        if(!IsOwner)
        {
            return;
        }
        if(_canCollide == false)
        {
            return;
        }
        StartCoroutine(CollisionCheckCoroutine());
        
        if(col.gameObject.TryGetComponent(out PlayerLength length))
        {
            var player1 = new PlayerData()
            {
                id = OwnerClientId,
                length = _playerLength.length.Value
            };
            var player2 = new PlayerData()
            {
                id = length.OwnerClientId,
                length = length.length.Value
            };
            DeterminedCollisionWinnerServerRpc(player1, player2);
        }
        else if (col.gameObject.TryGetComponent(out Tail tail))
        {
            Debug.Log("Tail Col");
            WinInformationServerRpc(tail.networkedOwner.GetComponent<PlayerController>().OwnerClientId, OwnerClientId);
        }

    }

    [ServerRpc]
    private void DeterminedCollisionWinnerServerRpc(PlayerData playerData1, PlayerData playerData2)
    {
        if(playerData1.length > playerData2.length)
        {
            WinInformationServerRpc(playerData1.id,playerData2.id);
        }
        else
        {
            WinInformationServerRpc(playerData2.id,playerData1.id);
        }
    }

    [ServerRpc]
    private void WinInformationServerRpc(ulong winner, ulong loser)
    {
        _targetClientsArray[0] = winner;
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams 
            {
                TargetClientIds = _targetClientsArray
            }
        };
        AtePlayerClientRpc(clientRpcParams);

        _targetClientsArray[0] = loser;
        clientRpcParams.Send.TargetClientIds = _targetClientsArray;
        GameOverClientRpc(clientRpcParams);

    }

    [ClientRpc]
    private void AtePlayerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("You ate them");
        if(!IsOwner)
        {
            return;
        }

    }

    [ClientRpc]
    private void GameOverClientRpc(ClientRpcParams clientRpcParams = default)
    {
   
        if(!IsOwner)
        {
            return;
        }
        Debug.Log("You lose");
        GameOverEvent?.Invoke();
        NetworkManager.Singleton.Shutdown();
    }

    struct PlayerData : INetworkSerializable
    {
        public ulong id;
        public int length;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref length);
            
        }
    }
}
