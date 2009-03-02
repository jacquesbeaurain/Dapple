using System;
using System.Threading;

namespace Geosoft.DotNetTools.Common
{   
   /// <summary>
   /// Implementation of a semaphore class for our queue
   /// http://weblogs.asp.net/tspascoal/archive/2004/01/16/59340.aspx
   /// </summary>
	public class Semaphore 
   {
      #region Member Variables
      private int m_iCount; 
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor, initialize the semaphore to be set
      /// </summary>
      internal Semaphore() : this(1) {}
 
      /// <summary>
      /// Desult constructor, initialize the semaphore to the passed in count
      /// </summary>
      /// <param name="iCount"></param>
      internal Semaphore(int iCount) { m_iCount = iCount; } 
      #endregion

      #region Public Methods
      /// <summary>
      /// Wait forever for a unit to become available
      /// </summary>
      /// <returns></returns>
      internal bool Wait() 
      { 
         lock(this) 
         { 
            // --- We need to wait in a loop in case someone else wakes up before us. This could ---
            // --- happen if the Monitor.Pulse statements were changed to Monitor.PulseAll ---
            // --- statements in order to introduce some randomness into the order ---
            // --- in which threads are woken. ---

            while(m_iCount <= 0) 
               if (Monitor.Wait(this) == false) 
                  return false; 
            m_iCount--; 
            return true; 
         } 
      } 
      
      /// <summary>
      /// Wait for a unit to become available or until specified time has elapsed
      /// </summary>
      /// <param name="iMilliseconds"></param>
      /// <returns></returns>
      internal bool Wait(int iMilliseconds) 
      { 
         DateTime dtBegin = DateTime.Now; 
         bool     bLockObtained = false; 
         try 
         { 
            if ((bLockObtained = Monitor.TryEnter(this, iMilliseconds)) == true) 
            { 
               // --- Wait until a unit becomes available. We need to wait --- 
               // --- in a loop in case someone else wakes up before us. This could --- 
               // --- happen if the Monitor.Pulse statements were changed to Monitor.PulseAll --- 
               // --- statements in order to introduce some randomness into the order --- 
               // --- in which threads are woken. --- 

               while(m_iCount <= 0) 
               { 
                  if (GetMilliSecondsSince(dtBegin) > iMilliseconds) 
                     return false; 
                  if ((bLockObtained= Monitor.Wait(this, iMilliseconds)) == false) 
                     return false; 
               } 
               m_iCount--; 
               return true; 
            } 
            else 
            { 
               return false; 
            } 
         } 
         finally 
         { 
            if (bLockObtained) Monitor.Exit(this); 
         } 
      } 

      /// <summary>
      /// Release out hold on a unit
      /// </summary>
      internal void Release() 
      { 
         // --- Lock so we can work in peace. This works because lock is actually ---
         // --- built around Monitor. ---
         lock(this) 
         { 
            // --- Release our hold on the unit of control. Then tell everyone ---
            // --- waiting on this object that there is a unit available. ---
            m_iCount++; 
            Monitor.Pulse(this); 
         } 
      }
      #endregion

      #region Private Methods
      /// <summary>
      /// Get the number of milliseconds from a particular time
      /// </summary>
      /// <param name="since"></param>
      /// <returns></returns>
      private int GetMilliSecondsSince(DateTime since) 
      { 
         TimeSpan t; 
         t = since - DateTime.Now; 
         return t.Seconds * 1000 + t.Minutes * 60000 + t.Hours * 3600000; 
      } 
      #endregion
   } 
}
