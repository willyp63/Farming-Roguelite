using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : Placeable
{
    [SerializeField]
    private int maxHealth;

    [SerializeField]
    private int damage;

    [SerializeField]
    private float attackSpeed;

    [SerializeField]
    private float attackRange;

    [SerializeField]
    private float moveSpeed;
}
