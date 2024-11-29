using System.Collections;
using UnityEngine;

public class Skill : MonoBehaviour
{
    public GameObject skillPrefab; // Skill1 프리팹을 할당할 변수
    public float spawnRange = 5.0f; // 랜덤으로 소환될 위치의 범위
    public float destroyTime = 1.0f; // 프리팹이 사라지는 시간 (1초)
    public float spawnInterval = 0.05f; // 프리팹 소환 간의 간격 (0.1초)

    void Update()
    {
        // T 키를 누르면 10번 반복해서 랜덤 위치에 Skill1 이펙트를 소환
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(SpawnSkillEffect());
        }
    }

    // 코루틴을 이용하여 10번 반복해서 랜덤 위치에 Skill1 이펙트를 소환
    IEnumerator SpawnSkillEffect()
    {
        // Player 태그를 가진 HeroKnight 오브젝트 찾기
        GameObject heroKnight = GameObject.FindGameObjectWithTag("Player");

        if (heroKnight != null && skillPrefab != null)
        {
            for (int i = 0; i < 10; i++)
            {
                // HeroKnight의 위치를 기준으로 랜덤한 위치 생성
                float randomX = Random.Range(-spawnRange, spawnRange);
                float randomY = 0; // Y 값을 고정시켜서 평면에 소환
                float randomZ = Random.Range(-spawnRange, spawnRange);

                // HeroKnight의 위치를 기준으로 랜덤한 오프셋을 적용
                Vector3 spawnPosition = heroKnight.transform.position + new Vector3(randomX, randomY, randomZ);

                // Skill1 프리팹을 해당 위치에 소환
                GameObject skillEffect = Instantiate(skillPrefab, spawnPosition, Quaternion.identity);

                // 일정 시간 후에 소환된 프리팹을 삭제
                Destroy(skillEffect, destroyTime);

                // 프리팹 간 간격을 두기 위해 대기
                yield return new WaitForSeconds(spawnInterval); // 소환 간 간격 (0.5초)
            }
        }
        else
        {
            Debug.LogWarning("HeroKnight 또는 Skill1 프리팹이 할당되지 않았습니다.");
        }
    }
}