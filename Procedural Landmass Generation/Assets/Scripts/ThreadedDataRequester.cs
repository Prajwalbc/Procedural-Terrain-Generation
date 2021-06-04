using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour
{
  static ThreadedDataRequester instance;

  Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

  void Awake()
  {
    instance = FindObjectOfType<ThreadedDataRequester>();
  }

  //THREADING
  public static void RequestData(Func<object> generateData, Action<object> callback) // method just starts a new thread
  {
    //ThreadStart type defines what the thread will do 
    //lambda function delegate here is used to negate the writing of another method which then needs to be invoked
    ThreadStart threadStart = delegate
    {
      instance.DataThread(generateData, callback);
    };
    new Thread(threadStart).Start();
  }

  void DataThread(Func<object> generateData, Action<object> callback)
  {
    object data = generateData();
    //to avoid running the callback on a separate thread, we enqueue it with the parameter
    //locking the queue so only one thread can access at a time
    lock (dataQueue)
    {
      dataQueue.Enqueue(new ThreadInfo(callback, data));
    }
  }

  void Update()
  {
    //In the unity's main thread we check if the both the dataQueue and dataQueue is > 0, if yes, we then dequeue them from their respective queue and 
    // here we excute the respective callback with their respective parameter(heightMap or MeshData accordingly)

    if (dataQueue.Count > 0)
    {
      for (int i = 0; i < dataQueue.Count; i++)
      {
        lock (dataQueue)
        {
          ThreadInfo threadInfo = dataQueue.Dequeue();
          threadInfo.callback(threadInfo.parameter);
        }
      }
    }
  }

  struct ThreadInfo
  {
    public readonly Action<object> callback;
    public readonly object parameter;

    public ThreadInfo(Action<object> callback, object parameter)
    {
      this.callback = callback;
      this.parameter = parameter;
    }
  }
}
