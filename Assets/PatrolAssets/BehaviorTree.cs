using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public enum NodeState
{
    Running,
    Success,
    Failure
}

public abstract class Node
{
    protected NodeState State { get; set; }
    public Node parent;
    protected List<Node> children;

    Dictionary<string, object> data = new Dictionary<string, object>();
    public void SetData(string key, object value)
    {
        data[key] = value;
    }

    public object GetData(string key)
    {
        if (data.TryGetValue(key, out object value))
        {
            return value;
        }
        if (parent != null)
        {
            return parent.GetData(key);
        }
        return null;
    }

    public bool TryRemoveData(string key)
    {
        if (data.Remove(key))
        {
            return true;
        }
        if (parent != null)
        {
            return parent.TryRemoveData(key);
        }
        return false;
    }
    public Node()
    {
        parent = null;
        State = NodeState.Running;
        children = new List<Node>();
    }

    public Node(List<Node> pChildren)
    {
        parent = null;
        this.State = NodeState.Running;
        children = new List<Node>();
        foreach (Node child in pChildren)
        {
            Attach(child);
        }
    }
    protected void Attach(Node n)
    {
        children.Add(n);
        n.parent = this;
    }

    public abstract NodeState Evaluate();
}

public class Sequence : Node
{
    public Sequence(List<Node> n) : base(n) {}
    public override NodeState Evaluate()
    {
        foreach (Node child in children)
        {
            State = child.Evaluate();
            if (State != NodeState.Success)
            {
                return State;
            }
        }
        return State = NodeState.Success;

    }
}

public class Selector : Node
{
    public Selector(List<Node> n) : base(n) { }

    public override NodeState Evaluate()
    {
        foreach (Node child in children)
        {
            State = child.Evaluate();
            if (State != NodeState.Failure)
            {
                return State;
            }
        }
        return State = NodeState.Failure;

    }
}

public class Inverter : Node
{
    public Inverter(List<Node> n) : base(n)
    {
        if (n.Count != 1)
        {
            throw new ArgumentException("inverter children.count pas egal a un");
        }
    }

    public override NodeState Evaluate()
    {
        NodeState childstate = children[0].Evaluate();
        if (childstate == NodeState.Success)
        {
            State = NodeState.Failure;
        }
        else if (childstate == NodeState.Failure)
        {
            State = NodeState.Success;
        }
        else
        {
            State = NodeState.Running;
        }

        return State;
    }
}

public class GoToTarget : Node
{
    Transform target;
    NavMeshAgent agent;

    public GoToTarget(Transform target, NavMeshAgent agent) : base()
    {
        this.target = target;
        this.agent = agent;
    }

    public override NodeState Evaluate()
    {
        agent.destination = target.position;
        if (agent.SetDestination(target.position))
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                State = NodeState.Success;
            }
            else
            {
                State = NodeState.Running;
            }
            return State;
        }
        return State = NodeState.Failure;
    }
}

public class IsWithinRange : Node
{
    Transform target;
    Transform self;
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

public class PatrolTask : Node
{
    List<Transform> targets;
    NavMeshAgent agent;
    int targetIndex = 0;

    float waitTime = 0;
    float elapedTime = 0;
    bool waiting = false;

    public PatrolTask(List<Transform> targets, NavMeshAgent agent, float waitTime)
    {
        this.targets = targets;
        this.agent = agent;
        this.waitTime = waitTime;
    }

    public override NodeState Evaluate()
    {
        if (!waiting)
        {
            if (agent.SetDestination(targets[targetIndex].position))
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    targetIndex = (targetIndex + 1) % targets.Count;
                    waiting = true;
                }
                return State = NodeState.Running;
            }
            return State = NodeState.Failure;
        }

        elapedTime += Time.deltaTime;
        if (elapedTime >= waitTime)
        {
            elapedTime = 0;
            waiting = false;
        }
        return State = NodeState.Running;
    }
}