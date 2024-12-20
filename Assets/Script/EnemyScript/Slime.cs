using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Slime : Enemy
{
    protected Coroutine speedBoostCoroutine;

    protected virtual IEnumerator SpeedBoostRoutine()
    {
        while (!isEnemyDead)
        {
            // 이동 속도 2배 증가
            moveSpeed *= 2;
            Debug.Log("이동 속도 2배 증가!");

            // 2초 동안 유지
            yield return new WaitForSeconds(1f);

            // 이동 속도 원래대로 복구
            moveSpeed /= 2;
            Debug.Log("이동 속도 복구.");
            yield return new WaitForSeconds(5f); 
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (isEnemyDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            HeroKnightUsing playerScript = collision.gameObject.GetComponent<HeroKnightUsing>();
            if (playerScript != null && !playerScript.isDead)
            {
                playerScript.TakeDamage(atkDmg);
                Debug.Log($"{enemyName} 공격: {atkDmg} 데미지");
            }
        }
    }

    protected override void SpawnMark()
    {
        base.SpawnMark();
        if (speedBoostCoroutine == null)
            speedBoostCoroutine = StartCoroutine(SpeedBoostRoutine());  // 이동 속도 증가 코루틴 시작
        
    }

}
