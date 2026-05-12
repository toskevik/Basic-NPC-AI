using UnityEngine;
using UnityEngine.Serialization;

public class NPCController : MonoBehaviour
{
    // Animator parameters - Allows us to set the parameters in the Animator Controller without referring to them by string
    private static readonly int IsIdle = Animator.StringToHash("isIdle");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsChasing = Animator.StringToHash("isChasing");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    
    // Constants
    private const float AccuracyWp = 1f; // Accuracy for the NPC to reach the waypoint

    // Inspector variables
    [Header("NPC Settings")]
    [Tooltip("Show Gizmo for debugging purposes")]
    [SerializeField] private bool bShowGizmo;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform npcHead;
    [Tooltip("Waypoints for the NPC to follow")]
    [SerializeField] private Transform[] wayPoint;
    [Tooltip("Field of view angle for the NPC")]
    [Range(1f, 179f)]
    [SerializeField] private float fieldOfViewAngle = 120f;
    [Tooltip("Walk speed for the NPC")]
    [Range(0.1f, 5f)]
    [SerializeField] private float walkSpeed = 0.6f;
    [Tooltip("Run speed for the NPC")]
    [Range(0.1f, 5f)]
    [SerializeField] private float runSpeed = 1.5f;
    [Tooltip("Rotation speed for the NPC")]
    [Range(0.1f, 5)]
    [SerializeField] private float rotationSpeed = 1.2f;
    [Tooltip("Chase distance for the NPC")]
    [Range(1f, 50f)]
    [SerializeField] private float chaseDistance = 5f;
    [Tooltip("Attack distance for the NPC")]
    [Range(1f, 10f)]
    [SerializeField] private float attackDistance = 1.5f;

    // Private variables for the NPC's internal state handling
    private Animator _anim;
    private int _currentWp;
    private float _speed;
    private float fieldOfViewHalfAngle;

// Enum for selecting NPC behavior pattern
    private enum NpcType {
        Normal, Agressive, Persistent, Commander
    }

    private enum State
    {
        Patrol, Chase, Attack, Dead
    }
    
    [Tooltip("Current state of the NPC")]
    [SerializeField]
    private State currentState = State.Patrol;
    
    // [Tooltip("Type of NPC")]
    // [SerializeField]
    // private NpcType _npcType = NpcType.Normal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _anim = GetComponent<Animator>();
        _speed = walkSpeed;
        fieldOfViewHalfAngle = fieldOfViewAngle * 0.5f;
    }

    // Update is called once per frame
    private void Update()
    {
        // Calculate the direction to the player
        var direction = playerTransform.position - transform.position;
        direction.y = 0f;

        // Calculate the angle between the NPC's forward vector and the direction to the player
        var angleToPlayer = Vector3.Angle(direction, npcHead.up);

        // If the NPC is patrolling, keep moving towards the current waypoint
        if (currentState==State.Patrol && wayPoint.Length > 0)
        {
            // Ensure the NPC's animation state is set to walking
            _anim.SetBool(IsIdle, false);
            _anim.SetBool(IsWalking, true);

            // If the NPC is close to the current waypoint, switch to the next waypoint
            // If the NPC has reached the last waypoint, reset to the first waypoint
            if (Vector3.Distance(wayPoint[_currentWp].position, transform.position) < AccuracyWp)
            {
                _currentWp++;
                if (_currentWp >= wayPoint.Length)
                    _currentWp = 0;
            }
        
            // Calculate the direction to the current waypoint
            direction = wayPoint[_currentWp].position - transform.position;
            direction.y = 0f;

            // Rotate and move towards the current waypoint
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            transform.Translate(0f, 0f, Time.deltaTime * _speed, Space.Self);
        }

        // If the NPC can see the player, chase the player, setting the current state of the NPC to chase
        // If the NPC is close enough to the player, attack the player, setting the current state of the NPC to attack
        if (Vector3.Distance(playerTransform.position, transform.position) < chaseDistance 
            && (angleToPlayer < fieldOfViewHalfAngle || currentState == State.Chase || currentState == State.Attack))
        {
            // If the NPC is chasing the player, set the speed to run speed
            // Tuning the NPC to be more persistent in chasing the player,
            // even if the player goes out of the NPC's field of view,
            // as long as the player is within the chase distance
            currentState = State.Chase;
            _speed = runSpeed;

            // Rotate and move towards the player
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            if (direction.magnitude > attackDistance)
            {
                // If the NPC is not close enough to the player, move towards the player
                transform.Translate(0f, 0f, Time.deltaTime * _speed);
                _anim.SetBool(IsWalking, false);
                _anim.SetBool(IsChasing, true);
                _anim.SetBool(IsAttacking, false);
            } else // If the NPC is close enough to the player, attack the player
            {
                currentState = State.Attack;
                _anim.SetBool(IsWalking, false);
                _anim.SetBool(IsChasing, false);
                _anim.SetBool(IsAttacking, true);
            }
        } else // If the NPC cannot see the player, meaning it is too far away or not within the NPC's field of view, set the current state to patrol and reduce speed to walking speed
        {
            currentState = State.Patrol;
            _speed = walkSpeed;
            _anim.SetBool(IsWalking, true);
            _anim.SetBool(IsChasing, false);
            _anim.SetBool(IsAttacking, false);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a (green) line from the NPC to the current waypoint if patrolling
        // Draw a (red) line from the NPC to the player if chasing or attacking
        if (!bShowGizmo) return;
        Vector3 direction;
        if (currentState == State.Patrol)
        {
            Gizmos.color = Color.green;
            direction = wayPoint[_currentWp].position - transform.position;
        }
        else
        {
            Gizmos.color = Color.red;
            direction = playerTransform.position - transform.position;
        }
        Gizmos.DrawLine(transform.position, transform.position + direction);
    }
}
