using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class SheepController : MonoBehaviour
{
    public float moveDelay = 2;
    private float lastMoved;
    private Rigidbody rigidBody;

    
    private float thirst = 100;
    private float hunger = 100;
    private float horny = 0;
    public float thirstLimit = 50;
    public float hungerLimit = 50;
    public float hornyLimit = 50;
    
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
        lastMoved = moveDelay;

        StartCoroutine(updateStats());
    }

    private void Update() {
        searching();
        if (thirst < thirstLimit) {
            //search for water
        }
        else if (hunger < hungerLimit) {
            //search for food
        } 
        else if (horny > hornyLimit) {
            //search for mate
        }
        else {
            //search for food
        }
    }

    void searching() {
        if (Time.time - lastMoved > moveDelay) {
            transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
            move();
        }
    }
    void move() {
        Vector3 force = -transform.right;
        force = new Vector3(force.x, 1, force.z);
        rigidBody.AddForce(force * 115);
        lastMoved = Time.time;
    }

    IEnumerator updateStats() {
        while (thirst > 0 && hunger > 0) {
            yield return new WaitForSeconds(1);
            thirst -= 1;
            hunger -= 1;
            horny += 1;
        }

        Destroy(gameObject);
    }
}
