using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
    Idle,
    Wandering,
    Attacking
}

public class NPC : MonoBehaviour , IDamagalbe
{
    [Header("Stats")]
    public int health;
    public float walkSpeed;
    public float runSpeed;
    public ItemData[] dropOnDeath;

    [Header("AI")]
    private NavMeshAgent agent;
    public float detectDistance;
    [SerializeField]
    private AIState aiState;
    
    [Header("Wandering")]
    public float minWanderDistance; // ���� �̵��� �ּҰ�
    public float maxWanderDistance; // ���� �̵��� �ִ밪
    public float minWanderWaitTime; // �󸶳� ��ٷȴٰ� �̵����� ?
    public float maxWanderWaitTime;

    [Header("Combat")]
    public int damage;
    public float attackRate;
    private float lastAttackTime;
    public float attackDistance;

    private float playerDistance;
    public float fieldOfView = 120f;    // ���ݽ� �þ� ����

    private Animator animator;
    private SkinnedMeshRenderer[] meshRenderers;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        SetState(AIState.Wandering);
    }

    // Update is called once per frame
    void Update()
    {
        playerDistance = Vector3.Distance(transform.position, CharacterManager.Instance.Player.transform.position);

        animator.SetBool("Moving", aiState != AIState.Idle);

        switch (aiState) 
        {
            case AIState.Idle:
            case AIState.Wandering:
                PassiveUpdate();
                break;
            case AIState.Attacking:
                AttackingUpdate();
                break;

        }
    }
    public void SetState(AIState state)
    {
        aiState = state;

        switch(aiState)
        {
            case AIState.Idle:
                agent.speed = walkSpeed;
                agent.isStopped = true;
                break;
            case AIState.Wandering:
                agent.speed = walkSpeed;
                agent.isStopped = false;
                break;
            case AIState.Attacking:
                agent.speed = runSpeed;
                agent.isStopped = false;
                break;
        }

        animator.speed = agent.speed / walkSpeed;
    }

    void PassiveUpdate()
    {
        if(aiState == AIState.Wandering && agent.remainingDistance < 0.1f)
        {
            SetState(AIState.Idle);
            Invoke("WanderToNewLocation" , Random.Range(minWanderWaitTime , maxWanderWaitTime)); 
            //Q. �� �����Ӹ��� Invoke �����ٵ�, �Ķ���ͷ� ������ �ð� ���ȿ� Invoke�� �Ͼ�� �ʴ°� ?
        }

        if(playerDistance < detectDistance)
        {
            SetState(AIState.Attacking);
        }
    }
    
    void WanderToNewLocation()
    {
        if (aiState != AIState.Idle) return;    // ���ǿ��� �̷��� ���Դµ�
        //if (aiState == AIState.Idle) return;    // Idle �϶� �ȵ����� �Ϸ��� �Ѱ� �ƴұ� ?

        SetState(AIState.Wandering);
        agent.SetDestination(GetWanderLocation());
    }

    //��ǥ������ �����ִ� �Լ�
    Vector3 GetWanderLocation()
    {
        NavMeshHit hit;

        // ���� ������ ������ �ָ� , �̵��Ҽ� �ִ� ��ο� ���� �ִ� ��θ� ��ȯ���ش�
        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance , maxWanderDistance)) , out hit , maxWanderDistance , NavMesh.AllAreas );
        //onUnitSphere == �������� 1�� �� ������ ������ ��



        //do - while ������ �ٲ㺸��
        int i = 0;

        while(Vector3.Distance(transform.position , hit.position) < detectDistance)
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);
            i++;

            if (i == 30) break;
        }

        return hit.position;
    }

    void AttackingUpdate()
    {
        if(playerDistance < attackDistance && IsPlayerInFOV())  // �Ÿ��� ������ ���ݰ��� �ȿ� �ִ� ���
        {
            agent.isStopped = true; // ���缭 ������
            if(Time.time - lastAttackTime > attackRate)
            {
                lastAttackTime = Time.time;

                CharacterManager.Instance.Player.controller.GetComponent<IDamagalbe>().TakePhysicalDamage(damage);
                // �÷��̾� ��Ʈ�ѷ� �ȿ� �ִ� TakePhysicalDamage �Լ��� �������� �־� ȣ�⸸ ���ش�.
                // �ǰݿ� ���� ������ ���ʿ��� ó�����ְ�,
                // ���⼭�� ������ �ߴ� < ��°͸� �˷��ش�.

                animator.speed = 1;
                animator.SetTrigger("Attack");
            }
        }
        else
        {
            if(playerDistance < detectDistance) // Ž������ �ȿ��� ������ , ���� ���� �ۿ� �ִ� ���
            {
                agent.isStopped = false;
                NavMeshPath path = new NavMeshPath();

                //��ǥ������ ã����
                if(agent.CalculatePath(CharacterManager.Instance.Player.transform.position , path))
                {
                    agent.SetDestination(CharacterManager.Instance.Player.transform.position);

                    //path �� �̿��� �̵������� ���� �� ������ , Ȱ���� ������
                }
                //������
                else
                {
                    agent.SetDestination(transform.position);
                    agent.isStopped = true;
                    SetState(AIState.Wandering);
                }
            }
            else // �־����� ���
            {
                agent.SetDestination(transform.position);
                agent.isStopped = true;
                SetState(AIState.Wandering);
            }
        }
    }

    //�þ�(����) ���� �ȿ� player�� �ִ��� �˻��ϴ� �Լ�
    bool IsPlayerInFOV()
    {
        Vector3 directionToPlayer = CharacterManager.Instance.Player.transform.position - transform.position;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        return angle < fieldOfView * 0.5f;
    }

    public void TakePhysicalDamage(int damage)
    {
        health -= damage;

        if(health <= 0)
        {
            Die();
        }

        //�ǰ� ȿ��
        StartCoroutine(DamageFlash());
    }

    void Die()
    {
        for(int i = 0; i < dropOnDeath.Length; i++)
        {
            Instantiate(dropOnDeath[i].dropPrefab , transform.position + Vector3.up * 2.0f , Quaternion.identity);
        }

        Destroy(gameObject);
    }


    IEnumerator DamageFlash()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material.color = new Color(1.0f, 0.6f, 0.6f);
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material.color = Color.white;
        }
    }
}
