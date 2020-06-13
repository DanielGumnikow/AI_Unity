using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent nm;                                 // create new navmeshagent variable
    public Transform target;                                // create new Transform variable
    public GameObject leader;                               // create new reference for zombie leader
    public GameObject control;                              // create new reference to the swarmcontrol object


    public float distanceThreshold = 20f;                   // set distance threshold for letting the AI move to 20

    public enum AIState { idle, chasing, attack, follow};   // enumeration for the different AI states
    public enum WanderType {Random, Waypoint};              // enumeration for the different Wandertypes 
    public WanderType wanderType = WanderType.Random;       // initialize wanderType
        
    public Transform[] waypoints;                           
    public float wanderRadius = 3f;                         // radius of the sphere in which the AI will move randomly
    private Vector3 wanderPoint;                            // the point where the AI is moving to, when random is activated                       
    private int waypointIndex = 0;                          // the index of the next Index the AI will move to
    private bool isLeader = false;                          // bool for checking if AI is a leaderzombie
    private SpriteRenderer Sprite;                          // spriterenderer for the colored statusicons

    public AIState aiState = AIState.idle;                  // set aiState to the idle state

    public Animator animator;                               // animator component for the animationstates

    public float attackThreshold = 1.5f;                    // distance from the target when the AI will attack
    Player player;                                          // playerscript reference
    RaycastHit hit;                                         // reference to calculated raycast

    /*
     * Initializing references to objects, activate statusicon for the leaderzombie, 
     * calculating the closest waypoints for each AI to set up as the first Waypoint of the path.
     * Setup first wanderpoint, if random wander is enabled, call method for setting up
     * the avoidancepriority for each agent, to prevent them from blocking each other
     * Start the coroutine of thinking of each AI agent
     */
    void Start()
    {
        nm = GetComponent<NavMeshAgent>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        Sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();

        if (isLeader)
        {
            transform.GetChild(1).gameObject.SetActive(true);
        }

        checkClosestWaypoint();
        wanderPoint = RandomWanderPoint();
        AvoiddancePrio();
        StartCoroutine(Think()); //call Think() method
    }


    /*
     * Setting up the avoidancepriority of each agent
     * Every agent gets a random priority to prevent them from blocking each other.
     * The leaderzombie gets the highest priority with 0, so no other agent will block him.
     */
    private void AvoiddancePrio()
    {
        if (gameObject.layer != 9)
        {
            nm.avoidancePriority = Random.Range(40, 60);
        }
        else 
        {
            nm.avoidancePriority = 0;
        }
    }

    /*
     * Return if this gameObject is the actual leaderzombie
     */
    public bool GetLeader()
    {
        return isLeader;
    }

    /*
     *  Sets this gameObject to the leaderzombie.
     */
    public void SetLeader()
    {
        isLeader = true;
    }


    /*
     * Method for calculating the next point the AI will move to,
     * depending on the WanderType, it is either a random point which is close.
     * or one of the Waypoints.
     */
    public void Wander() 
    {
        if (wanderType == WanderType.Random)
        {
            nm.speed = 0.5f;
            animator.SetBool("Chasing", false);
            if (Vector3.Distance(transform.position, wanderPoint) < 2f)
            {

                wanderPoint = RandomWanderPoint();
            }
            else
            {
                nm.SetDestination(wanderPoint);
            }
        }
        else 
        {
            if (waypoints.Length >= 2)
            {
                if (Vector3.Distance(waypoints[waypointIndex].position, transform.position) < 2f)
                {
                    if (waypointIndex == waypoints.Length - 1)
                    {
                        waypointIndex = 0;
                    }
                    else
                    {
                        waypointIndex++;
                    }
                }
                else
                {
                    nm.SetDestination(waypoints[waypointIndex].position);
                }
            }
            else 
            {
                Debug.LogWarning("More than 1 Waypoint is needed, to use the Waypoints for: " + gameObject.name);
            }
        }
    }


    /*
     * Calculate a random close point inside a sphereradius where the AI will 
     * move to when WanderType.random is selected.
     */
    public Vector3 RandomWanderPoint() 
    {
        Vector3 randomPoint = (Random.insideUnitSphere * wanderRadius) + transform.position;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomPoint, out navHit, wanderRadius, -1);
        return new Vector3(navHit.position.x, transform.position.y, navHit.position.z);
    }

    /*
     * Check the closest Waypoint to the AI´s position, to change the waypoint the AI will move to.
     */
    public void checkClosestWaypoint() 
    {
        float distanceClosest = 10000f;
        int closestWaypointIndex = 0;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[i].position);
            if (distanceToWaypoint < distanceClosest)
            {
                distanceClosest = distanceToWaypoint;
                closestWaypointIndex = i;
            }
        }
        waypointIndex = closestWaypointIndex;
    }

    /*
     * This Enumerator has the whole behaviour of the states of the AI.
     * There are 4 cases of AIStates, idle, chasing, attack, follow.
     */
    IEnumerator Think()
    {
        while (true)
        {
            switch (aiState)
            {

                /*
                 * The default state of the AI. StatusIcon = green.
                 * The distance to the player and the leaderzombie will be calculated and if they are in the distanceThreshold, a linecast will be used to determine
                 * if the player/leaderzombie should be visible or is blocked by walls. Zombies can see the player trough other zombies.(Layermask)
                 * The speed of the Agents will be set to 0.5f and the animation of the slow walk is used.
                 * Only 4 zombies can be in the attacking state at the same time.
                 * When there is no player or leaderzombie visible, the Agent starts to wander depending on the Wandertype, in this case the waypoint system.
                 * If the player is visible the AI switches to the AIState.chasing.
                 * If the leaderzombie is visible the AI switches to the AIState.follow.
                 */
                case AIState.idle:
                    float dist = Vector3.Distance(target.position, transform.position);
                    Sprite.color = new Color(0f, 1f, 0f, 1f);
                    nm.speed = 0.5f;
                    if (control.GetComponent<SwarmControl>().leaderExists)
                    {
                        leader = GameObject.FindGameObjectWithTag("Leader");
                        float dist2 = Vector3.Distance(leader.transform.position, transform.position);
                        if (dist2 < distanceThreshold && dist2 > 0.2f) 
                        {
                            if (Physics.Linecast(transform.position, leader.transform.position, out hit, ~(1 << 8)))
                            {
                                if (hit.transform.CompareTag("Leader"))
                                {
                                    nm.speed = 0.5f;
                                    aiState = AIState.follow;
                                    animator.SetBool("Chasing", false);
                                }
                                else
                                {
                                    Wander();
                                }

                            }
                        } 


                    }
                    if (dist < distanceThreshold && !player.playerDown)
                    {
                        if (Physics.Linecast(transform.position, target.position, out hit, ~(1 << 8)))
                        {
                            if (hit.transform.CompareTag("Player") && control.GetComponent<SwarmControl>().attackingZombies < 4 && control.GetComponent<SwarmControl>().chasingZombies < 4)
                            {
                                aiState = AIState.chasing;
                                animator.SetBool("Chasing", true);
                                control.GetComponent<SwarmControl>().chasingZombies += 1;
                            }
                            else
                            {
                                Wander();
                            }
                        }                   
                    }
                    else 
                    {
                        Wander();    
                    }
                    break;

                /*
                 * The chasing state of the AI. Statusicon = Yellow. 
                 * Movement speed is set up to double the amount of the idle state speed (now 1.0f)
                 * The AI starts to move to the players location. If the player has 0 healthpoints left
                 * or the AI cant see him anymore the AI will go back into the idle state.
                 * If the player is in attackrange, the AI will go into the AIState.attack.
                 */
                case AIState.chasing:
                    
                    Sprite.color = new Color(1f, 1f, 0f, 1f);
                    dist = Vector3.Distance(target.position, transform.position);
                    nm.speed = 1f;
                    nm.SetDestination(target.position);

                    if (player.getHealthPoints() <= 0)
                    {
                        aiState = AIState.idle; 
                        animator.SetBool("Chasing", false); 
                        control.GetComponent<SwarmControl>().chasingZombies -= 1;
                    }

                    if (Physics.Linecast(transform.position, target.position, out hit, ~(1 << 8)))
                    {
                        if (!hit.transform.CompareTag("Player"))
                        {
                            aiState = AIState.idle; 
                            animator.SetBool("Chasing", false);
                            control.GetComponent<SwarmControl>().chasingZombies -= 1;
                        }
                    }

                    if (dist > distanceThreshold) 
                    {
                        aiState = AIState.idle; 
                        animator.SetBool("Chasing", false); 
                        control.GetComponent<SwarmControl>().chasingZombies -= 1;
                    }

                    if (dist < attackThreshold && control.GetComponent<SwarmControl>().attackingZombies < 4) 
                    {
                        aiState = AIState.attack;
                        animator.SetBool("Attacking", true);
                        control.GetComponent<SwarmControl>().attackingZombies += 1;
                    }
                    break;
                
                /*
                 *  The attacking state of the AI. Statusicon = red.
                 *  When the player has 0 healthpoints, the AI will check for the closest Waypoint and switch to idle state.
                 *  If the player moves away and is not in attack range anymore, the AI will switch into chasing state.
                 *  The AI damages the player while in attack state.
                 */
                case AIState.attack:

                    Sprite.color = new Color(1f, 0f, 0f, 1f);
                    dist = Vector3.Distance(target.position, transform.position);
                    
                    if (player.getHealthPoints() <= 0)
                    {
                        control.GetComponent<SwarmControl>().attackingZombies -= 1;
                        control.GetComponent<SwarmControl>().chasingZombies -= 1;
                        nm.SetDestination(transform.position);
                        checkClosestWaypoint();
                        wanderType = WanderType.Waypoint;

                        aiState = AIState.idle;
                        animator.SetBool("Attacking", false);
                        animator.SetBool("Chasing", false);
                    }

                    if (dist > attackThreshold)
                    {
                        control.GetComponent<SwarmControl>().attackingZombies -= 1;
                        aiState = AIState.chasing;
                        animator.SetBool("Attacking", false);
                    }
                    player.damageHealthPoints(0.5f);
                    nm.SetDestination(transform.position);    
                    break;
                
                /*
                 * The follow state of the AI. Statusicon = orange.
                 * If the zombieleader is visible but far away, the AI will have a speedboost, 
                 * until beeing close to the leaderzombie
                 * When the leaderzombie is not visible or to far away, the AI will switch into idle state.
                 * Same goes for the player.
                 * If the player is visible while following the leaderzombie, the AI will switch into the
                 * chasing state.
                 */
                case AIState.follow:
                    Sprite.color = new Color(1f, 0.65f, 0f, 1f);
                    dist = Vector3.Distance(target.position, transform.position);
                    float distLeader = Vector3.Distance(leader.transform.position, transform.position);

                    if (distLeader > 3)
                    {
                        nm.speed = 1f;
                    }
                    else
                    {
                        nm.speed = 0.5f;
                    }

                    if (Physics.Linecast(transform.position, leader.transform.position, out hit, ~(1 << 8)))
                    {
                        if (!hit.transform.CompareTag("Leader"))
                        {
                            nm.speed = 0.5f;
                            aiState = AIState.idle;
                            animator.SetBool("Chasing", false);
                        }
                        else 
                        {
                            nm.SetDestination(leader.transform.position);
                        }
                    }

                        if (distLeader > distanceThreshold)
                        {
                            aiState = AIState.idle;
                            animator.SetBool("Chasing", false);
                        }

                    if (dist < distanceThreshold)
                    {
                        if (Physics.Linecast(transform.position, target.position, out hit, ~(1 << 8)))
                        {
                            if (hit.transform.CompareTag("Player") && control.GetComponent<SwarmControl>().chasingZombies < 4)
                            {
                                aiState = AIState.chasing;
                                animator.SetBool("Chasing", true);
                            }
                            else
                            {
                                Wander();
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            
            yield return new WaitForSeconds(0.2f); //delay 0.2 second when destination changes
        }
    }
}
