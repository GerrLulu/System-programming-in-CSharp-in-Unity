using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace LessonTwo.TaskTwo
{
    public struct TaskTwoLessonTwo : IJobParallelFor
    {
        public NativeArray<Vector3> Positions;
        public NativeArray<Vector3> Velocities;
        public NativeArray<Vector3> FinalPositions;


        public void Execute(int index)
        {
            for(int i = 0; i < Positions.Length; i++)
            {
                if (i == index)
                    continue;

                FinalPositions[i] = Positions[i] + Velocities[i];
            }
        }
    }
}