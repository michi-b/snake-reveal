using System;
using Extensions;
using Game.Enums;
using Unity.Mathematics;
using UnityEngine;
using Utility;

namespace Game.Lines
{
    /// <summary>
    /// minimal struct to cache information in line points of a line container
    /// </summary>
    [Serializable]
    public struct LineNode
    {
        public int2 Position;
        public GridDirection Direction;

        public override string ToString()
        {
            return Position.ToString();
        }
    }
}