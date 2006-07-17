using System;

namespace Geosoft.DotNetTools.Common
{
	/// <summary>
	/// Summary description for Queue.
	/// </summary>
	public class Queue : System.Collections.Queue
	{
      /// <summary>
      /// Semaphore
      /// </summary>
      protected Semaphore  m_hSemaphore = new Semaphore(0);

      /// <summary>
      /// 	<para>Initializes an instance of the <see cref="Queue"/> class.</para>
      /// </summary>
		public Queue() : base()
		{			
		}

      /// <summary>
      /// Enque an object onto the queue
      /// </summary>
      /// <param name="obj"></param>
      public override void Enqueue(object obj)
      {
         lock (base.SyncRoot)
         {
            base.Enqueue (obj);
         }
         m_hSemaphore.Release();
      }

      /// <summary>
      /// Dequeue an object from the queue when one is pushed on
      /// </summary>
      /// <returns></returns>
      public override object Dequeue()
      {
         if (m_hSemaphore.Wait()) 
         {
            lock (base.SyncRoot) 
            {
               return base.Dequeue ();
            }
         }
         return null;
      }

      /// <summary>
      /// Dequeue an object from the queue, waiting a specified timeout value before returing empty
      /// </summary>
      /// <param name="iTimeout"></param>
      /// <returns></returns>
      public object Dequeue(int iTimeout)
      {
         if (m_hSemaphore.Wait(iTimeout)) 
         {
            lock (base.SyncRoot) 
            {
               return base.Dequeue ();
            }
         }
         return null;
      }
	}
}
