using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LessonOne.TaskTwo
{
    public class SckriptForThirdTask : MonoBehaviour
    {
        [SerializeField] private int _waitingTime = 1000;

        private void Start()
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = cancelTokenSource.Token;

            Task1(cancelToken);
            Task2(cancelToken);

            cancelTokenSource.Cancel();
            cancelTokenSource.Dispose();
        }

        private async void Task1(CancellationToken ct)
        {
            await Task.Delay(_waitingTime, ct);
            Debug.Log($"Task 1 completed");
        }

        private async void Task2(CancellationToken ct)
        {
            int t = (int)Time.fixedDeltaTime;
            await Task.Delay(t, ct);
            Debug.Log($"Task 2 completed");
        }
    }
}