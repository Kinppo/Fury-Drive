using System;
using PathCreation;
using UnityEngine;

public class AICarController : MonoBehaviour
{
    public float speed;
    public Vector3 pathOffset;
    public float positionInRow;
    public PathCreator pathCreator;

    [HideInInspector] public float _dstTravelled;
    [HideInInspector] public Vector3 currentSpeed;
    [HideInInspector] public Vector3 destination;

    private void Awake()
    {
        pathCreator.bezierPath.GlobalNormalsAngle = 45;
        _dstTravelled = positionInRow;
    }

    public void MoveCar()
    {
        _dstTravelled += speed * Time.deltaTime;
        destination = pathCreator.path.GetPointAtDistance(_dstTravelled, EndOfPathInstruction.Stop) + pathOffset;
    
        if (_dstTravelled < 0)
            if (transform.eulerAngles.y == 0)
                destination += new Vector3(0, 0, _dstTravelled);
            else if (transform.eulerAngles.y == 180)
                destination += new Vector3(0, 0, -_dstTravelled);
            else if (transform.eulerAngles.y == 270)
                destination += new Vector3(-_dstTravelled, 0, 0);
            else destination += new Vector3(_dstTravelled, 0, 0);

        currentSpeed = (destination - transform.position).normalized * speed;
        transform.position = destination;

        if (_dstTravelled >= 0)
        {
            transform.rotation = pathCreator.path.GetRotationAtDistance(_dstTravelled, EndOfPathInstruction.Stop);
            transform.eulerAngles =
                new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
        }
    }
}

// Vector3 distance = (obj1.position - obj2.position).magnitude;