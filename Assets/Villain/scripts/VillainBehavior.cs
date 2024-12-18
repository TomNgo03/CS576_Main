using UnityEngine;
using System.Collections;

public class VillainBehavior : MonoBehaviour
{
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float attackCooldown = 1f;
    
    private float baseSpeed;
    private float speedMultiplier = 1f;

    private Transform currentTargetTree;
    private int currentTreeHealth = 10;
    private int health;
    private bool isAttacking = false;
    private VillainAnimationController animController;
    private bool isDead = false;

    private void Start()
    {
        health = maxHealth;
        animController = GetComponent<VillainAnimationController>();
        baseSpeed = moveSpeed;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
        moveSpeed = baseSpeed * speedMultiplier;
    }

    public void ResetSpeed()
    {
        speedMultiplier = 1f;
        moveSpeed = baseSpeed;
    }

    public void Initialize(Transform initialTree)
    {
        currentTargetTree = initialTree;
        StartCoroutine(AIRoutine());
    }

    private IEnumerator AIRoutine()
    {
        while (!isDead)
        {
            if (currentTargetTree == null)
            {
                FindNextTree();
                yield return null;
                continue;
            }

            // Move towards tree if not in range
            float distanceToTree = Vector3.Distance(transform.position, currentTargetTree.position);
            if (distanceToTree > attackRange)
            {
                MoveTowardsTree();
            }
            else if (!isAttacking)
            {
                StartCoroutine(AttackTree());
            }

            yield return null;
        }
    }

    private void MoveTowardsTree()
    {
        Vector3 direction = (currentTargetTree.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
        
        // Update animation
        if (animController != null)
        {
            // Assuming "Running" is the animation state name
            animController.CrossFade("Running", 0.15f);
        }
    }

    private IEnumerator AttackTree()
    {
        isAttacking = true;
        
        // Play attack animation
        if (animController != null)
        {
            animController.CrossFade("AttackHor", 0.15f);
        }

        // Damage tree
        currentTreeHealth--;
        if (currentTreeHealth <= 0 && currentTargetTree != null)
        {
            Destroy(currentTargetTree.gameObject);
            currentTargetTree = null;
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private void FindNextTree()
    {
        GameObject[] trees = GameObject.FindGameObjectsWithTag("Tree");
        float closestDistance = float.MaxValue;
        Transform closestTree = null;

        foreach (GameObject tree in trees)
        {
            float distance = Vector3.Distance(transform.position, tree.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTree = tree.transform;
            }
        }

        currentTargetTree = closestTree;
        if (closestTree != null)
        {
            currentTreeHealth = 10; // Reset health for new tree
        }
    }

    public void TakeDamage()
    {
        health--;
        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        if (animController != null)
        {
            animController.CrossFade("Death", 0.15f);
        }
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // Wait for death animation to play
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
