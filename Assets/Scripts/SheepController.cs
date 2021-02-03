using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SheepController : MonoBehaviour
{
    private Rigidbody rigidBody;
    
    private float actionDelay = 2;
    private float lastActed;

    // Below lower limit will prioritize, above upper limit won't at all (reversed for reproductive urge).
    [Range(0,100)] public int thirst = 80;
    [Range(0,100)] public int hunger = 80;
    [Range(0,100)] public int reproductiveUrge = 0;
    private int thirstLowerLimit = 40;
    private int hungerLowerLimit = 40;
    private int reproductiveUrgeLowerLimit = 20;
    private int thirstUpperLimit = 60;
    private int hungerUpperLimit = 60;
    private int reproductiveUrgeUpperLimit = 60; 
    private const int StatRestoration = 10;
    // Any numbers in the thousands has been accidentally left over from testing

    
    
    private const int sightRange = 10;
    private readonly List<GameObject> inSightRange = new List<GameObject>();
    private const float interactRange = 2;

    public enum SearchingFor
    {
        Water,
        Food,
        Mate,
        Nothing
    }
    public SearchingFor searchingFor = SearchingFor.Food;

    public enum Currently
    {
        Eating,
        Drinking,
        Mating,
        DoingNothing
    }

    public Currently currently = Currently.DoingNothing;
    public bool hasAction;
    public bool hasTarget;
    public GameObject currentTarget;

    private void Awake() {
        rigidBody = GetComponent<Rigidbody>();
        lastActed = actionDelay;

        StartCoroutine(updateStats()); // Will update the stats each second until death
    }

    private void Update() {
        // If ready to move, continue with update
        if (!(Time.time - lastActed > actionDelay)) return;
        
        updateVision();
        // If target is no longer valid, or is searching for a target, try and find one
        if (!hasTarget) {
            selectTarget();
        }
        // If currently attempting to perform an action
        if (hasAction) {
            if (hasTarget) {
                // If target is close enough to perform said action
                if (Vector3.Distance(gameObject.transform.position, currentTarget.transform.position) < interactRange) {
                    switch (currently) {
                        case Currently.Drinking:
                            drink();
                            break;
                        case Currently.Eating:
                            eat();
                            break;
                        case Currently.Mating:
                            mate();
                            break;
                        case Currently.DoingNothing:
                            break;
                    }
                }
                else {
                    // If target isn't close enough to perform said action then move towards it
                    var tempQuaternion = Quaternion
                        .LookRotation(currentTarget.transform.position - gameObject.transform.position).eulerAngles;
                    transform.eulerAngles = new Vector3(tempQuaternion.x, tempQuaternion.y + 90, tempQuaternion.z);
                    move();
                }
            }
            else {
                // Unable to find a target for this action, so has to go back to searching
                hasAction = false;
            }
        }
        else {
            if (thirst < thirstLowerLimit) {
                searchingFor = SearchingFor.Water;
            }
            else if (hunger < hungerLowerLimit) {
                searchingFor = SearchingFor.Food;
            }
            else if (reproductiveUrge > reproductiveUrgeUpperLimit) {
                searchingFor = SearchingFor.Mate;
            }
            else if (thirst < thirstUpperLimit) {
                searchingFor = SearchingFor.Water;
            }
            else if (hunger < hungerUpperLimit) {
                searchingFor = SearchingFor.Food;
            }
            else if (reproductiveUrge > reproductiveUrgeLowerLimit) {
                searchingFor = SearchingFor.Mate;
            }
            
            transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
            move();
        }
        
        lastActed = Time.time;
    }

    private void move() {
        // Simply jumps the sheep forwards
        var force = -transform.right;
        force = new Vector3(force.x, 1.4f, force.z);
        rigidBody.AddForce(force * 115);
    }
    
    private void drink() {
        // Increases the sheep's thirst stat. If the sheep is full, it will stop drinking
        thirst += StatRestoration;
        if (thirst > 91) {
            hasAction = false;
            hasTarget = false;
            searchingFor = SearchingFor.Nothing;
        }
    }

    private void eat() {
        // Increases the sheep's hunger stat and reduces the grass available on a tile. 
        var tileScript = currentTarget.GetComponent<TileController>();

        if (tileScript.noiseLevel < 0.55) {
            hunger += StatRestoration * 2;
            tileScript.updateTile();
        }
        else {
            hunger += StatRestoration;
            tileScript.updateTile();
        }
        
        // If the tile runs out of grass, or the sheep is full, it stops eating
        if (hunger > 0.81) {
            hasAction = false;
            hasTarget = false;
            searchingFor = SearchingFor.Nothing;
        }
        else if (tileScript.noiseLevel > 0.6) {
            hasTarget = false;
        }
    }

    private void mate() {
        
    }
    
    private void updateVision() {
        // Fills a list with every nearby object
        inSightRange.Clear();
        var collidersInSightRange = Physics.OverlapSphere(gameObject.transform.position, sightRange).OrderBy(collider =>
            Vector3.Distance(gameObject.transform.position, collider.transform.position));
        foreach (var collider in collidersInSightRange) inSightRange.Add(collider.gameObject);
    }

    private void selectTarget() {
        // Checks if there are any visible objects that suit the search criteria, if so sets that object as the target.
        foreach (var thing in inSightRange) {
            if (thing.CompareTag("Terrain")) {
                var noiseLevel = thing.GetComponent<TileController>().noiseLevel;
                
                if (noiseLevel < 0.5 && searchingFor == SearchingFor.Water) {
                    currentTarget = thing;
                    hasTarget = true;
                    currently = Currently.Drinking;
                    hasAction = true;
                    break;
                } 
                if (0.5 <= noiseLevel && noiseLevel < 0.6 && searchingFor == SearchingFor.Food) {
                    currentTarget = thing;
                    hasTarget = true;
                    currently = Currently.Eating;
                    hasAction = true;
                    break;
                }
            }

            if (thing.CompareTag("Creature") && searchingFor == SearchingFor.Mate) {
                currentTarget = thing;
                hasTarget = true;
                currently = Currently.Mating;
                hasAction = true;
                break;
            }
        }
    }
    
    private IEnumerator updateStats() {
        // Reduces thirst and hunger, and increases urgeToMate, by 1 each second
        while (thirst > 0 && hunger > 0) {
            yield return new WaitForSeconds(1);
            thirst -= 1;
            hunger -= 1;
            reproductiveUrge += 1;
        }
        
        // When thirst or hunger drops to 0, the sheep dies.
        Destroy(gameObject);
    }
}
