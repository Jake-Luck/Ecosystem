using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepController : MonoBehaviour
{
    public float moveDelay = 2;
    private Rigidbody rigidBody;

    public float hunger;
    public float thirst;
    public float horniness;
    private int directionX;
    private int directionY;
    private int rotation;
    
    public enum Activity {
        Searching,
        Pursuing,
        Eating,
        Drinking,
        Mating
    }
    public Activity currentActivity = Activity.Searching;

    void Awake() {
        rigidBody = GetComponent<Rigidbody>();
        
        StartCoroutine(searching());
    }
    
    IEnumerator searching() {        
        while(currentActivity == Activity.Searching) {

            float direction = UnityEngine.Random.Range(0, 4);
            switch (direction)
            {
                case 0:
                    directionX = 1;
                    directionY = 0;
                    rotation = 180;
                    break;
                case 1:
                    directionX = 0;
                    directionY = 1;
                    rotation = 90;
                    break;
                case 2:
                    directionX = -1;
                    directionY = 0;
                    rotation = 0;
                    break;
                case 3:
                    directionX = 0;
                    directionY = -1;
                    rotation = -90;
                    break;
                default:
                    Debug.Log("4");
                    break;
            }

            move(directionX, directionY, rotation);

            yield return new WaitForSeconds(moveDelay);
        }
    }

    void move(float x, float y, float rotation) {
        Vector3 moveDirection = new Vector3(x * 100, 100, y * 100);
        rigidBody.AddForce(moveDirection);
        Quaternion tempQuaternion = new Quaternion();
        tempQuaternion.eulerAngles = new Vector3(0f, rotation, 0f);
        rigidBody.transform.rotation = tempQuaternion;
    }
}
