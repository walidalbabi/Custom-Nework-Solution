using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KCCController : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _axisSpeed;

    [Header("Physics")]
    [SerializeField] private float _gravity;
    [SerializeField] private LayerMask _discludePlayer;

    [Header("Surface Control")]
    [SerializeField] private Vector3 _sensorLocal;
    [SerializeField] private float _surfaceSlideSpeed;
    [SerializeField] private float _slopeClimbSpeed;
    [SerializeField] private float _slopeDecendSpeed;

    private bool _isGrounded;
    private float _curGrav;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SimpleMove();
        FinalMovement();
    }

    private float _jumpHeight;
    private Vector3 _moveVector;

    private void SimpleMove()
    {
        _moveVector = CollisionSlopeCheck(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * 5f);
    }
    private void FinalMovement()
    {
        Vector3 movements = new Vector3(_moveVector.x, _curGrav + _jumpHeight, _moveVector.z) * _moveSpeed;
        movements = transform.TransformDirection(movements);
        transform.position += movements;
    }
    private Vector3 CollisionSlopeCheck(Vector3 dir)
    {
        Vector3 d = transform.TransformDirection(dir);
        Vector3 l = transform.TransformPoint(_sensorLocal);

        Ray ray = new Ray(l, d);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 2, _discludePlayer))
        {
            if (hit.distance <= 0.7f)
            {
                Debug.DrawLine(l, hit.point, Color.yellow, 0.2f);

                Vector3 temp = Vector3.Cross(hit.normal, d);
                Debug.DrawRay(hit.point, temp * 20, Color.green, 0.2f);

                Vector3 myDirection = Vector3.Cross(temp, hit.normal);
                Debug.DrawRay(hit.point, myDirection * 20f, Color.red, 0.2f);

                Vector3 dir2 = myDirection * _surfaceSlideSpeed * _moveSpeed * _axisSpeed;

                RaycastHit raycastCheck = WallCheckDetails(dir2);
                if (raycastCheck.transform != null)
                {
                    dir2 *= raycastCheck.distance * 0.5f;
                }
                transform.position += dir2;
                return Vector3.zero;
            }
            else return dir;
        }

        return dir;
    }
    private RaycastHit WallCheckDetails(Vector3 direction)
    {
        Vector3 l = transform.TransformPoint(_sensorLocal);

        Ray ray = new Ray(l, direction);
                RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 2, _discludePlayer))
        {
            return hit;
        }
        return hit;
    }
    private void OnDrawGizmos()
    {
        Vector3 l = transform.TransformPoint(_sensorLocal);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(l, .2f);
    }
}
