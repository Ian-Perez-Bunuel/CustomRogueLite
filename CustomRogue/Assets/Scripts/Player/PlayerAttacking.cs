using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttacking : MonoBehaviour
{

    private PlayerStateManager stateManager;
    private Rigidbody rb;

    [Header("Attack")]
    public InputActionReference attack;
    private bool canAttack = true;

    [Header("Slam")]
    public InputActionReference slam;
    public float slamPower;
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
        if (stateManager.grounded && groundPoundActive)
        {
            // Temp
            Vector3 feetTransform = new Vector3(transform.position.x, transform.position.y - stateManager.height / 2f, transform.position.z); ;
            Instantiate(tempCrater, feetTransform, Quaternion.identity);
            //

            groundPoundActive = false;
        }
    }

    private void Attack()
    {
        // Do attack Logic
        // Attack Cooldown
    }
    private void Slam()
    {
        Vector3 impulseDir;

        if (stateManager.OnSlope())
        {
            impulseDir = stateManager.slopeHit.normal * slamPower;
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
        // Add Impulse downwards
        //rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.down * groundPoundPower, ForceMode.Impulse);

        groundPoundActive = true;
    }
}
