using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LessonOne.TaskThree
{
    public class SckriptForThirdTask : MonoBehaviour
    {
        [SerializeField] private int _waitingTime = 1000;


        private void Start()
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = cancelTokenSource.Token;

            Task<bool> task1 = Task1(cancelToken);
            Task<bool> task2 = Task2(cancelToken);
            Task<bool> resultTask = WhatTaskFasterAsync(cancelToken, task1, task2);

            bool result = resultTask.Result;
            if (result)
            {
                Debug.Log($"Task 1 completed {result}");
            }
            else
            {
                Debug.Log($"Task 2 completed {result}");
            }
        }

        private async Task<bool> Task1(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                Debug.Log("Операция 1 прервана токеном.");
                return false;
            }

            await Task.Delay(_waitingTime);
            return true;
        }

        private async Task<bool> Task2(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                Debug.Log("Операция 2 прервана токеном.");
                return false;
            }

            int t = (int)Time.fixedDeltaTime;
            await Task.Delay(t);
            return false;
        }

        private static async Task<bool> WhatTaskFasterAsync(CancellationToken ct, Task task1, Task task2)
        {
            bool result;
            using (CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                CancellationToken linkedCt = linkedCts.Token;
                Task<bool> resultTask = (Task<bool>)await Task.WhenAny(task1, task2);

                result = resultTask.Result;

                linkedCts.Cancel();
                return result;
            }
        }
    }
}