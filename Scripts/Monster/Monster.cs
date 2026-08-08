using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    //컴포넌트
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Collider2D col;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    
    //각종 속성
    [SerializeField] protected Transform target; //플레이어 위치
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int contactDamage;
    
    //기타 변수
    protected int currentHealth;

    protected void Awake()
    {
         currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
