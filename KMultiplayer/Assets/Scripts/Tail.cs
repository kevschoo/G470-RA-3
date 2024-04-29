using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tail : MonoBehaviour
{

    public Transform followTransform;
    public Transform networkedOwner;

    [SerializeField] private float delayTime = .1f;
    [SerializeField] private float distance = .3f;
    [SerializeField] private float moveStep = 10f;

    
    private Vector3 _targetPos;

    private void Update()
    {
        _targetPos = followTransform.position - followTransform.forward * distance;
        _targetPos += (transform.position - _targetPos) * delayTime;
        _targetPos.z = 0f;

        transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * moveStep);

    }

}
