using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
[GenerateAuthoringComponent]
public struct MoveForward : IComponentData
{
    public float speed;
}
