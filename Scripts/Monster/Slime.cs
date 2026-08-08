using System;
using UnityEngine;

public class Slime : Monster
{
    [SerializeField] Slime smallSlimePrefab;
    [SerializeField] bool splitAction;
    [SerializeField] int splitCount;
    
    private void FixedUpdate()
    {
        if(target == null) return;
        
        Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        
        rb.MovePosition(rb.position + direction * (moveSpeed * Time.fixedDeltaTime));
    }
    
    protected override void Die()
    {
        if (splitAction)
            Split();
        
        base.Die();
    }

    private void Split()
    {
        for (int i = 0; i < splitCount; i++)
        {
            //작은 슬라임들 생성
            Instantiate(smallSlimePrefab, transform.position, Quaternion.identity);
        }
    }
}
