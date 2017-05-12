using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncException
{
    public static class Extensions
    {
        public static Task PreserveMultipleExceptions(this Task originalTask)
        {
            var tcs = new TaskCompletionSource<object>();
            originalTask.ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.RanToCompletion:
                        tcs.SetResult(null);
                        break;
                    case TaskStatus.Faulted:
                        tcs.SetException(originalTask.Exception);
                        break;
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => MainAsync(args));
            Console.ReadKey();
        }

        static async void MainAsync(string[] args)
        {
            var getNumbers1Task = GetNumbers1();
            var getNumbers2Task = GetNumbers2();
            var getNumbers3Task = GetNumbers3();

            try
            {
                await Task.WhenAll(getNumbers1Task, getNumbers2Task, getNumbers3Task).PreserveMultipleExceptions();
            }
            catch (AggregateException)
            {
            }

            if (getNumbers1Task.IsFaulted)
            {
                Console.Write(string.Format("{0} - {1}", "GetNumbers1", getNumbers1Task.Exception.Message));
            }

            if (getNumbers2Task.IsFaulted)
            {
                Console.Write(string.Format("{0} - {1}", "GetNumbers2", getNumbers2Task.Exception.Message));
            }

            if (getNumbers3Task.IsFaulted)
            {
                Console.Write(string.Format("{0} - {1}", "GetNumbers3", getNumbers3Task.Exception.Message));
            }
        }

        private void Log(string msg)
        {
            Console.Write(msg);
        }

        private static async Task<int> GetNumbers1()
        {
            return await Task.Run(() =>
            {
                Thread.Sleep(1000);
                return GetSum(1, 2);
            });
        }

        private static async Task<int> GetNumbers2()
        {
            return await Task.Run(() =>
            {
                Thread.Sleep(1000);
                return GetSum(1, 2);
            });
        }

        private static async Task<int> GetNumbers3()
        {
            return await Task.Run(() =>
            {
                return GetSum(1, 0);
            });
        }

        private static int GetSum(int a, int b)
        {
            try
            {

                var total = a + b;

                if (total == 1)
                {
                    throw new Exception("Erro soma");
                }

                return total;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
