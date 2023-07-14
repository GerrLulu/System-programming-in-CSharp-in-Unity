using Unity.Collections;
using Unity.Jobs;

namespace LessonTwo.TaskOne
{
    public struct TaskOneLessonTwo : IJob
    {
        public NativeArray<int> Array;


        public void Execute()
        {
            for(int i = 0; i < Array.Length; i++)
            {
                if (Array[i] > 10)
                {
                    Array[i] = 0;
                }
            }
        }
    }
}