using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using Random = UnityEngine.Random;

public class Movement : MonoBehaviour
{
    public float movementSpeed;
    private Boolean isMoving;
    private Vector2 input;
    private Animator animator;
    public LayerMask solidObjLayer;
    public LayerMask grassLayer;
    public LayerMask pokemonObjLayer;
    public LayerMask InteractableLayer;

    [SerializeField] MonsterBase randomGrassMonster;
    [SerializeField] public MonsterBase playerBase;
    public Monster player;

    public event Action<MonsterBase, Monster, Collider2D> onEncountered;

    private void Awake()
    {
        player = new Monster(playerBase, 5);
        //Take the animation in animator
        animator = GetComponent<Animator>();
    }

    public void HandleUpdate()
    {
        if (!isMoving)
        {
            //Take input from player
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //Removing diagonal movement
            if (input.x != 0) input.y = 0;

            //Check does player input something
            if(input != Vector2.zero)
            {
                //Set for both parameter to let the animation know which direction
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                var targetPosition = transform.position;
                targetPosition.x += input.x;
                targetPosition.y += input.y;

                if (IsWalkAble(targetPosition)) {
                    StartCoroutine(Move(targetPosition));
                }
            }
        }
        animator.SetBool("isMoving", isMoving);
        if (Input.GetKeyDown(KeyCode.Z))
            Interact();
    }

    void Interact(){
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var InteractPos = transform.position + facingDir;

        // Debug.DrawLine(transform.position, InteractPos, Color.green, 0.5f);
        var collider = Physics2D.OverlapCircle(InteractPos , 0.3f, InteractableLayer);
        if (collider != null){
            collider.GetComponent<Interactable>()?.Interact();
        }
    }
    
    //Function to move a player one tiles
    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, movementSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;
        checkForEncounter(targetPos);
    }


    //Check collision with something that can't not pass
    private bool IsWalkAble(Vector3 targetPos)
    {
        if(Physics2D.OverlapCircle(targetPos, 0.1f, solidObjLayer | InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }

    private void checkForEncounter(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.1f, grassLayer) != null)
        {
            if(Random.Range(1, 101) <= 10)
            {
                Debug.Log("grass touching");
                animator.SetBool("isMoving", false);
                onEncountered(randomGrassMonster, player, null);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.gameObject.tag == "Monster"){
            ListMonsters monsters = collision.GetComponent<ListMonsters>();
            onEncountered(monsters.Monster, player, collision);
        }
    }
}
