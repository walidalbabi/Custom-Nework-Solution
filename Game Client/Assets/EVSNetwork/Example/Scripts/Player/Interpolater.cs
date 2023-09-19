using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Interpolation
{
    public int tick;
    public Vector3 position;
    public Quaternion rotation;
}
public class Interpolater : MonoBehaviour
{

    [SerializeField] private float _lerpSpeed = 0.1f;

    [SerializeField] private Queue<Interpolation> stateQueue = new Queue<Interpolation>();
    private Interpolation currentState;
    private Quaternion rotTarget;

    private void Awake()
    {
        transform.parent = null;
    }

    private void Update()
    {
        Quaternion lerpRot = Quaternion.SlerpUnclamped(transform.rotation, rotTarget, _lerpSpeed * 2);
        transform.rotation = lerpRot;

        if (currentState.tick == 0)
            if (stateQueue.Count > 0)
                currentState = stateQueue.Dequeue();

        while (currentState.tick != 0)
        {
            Vector3 lerpPos = Vector3.LerpUnclamped(transform.position, currentState.position, _lerpSpeed);
            transform.position = lerpPos;

            currentState.tick = 0;
        }
    }

    private Interpolation previouseSetted;
    public void SetInterpolation(Interpolation state)
    {
        rotTarget = state.rotation;

        if (previouseSetted.tick == state.tick) return;

        if (stateQueue.Count > 5) // max queue size, for example
            stateQueue.Dequeue();

        stateQueue.Enqueue(state);
        previouseSetted = state;
    }

}
