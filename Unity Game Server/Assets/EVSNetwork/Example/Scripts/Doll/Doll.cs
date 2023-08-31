using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doll : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private Transform[] _waypoints;

    private Vector3 _targetPos;
    private float _targetDistance;
    private int _waypointIndex = 0;

    private void Update()
    {
        _targetDistance = Vector3.Distance(transform.position, _targetPos);

        if (_targetDistance > 0.2f)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, Time.deltaTime * _speed);
        }
        else
        {
            _waypointIndex = _waypointIndex == _waypoints.Length - 1 ? 0 : _waypointIndex + 1;
            _targetPos = _waypoints[_waypointIndex].position;
        }
    }
}
