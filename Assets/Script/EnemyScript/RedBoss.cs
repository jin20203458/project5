//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class RedSlimeKing : Slime
//{

//    public delegate void BossDeathHandler();
//    public event BossDeathHandler OnBossDeath;  // ������ �׾��� �� ȣ��� �̺�Ʈ

//    public Text attackMessageText;                          // UI �ؽ�Ʈ�� ������ ���� (����� �޽����� ǥ���� �ؽ�Ʈ)
//    public float specialAttackCooldown = 8f;                // Ư�� ����(���Ȱ���) ��ٿ�
//    public float PerformAreaAttackRange;                    // �������ݹݰ�
//    public GameObject attackEffectPrefab;                   // ���� ���� ����Ʈ Prefab�� ������ ����
//    private bool canUseSpecialAttack = false;               // �������� ��� ���� ����
//    private SpawnEnemy spawnEnemyScript;                    // SpawnEnemy ��ũ��Ʈ�� ������ ����

//    public Vector3 effectScale = new Vector3(1f, 1f, 1f);  // ����Ʈ�� ũ�� ���� (�ν����Ϳ��� ���� ����)


//    // Start �޼��带 override�� ����
//    protected override void Start()
//    {
//        base.Start(); // �θ� Ŭ������ Start ȣ��

//        Debug.Log("RedBoss Initialized");
//        PerformAreaAttackRange = detectionRange * 0.9f; // �������ݹݰ�
//        spawnEnemyScript = GetComponent<SpawnEnemy>();
//    }

//    protected override void Update()
//    {
//        // �⺻ Enemy�� Update ��� ����
//        base.Update();

//        // ���� Ư�� �ൿ �߰�
//        if (canUseSpecialAttack && !isEnemyDead)
//            StartCoroutine(UseSpecialAttack());
//    }

//    protected override void DetectAndChasePlayer()
//    {
//        if (player == null) return;

//        HeroKnightUsing playerScript = player.GetComponent<HeroKnightUsing>();
//        if (playerScript != null && playerScript.isDead)
//        {
//            isChasing = false;
//            return;
//        }

//        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
//        if (distanceToPlayer <= detectionRange)
//        {
//            if (!isChasing) // �÷��̾ ó�� Ž������ ���� ��ũ�� ��ȯ
//            {
//                SpawnMark();
//                canUseSpecialAttack = true;

//                // ������ �÷��̾ Ž������ �� �� ������ ����
//                if (spawnEnemyScript != null) spawnEnemyScript.StartSpawning();
//                SpawnMark();
//                canUseSpecialAttack = true;

//                isChasing = true;
//                Vector3 direction = (player.position - transform.position).normalized;
//                transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
//                LookAtPlayer();
//            }

         
//        }
//        else
//        {
//            isChasing = false;
//        }
//    }
//    protected override void SpawnMark()
//    {
//        Debug.Log("���� ������ŷ�� ����� �ֽ��մϴ�!!");
//        if (markPrefab != null)
//        {
//            // ��Ŀ�� ������ ��ġ�� ���� ��ġ���� markYOffset��ŭ Y������ �ø�
//            Vector3 spawnPosition = transform.position + new Vector3(0, 3f, 0); // markYOffset ����ŭ Y������ �̵�
//            GameObject markInstance = Instantiate(markPrefab, spawnPosition, Quaternion.identity);
//            StartCoroutine(SpeedBoostRoutine());  // �̵� �ӵ� ���� �ڷ�ƾ ����

//            // ������ ��Ŀ�� ���ʹ̸� �����ϵ��� ����
//            Mark markScript = markInstance.GetComponent<Mark>();
//            if (markScript != null)
//            {
//                markScript.enemy = transform; // ��Ŀ�� ���ʹ̸� �����ϵ��� ����
//            }
//        }
//    }

//    private IEnumerator UseSpecialAttack()
//    {
//        canUseSpecialAttack = false;

//        // Ư�� ���� �غ� �޽����� ȭ�鿡 ǥ��
//        ShowAttackMessage("���� ������ŷ�� ���� ������ �غ��մϴ�.");
//        Debug.Log("���� ������ŷ�� ���� ������ �غ��մϴ�.");
//        yield return new WaitForSeconds(2f);  // 2�� ���� �ؽ�Ʈ ����
//        ShowAttackMessage("");
//        yield return new WaitForSeconds(5f);  // 3�� �� �������� 
//        PerformAreaAttack();


//        // ��ٿ� ���
//        yield return new WaitForSeconds(specialAttackCooldown);


//        canUseSpecialAttack = true;

//    }

//    private void ShowAttackMessage(string message)
//    {
//        // UI �ؽ�Ʈ�� �޽����� ����
//        if (attackMessageText != null)
//        {
//            attackMessageText.text = message;
//        }
//    }

//    private void PerformAreaAttack()
//    {
//        // ���� ���� �ִ� ��� Collider2D�� Ž��
//        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, PerformAreaAttackRange);

//        // ���� ������ ����Ʈ ���� (���� ��ü�� ǥ��)
//        if (attackEffectPrefab != null)
//        {
//            GameObject effectInstance = Instantiate(attackEffectPrefab, transform.position, Quaternion.identity);
//            // �ν����Ϳ��� ������ effectScale�� ũ�� ����
//            effectInstance.transform.localScale = effectScale;
//        }

//        foreach (Collider2D hitObject in hitObjects)
//        {
//            // �÷��̾� �±׸� ���� ������Ʈ���� Ȯ��
//            if (hitObject.CompareTag("Player"))
//            {
//                // �÷��̾��� ��ũ��Ʈ ��������
//                HeroKnightUsing playerScript = hitObject.GetComponent<HeroKnightUsing>();

//                if (playerScript != null && !playerScript.isDead)
//                {
//                    playerScript.TakeDamage(atkDmg * 2);
//                    Debug.Log($"���庸���� {hitObject.name}���� {atkDmg * 3}�� Ư�� �������� �������� �������ϴ�!");
//                }
//            }
//        }
//    }

//    // ���� �׾��� �� ȣ��Ǵ� �Լ�
//    protected override void HandleWhenDead()
//    {
//        base.HandleWhenDead();  // �⺻ Enemy�� ���� ó��

//        // RedBoss ������ ���� ó��
//        Debug.Log("RedBoss defeated! Special loot dropped.");

//        OnBossDeath?.Invoke();
//        Debug.Log("���� ���� �̺�Ʈ ȣ���");


//        DropSpecialLoot();
//    }

//    // ���� ���� Ư�� ������ ��� �Լ�
//    private void DropSpecialLoot()
//    {
//        // ���� ������ ������Ʈ ���� (��: Instantiation�� ���� ������ ���)
//        Debug.Log("Special loot is dropped!");
//    }
//    protected override void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(transform.position, detectionRange);

//        Gizmos.color = Color.blue;
//        Gizmos.DrawWireSphere(transform.position, PerformAreaAttackRange);
//    }
//}