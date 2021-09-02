using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForceDirectedGraph : MonoBehaviour
{
    public List<FDGNode> nodes;
    public float desiredConnectedNodeDistance = 1.0f;
    public float connectedNodeForce = 3.0f;
    public float disconnectedNodeForce = 3.0f;
    public float dragFactor = 0.994f;

    // Start is called before the first frame update
    void Start()
    {
        nodes = new List<FDGNode>();
        for (int i = 0; i < 20; i++)
        {
            var newNode = new FDGNode(Random.insideUnitSphere * 10.0f, Random.ColorHSV());
            var potentialNewNeighbors = nodes.Where(node => Random.value > 0.85f).ToList();
            nodes.Add(newNode);
            newNode.AddNeighbors(potentialNewNeighbors);
        }
        var nodeRed = new FDGNode(Random.insideUnitSphere * 10.0f, Color.red);
        var nodeGreen = new FDGNode(Random.insideUnitSphere * 10.0f, Color.green);
        var nodeBlue = new FDGNode(Random.insideUnitSphere * 10.0f, Color.blue);

        nodes.Add(nodeRed);
        nodes.Add(nodeGreen);
        nodes.Add(nodeBlue);

        nodeBlue.MakeNeighbors(nodeRed);
        nodeBlue.MakeNeighbors(nodeGreen);

    }

    // Update is called once per frame
    void Update()
    {        
        if (nodes == null)
            Start();

        ApplyGraphForce();
        foreach (var node in nodes)
        {
            node.position += node.velocity * Time.deltaTime;
        }  
    }

    private void ApplyGraphForce()
    {
        foreach (var node in nodes)
        {
            foreach (var connectedNode in node.neighbors)
            {
                var difference = node.position - connectedNode.position;
                difference = (difference == Vector3.zero) ? Random.insideUnitSphere * 0.001f: difference;
                var distance = Mathf.Max(difference.magnitude, 0.001f);

                var appliedForce = connectedNodeForce * Mathf.Log10(distance / desiredConnectedNodeDistance);
                connectedNode.velocity += appliedForce * difference.normalized * Time.deltaTime;
                connectedNode.velocity *= dragFactor;
            }

            var disconnectedNodes = nodes.Except(node.neighbors);
            foreach (var disconnectedNode in disconnectedNodes)
            {
                if (disconnectedNode == node)
                    continue;
                var difference = node.position - disconnectedNode.position;
                difference = (difference == Vector3.zero) ? Random.insideUnitSphere * 0.001f : difference;

                var distance = Mathf.Max(difference.magnitude, 1.0f);

                var appliedForce = disconnectedNodeForce / (distance * distance);
                disconnectedNode.velocity -= appliedForce * difference.normalized * Time.deltaTime;
                disconnectedNode.velocity *= dragFactor;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (nodes == null)
            Start();
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(Vector3.zero, 10.0f);

        foreach (var node in nodes)
        {
            Gizmos.color = node.color;
            Gizmos.DrawWireSphere(node.position, 0.125f);
            Gizmos.color = Color.white;
            foreach (var connectedNode in node.neighbors)
            {
                Gizmos.DrawLine(node.position, connectedNode.position);
            }
        }   
    }
}

public class FDGNode
{
    public Vector3 position;
    public Vector3 velocity;
    public Color color;
    public List<FDGNode> neighbors;

    public FDGNode(Vector3 position, Color color)
    {
        this.position = Vector3.zero;// position;
        this.color = color;
        this.velocity = Vector3.zero;
        this.neighbors = new List<FDGNode>();
    }

     public void MakeNeighbors(FDGNode neighbor)
    {
        this.neighbors.Add(neighbor);
        neighbor.neighbors.Add(this);        
    }

    public void AddNeighbors(List<FDGNode> listOfNeighbors)
    {
        foreach (var neighbor in listOfNeighbors)
        {
            this.MakeNeighbors(neighbor);
        }

    }
}
