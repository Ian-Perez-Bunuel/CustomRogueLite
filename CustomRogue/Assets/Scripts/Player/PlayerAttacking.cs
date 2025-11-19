using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttacking : MonoBehaviour
{
    private PlayerStateManager stateManager;
    private Rigidbody rb;

    public Transform facing;

    [Header("Attack")]
    public InputActionReference attack;
    [SerializeField] Terraformer attackTerraformer;
    private bool canAttack = true;
    public float attackCooldown;
    [Header("Attack Raycast")]
    public float attackRange;

    [Header("Slam")]
    public InputActionReference slam;
    public float slamPower;
    [SerializeField] Terraformer slamTerraformer;
    [Header("GroundPound")]
    public float groundPoundPower;
    public float groundPoundDelayAmount;
    private bool groundPoundActive = false;
    public GameObject tempCrater;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stateManager = GetComponent<PlayerStateManager>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canAttack && attack.action.IsPressed())
        {
            Attack();
        }

        if (slam.action.WasPressedThisFrame())
        {
            if (stateManager.grounded)
            {
                Slam();
            }
            else
            {
                GroundPound();
            }
        }
    }

    private void FixedUpdate()
    {
        if (groundPoundActive)
        {
            // Add Impulse downwards
            rb.AddForce(Vector3.down * groundPoundPower, ForceMode.Impulse);

            if (stateManager.grounded)
            {
                slamTerraformer.gameObject.SetActive(true);

                groundPoundActive = false;
            }
        }
    }

    private void Attack()
    {
        Vector3 origin = transform.position;
        Vector3 direction = facing.forward;

        // Raycast in direction facing
        if (Physics.Raycast(origin, direction, out RaycastHit hit, attackRange))
        {
            // Hit something: place the attackTerraformer at the hit point
            attackTerraformer.transform.position = hit.point;

            // Activate the object
            attackTerraformer.gameObject.SetActive(true);
        }
        else
        {
            // No hit - Place on player
            attackTerraformer.transform.position = transform.position;
        }

        // Attack cooldown
        StartCoroutine(AttackCooldown());
    }
    private void Slam()
    {
        Vector3 impulseDir;

        // Spawn crater
        slamTerraformer.gameObject.SetActive(true);

        if (stateManager.OnSlope())
        {
            impulseDir = stateManager.raycastHit.normal * slamPower;
        }
        else
        {
            impulseDir = new Vector3(0.0f, slamPower, 0.0f);
        }
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(impulseDir, ForceMode.Impulse);
    }
    private void GroundPound()
    {
        StartCoroutine(GroundPoundDelay());
    }

    IEnumerator GroundPoundDelay()
    {
        yield return new WaitForSeconds(groundPoundDelayAmount);

        groundPoundActive = true;
    }
    
    IEnumerator AttackCooldown()
    {
        canAttack = false;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }
}
