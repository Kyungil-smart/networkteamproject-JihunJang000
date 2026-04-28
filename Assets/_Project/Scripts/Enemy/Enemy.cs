using UnityEngine;

// 피격 (투사체 파티클에 닿았을떄 파티클에서 충돌처리 및 데미지 판정)
// 타격 (넥서스와 일정거리 이상 가까워졌을 때 넥서스 공격) 
// 이동. (고정된 넥서스를 향해 Navmesh 이동)
// 스폰 (EnemySpawner에서 생성되는 랜덤 위치에서 라운드 값에 맞게 적을 생성)


public class Enemy : MonoBehaviour, IDamageable
{
    public float hp = 50f;

    public void TakeDamage(float damage)
    {
        hp -= damage;
        Debug.Log($"적 피격! 데미지: {damage} / 남은 체력: {hp}");

        if (hp <= 0f)
        {
            Debug.Log("적 처치됨");
            Destroy(gameObject);
        }
    }
}
