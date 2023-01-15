using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tube : MonoBehaviour
{
    public enum Direction
    {
        up,
        down,
        left,
        right
    };
     
    public Direction direction = new Direction();
    public bool isEntrance = false;
    public bool isExit = false;

    private float arrowHeadLength = 0.25f;
    private float offsetDistance = 0.25f;

    private void OnDrawGizmos() {
        Vector2 mainDirection;
        Vector2 endPos;
        Vector2 angleUp;
        Vector2 angleDown;

        switch (direction){
            case Direction.right:
                mainDirection = Vector2.right;
                endPos = new Vector2(transform.position.x + arrowHeadLength, transform.position.y);
                angleUp = new Vector3(-1,1);
                angleDown = new Vector3(-1,-1);
                break;
            case Direction.left:
                mainDirection = Vector2.left;
                endPos = new Vector2(transform.position.x - arrowHeadLength, transform.position.y);
                angleUp = new Vector3(1,1);
                angleDown = new Vector3(1,-1);
                break;
            case Direction.up:
                mainDirection = Vector2.up;
                endPos = new Vector2(transform.position.x, transform.position.y + arrowHeadLength);
                angleUp = new Vector3(-1,-1);
                angleDown = new Vector3(1,-1);
                break;
            default:
                mainDirection = Vector2.down;
                endPos = new Vector2(transform.position.x, transform.position.y - arrowHeadLength);
                angleUp = new Vector3(-1,1);
                angleDown = new Vector3(1,1);
                break;
        }

        Gizmos.DrawRay(transform.position, mainDirection * arrowHeadLength);
        Gizmos.DrawRay(endPos, angleUp * (arrowHeadLength/3));
        Gizmos.DrawRay(endPos, angleDown * (arrowHeadLength/3));
    }
}
