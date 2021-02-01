using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SheepController : MonoBehaviour
{
    private Rigidbody rigidBody;
    
    private float moveDelay = 2;
    private float lastMoved;

    // Below lower limit will prioritize, above upper limit won't at all (reversed for reproductive urge).
    public int thirst = 80;
    public int hunger = 80;
    public int reproductiveUrge = 0;
    private int thirstLowerLimit = 40;
    private int hungerLowerLimit = 40;
    private int reproductiveUrgeLowerLimit = 2000;
    private int thirstUpperLimit = 60;
    private int hungerUpperLimit = 60;
    private int reproductiveUrgeUpperLimit = 6000; 
    private const int StatRestoration = 10;
    // Any numbers in the thousands has been accidentally left over from testing

    private int directionX;
    private int directionY;
    private int rotation;

    private static int sightRange = 5;
    private List<GameObject> inSightRange = new List<GameObject>();

    private float interactRange = 2;

    public enum SearchingFor
    {
        Water,
        Food,
        Mate,
        Nothing
    }
    public SearchingFor searchingFor = SearchingFor.Food;

    private bool hasTarget;
    public GameObject currentTarget;

    private void Awake() {
        rigidBody = GetComponent<Rigidbody>();
        lastMoved = moveDelay;

        StartCoroutine(updateStats()); // Will update the stats each second until death
        StartCoroutine(visionHandler()); // Will look for a target after each movement until one is found
    }

    private void Update() {
        // If ready to move, continue with function
        if (!(Time.time - lastMoved > moveDelay)) return;
        
        if (hasTarget) {
            // If target is close enough to interact with... then interact with it
            if (Vector3.Distance(gameObject.transform.position, currentTarget.transform.position) < interactRange) {
                switch (searchingFor) {
                    case SearchingFor.Water:
                        drink();
                        break;
                    case SearchingFor.Food:
                        eat();
                        break;
                    case SearchingFor.Mate:
                        mate();
                        break;
                }
            }
            else {
                // Rotates to face target then moves towards it
                var tempQuaternion = Quaternion
                    .LookRotation(currentTarget.transform.position - gameObject.transform.position).eulerAngles;
                transform.eulerAngles = new Vector3(tempQuaternion.x, tempQuaternion.y + 90, tempQuaternion.z);
                move();
            }
        }
        // Sets what to look for
        // Could probably do with some refactoring, this is super messy
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
            else {
                searchingFor = SearchingFor.Nothing;
                // 50% chance of doing nothing until next movement cycle
                if (Random.Range(0,2) == 1) {
                    lastMoved = Time.time;
                    return;
                }
            }

            // Rotates to face random direction then moves forward
            transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
            move();
            
        }
            
        lastMoved = Time.time;
    }
    
    private void move() {
        // Simply jumps the sheep forwards
        var force = -transform.right;
        force = new Vector3(force.x, 1.4f, force.z);
        rigidBody.AddForce(force * 115);
    }

    private void updateVision() {
        // Fills a list with every nearby object
        inSightRange.Clear();
        var collidersInSightRange = Physics.OverlapSphere(gameObject.transform.position, sightRange).OrderBy(collider =>
            Vector3.Distance(gameObject.transform.position, collider.transform.position));
        foreach (var collider in collidersInSightRange) inSightRange.Add(collider.gameObject);
    }

    private void selectTarget() {
        updateVision();
        
        // Checks if there are any visible objects that suit the search criteria, if so sets that object as the target.
        foreach (var thing in inSightRange) {
            if (thing.CompareTag("Terrain")) {
                var noiseLevel = thing.GetComponent<TileController>().noiseLevel;
                
                if (noiseLevel < 0.5 && searchingFor == SearchingFor.Water) {
                    currentTarget = thing;
                    hasTarget = true;
                    break;
                } 
                if (0.5 <= noiseLevel && noiseLevel < 0.6 && searchingFor == SearchingFor.Food) {
                    currentTarget = thing;
                    hasTarget = true;
                    break;
                }
            }

            if (thing.CompareTag("Creature") && searchingFor == SearchingFor.Mate) {
                currentTarget = thing;
                hasTarget = true;
                break;
            }
        }
    }
    
    private void drink() {
        // Increases the sheep's thirst stat. If the sheep is full, it will stop drinking
        thirst += StatRestoration;
        if (thirst > 91) {
            hasTarget = false;
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
        if (tileScript.noiseLevel > 0.6 || hunger > 81) {
            hasTarget = false;
        }
    }

    private void mate() {
        
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

    private IEnumerator visionHandler() {
        // Updates the sheep's vision once per movement.
        while (thirst > 0 && hunger > 0)
        {
            yield return new WaitForSeconds(2 * moveDelay);

            if (!hasTarget) selectTarget();
        }
    }
}
