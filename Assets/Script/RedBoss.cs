using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Boss : Enemy
{
    public float specialAttackCooldown = 5f; // 특수 공격 쿨다운
    private bool canUseSpecialAttack = true;

    // Start 메서드를 override로 선언
    protected override void Start()
    {
        base.Start(); // 부모 클래스의 Start 호출

        // RedBoss 고유의 상태 설정
        SetEnemyStatus("레드보스 킹", 500, 20); // 보스 초기화
        Debug.Log("RedBoss Initialized");
    }

    void Update()
    {
        // 기본 Enemy의 Update 기능 유지
        base.Update();

        // 보스 특수 행동 추가
        if (canUseSpecialAttack && !isEnemyDead)
        {
            StartCoroutine(UseSpecialAttack());
        }
    }

    private IEnumerator UseSpecialAttack()
    {
        canUseSpecialAttack = false;

        // 특수 공격 로직 (예: 범위 공격)
        Debug.Log("RedBoss uses a special attack!");
        PerformAreaAttack();

        // 쿨다운 대기
        yield return new WaitForSeconds(specialAttackCooldown);
        canUseSpecialAttack = true;
    }

    private void PerformAreaAttack()
    {
        // 범위 내에 있는 모든 플레이어 탐지
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, detectionRange, LayerMask.GetMask("Player"));

        foreach (Collider2D player in hitPlayers)
        {
            HeroKnightUsing playerScript = player.GetComponent<HeroKnightUsing>();
            if (playerScript != null && !playerScript.isDead)
            {
                playerScript.TakeDamage(atkDmg * 2); // 보스의 특수 공격은 2배 데미지
                Debug.Log($"RedBoss dealt {atkDmg * 2} damage with a special attack!");
            }
        }
    }

    // Gizmo: 특수 공격 범위를 시각적으로 표시
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();  // 기본 Enemy의 시각적 범위 표시
        Gizmos.color = Color.yellow;  // 특수 공격 범위 색깔
        Gizmos.DrawWireSphere(transform.position, detectionRange);  // 특수 공격 범위
    }

    // 적이 죽었을 때 호출되는 함수
    protected override void HandleWhenDead()
    {
        base.HandleWhenDead();  // 기본 Enemy의 죽음 처리

        // RedBoss 고유의 죽음 효과
        Debug.Log("RedBoss defeated! Special loot dropped.");
        DropSpecialLoot();
    }

    // 보스 전용 특별 아이템 드랍 함수
    private void DropSpecialLoot()
    {
        // 실제 아이템 오브젝트 생성 (예: Instantiation을 통한 아이템 드랍)
        Debug.Log("Special loot is dropped!");
        // 예시: Instantiate(lootPrefab, transform.position, Quaternion.identity);
    }
}
