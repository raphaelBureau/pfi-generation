using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolBehavior : MonoBehaviour 
{
    public class IsWithinRange : Node
    {
        Transform target;
        Transform self;
        NavMeshAgent agent;
        Animator animator;
        float detectionRange;
        public IsWithinRange(Transform target, Transform self, float detectionRange) : base()
        {
            this.target = target;
            this.self = self;
            this.detectionRange = detectionRange;
        }
        public override NodeState Evaluate()
        {
            if (Vector3.Distance(target.position, self.position) < detectionRange)
            {
                return NodeState.Success;
            }
            return NodeState.Failure;
        }
    }
    public class Walk : Node
    {
        NavMeshAgent agent;
        Animator animator;
        Transform target;
        public Walk(NavMeshAgent agent, Animator anim, Transform target) : base()
        {
            this.agent = agent;
            this.animator = anim;
            this.target = target;
        }
        public override NodeState Evaluate() //ne devrait pas retourner failure
        {
            agent.destination = target.position;
            if (agent.SetDestination(target.position))
            {
                if (agent.remainingDistance <= agent.stoppingDistance + 5)
                {
                    State = NodeState.Success;
                    animator.SetBool("running", false);
                    animator.SetBool("walking", false);
                    print("touching");
                }
                else
                {
                    State = NodeState.Running;
                    animator.SetBool("walking", true);
                    animator.SetBool("running", false);
                    //animator.SetFloat("Speed", 1);
                }
                return State;
            }
            print("walk failure");
            animator.SetBool("running", false);
            animator.SetBool("walking", false);
            return NodeState.Failure;
        }
    }
    public class Run : Node
    {
        float runTime, cooldown, speed;
        NavMeshAgent agent;
        Animator animator;
        Transform target;
        bool waiting = false;
        float stamina = 0;//si le target est out of range la stamina reste la meme (pas un bug cest une feature lol)
        public Run(float runTime, float cooldown, float speed, NavMeshAgent agent, Animator anim,Transform target) : base()
        {
            this.runTime = runTime; //le temps de course
            this.cooldown = cooldown; //le temps d'epuisement
            this.speed = speed;
            this.agent = agent;
            this.animator = anim;
            this.target = target;
        }
        public override NodeState Evaluate() //retorune succes si en cooldown
        {
            print(animator.GetBool("running"));
            if (!waiting)
            {
                agent.destination = target.position;
                if (agent.SetDestination(target.position))
                {
                    if (agent.remainingDistance <= agent.stoppingDistance + 0.5)
                    {
                        State = NodeState.Success; //la partie est termine parceque le target est touche
                        animator.SetBool("running", false);
                        animator.SetBool("walking", false);
                        //animator.SetFloat("Speed", 1);
                        print("touching");
                    }
                    else
                    {
                        stamina += Time.deltaTime; //depense seulement de la stamina quand il cours
                        State = NodeState.Running; //actuellement running lol
                        animator.SetBool("running", true);
                        animator.SetBool("walking", true);
                        //animator.SetFloat("Speed", speed);
                        print("runin");
                    }
                    if (stamina >= runTime)
                    {
                       // print("stamina exhausted");
                        waiting = true;
                        stamina = 0;
                    }
                    return State;
                }
                print("run failure");
                animator.SetBool("running", false);
                animator.SetBool("walking", false);
                return NodeState.Failure;
            }
            else
            {
                stamina+= Time.deltaTime; //ici stamina devient le timer du cooldown
                if(stamina >= cooldown)
                {
                   // print("stamina refreshed");
                    waiting = false;
                    stamina = 0;
                }
            }
            return NodeState.Failure; //si failure le selecteur va choisir Walk
        }
    }
    public class PatrolTask : Node
    {
        List<Transform> targets;
        NavMeshAgent agent;
        Animator animator;
        int targetIndex = 0;

        float waitTime = 0;
        float elapedTime = 0;
        bool waiting = false;

        public PatrolTask(List<Transform> targets, NavMeshAgent agent, float waitTime, Animator anim)
        {
            this.targets = targets;
            this.agent = agent;
            this.waitTime = waitTime;
            animator = anim;
        }

        public override NodeState Evaluate()
        {
            if (!waiting)
            {
                if (agent.SetDestination(targets[targetIndex].position))
                {
                    if (agent.remainingDistance <= agent.stoppingDistance + 1)
                    {
                        targetIndex = (targetIndex + 1) % targets.Count;
                        waiting = true;
                    }
                    animator.SetBool("walking", true);
                    animator.SetBool("running", false);
                    //animator.SetFloat("Speed", 1);
                    return State = NodeState.Running;
                }
                animator.SetBool("walking", false);
                animator.SetBool("running", false);
                return State = NodeState.Success;
            }
           // animator.SetBool("mj", true);
            elapedTime += Time.deltaTime;
            if (elapedTime >= waitTime)
            {
                elapedTime = 0;
                waiting = false;
                
            }
            animator.SetBool("walking", false);
            return State = NodeState.Running;
        }
    }
}
