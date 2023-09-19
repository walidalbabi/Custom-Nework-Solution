using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct PositionData
{
    public int tick;
    public Vector3 position;
    public void Init(int tick, Vector3 position)
    {
        this.tick = tick;
        this.position = position;
    }
}

public struct RotationData
{
    public int tick;
    public Quaternion rotation;

    public void Init(int tick, Quaternion rotation)
    {
        this.tick = tick;
        this.rotation = rotation;
    }
}

[System.Serializable]
public struct PlayerSnapshot
{
    public int tick;
    public Vector3 position;
    public Quaternion rotation;

    public void Init(int tick, Vector3 position, Quaternion rotation)
    {
        this.tick = tick;
        this.position = position;
        this.rotation = rotation;
    }
}


[RequireComponent(typeof(NetworkedObject))]
public class NetworkTransform : NetworkBehaviour
{

    private float ViewInterpolationDelayTicks =  0.1f; // 100 milliseconds
    private float ExtrapolationLimitTicks = 0.25f; // 250 milliseconds

    private Queue<PlayerSnapshot> snapshotQueue = new Queue<PlayerSnapshot>();
    private int lastSnapshotTick = -1;

    private TimeManager timeManager;

    [SerializeField] PlayerSnapshot previousSnapshot;
    [SerializeField] PlayerSnapshot nextSnapshot;
    private bool isMoving = false;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    private void Start()
    {
        timeManager = NetworkManager.instance.timeManager;

        ViewInterpolationDelayTicks *= TimeManager.TICK_RATE;
        ExtrapolationLimitTicks *= TimeManager.TICK_RATE;
    }

    protected override void OnNewObjectAdded(object obj)
    {

        if (obj.GetType() == typeof(PlayerSnapshot))
        {
            PlayerSnapshot snap = (PlayerSnapshot)obj;

            ReceiveSnapshot(snap);
        }

    }

    private void Update()
    {
        if (!isMoving) return;

        int currentTick = timeManager.currentTick - (int)ViewInterpolationDelayTicks;

        // Ensure we're not interpolating between the same snapshot
        if (previousSnapshot.tick == nextSnapshot.tick)
            return;

        float lerpFactor = (float)(currentTick - previousSnapshot.tick) / (nextSnapshot.tick - previousSnapshot.tick);
        lerpFactor = Mathf.Clamp01(lerpFactor);

        transform.position = Vector3.Lerp(previousSnapshot.position, nextSnapshot.position, lerpFactor);
        transform.rotation = Quaternion.Slerp(previousSnapshot.rotation, nextSnapshot.rotation, lerpFactor);

        if (Vector3.Distance(transform.position, nextSnapshot.position) < 0.01f)
        {
            transform.position = nextSnapshot.position;
            transform.rotation = nextSnapshot.rotation;
            NextTarget();
        }


    }

    private void NextTarget()
    {
        if (snapshotQueue.Count > 0)
        {
            previousSnapshot = nextSnapshot;
            nextSnapshot = snapshotQueue.Dequeue();
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    public void ReceiveSnapshot(PlayerSnapshot newSnapshot)
    {
        // Make sure you're not adding out-of-order snapshots
        if (lastSnapshotTick >= newSnapshot.tick)
            return;

        lastSnapshotTick = newSnapshot.tick;
      
        // Trim the oldest snapshot if there are too many
        while (snapshotQueue.Count >= 3)
            snapshotQueue.Dequeue();

        snapshotQueue.Enqueue(newSnapshot);

        if (!isMoving)
            NextTarget();
    }
}


