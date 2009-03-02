//#define VERBOSE
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Utility;

namespace WorldWind.DataSource
{
    /// <summary>
    /// Callback function should return a valid stream if one could be opened for a cached version
    /// of the data request. Using a callback to achieve allows requests to be scheduled even if
    /// they result in a cache hit. Note that the callback will be called from the background worker
    /// thread!
    /// </summary>
    /// <param name="drd">The Data Request Descriptor that should be located in the cache</param>
    /// <returns>A stream prepared to read the cached data, or null if the request was not found
    /// in the cache (or was too old).</returns>
    internal delegate Stream CacheCallback(DataRequestDescriptor drd);

    /// <summary>
    /// Callback function to be called upon completion of the data request - either from cache
    /// or external data source. Caution: This function will be called from the background worker
    /// thread!
    /// </summary>
    /// <param name="drd">The complete data request</param>
    internal delegate void CompletionCallback(DataRequest dr);

    /// <summary>
    /// Callback function to return current download priority of the request. Priority should be
    /// in the range of 0 (lowest) to 100 (highest) priority. Requests with negative priority 
    /// are removed from the download queue.
    /// </summary>
    /// <returns>Download priority. Negative = remove from queue; 0-100 = lowest to highest priority.</returns>
    internal delegate float PriorityCallback();

	 public enum DataRequestState
    {
        Queued,
        NoCache,
        InProcess,
        Finished,
        Error,
        Delayed,
        Cancelled
    }

    /// <summary>
    /// The descriptor for new data requests.
    /// </summary>
	 public struct DataRequestDescriptor
    {
        /// <summary>
        /// The URL to load (in the future, this will also allow piping from external applications).
        /// Must not be null.
        /// </summary>
        internal string Source;
        /// <summary>
        /// Relative path to cache the data in (%WW_HOME/Cache/ will be automatically prefixed). Can be null
        /// if no caching should be performed (eg. for file access).
        /// </summary>
        internal string CacheLocation;
        /// <summary>
        /// Maximum age of the cached file. Can be used to override the default for frequently updated data.
        /// </summary>
        internal TimeSpan? MaxCacheAge;

        /// <summary>
        /// A human readable description of what this data request is for.
        /// </summary>
        internal string Description;

        internal int BasePriority;

        internal CacheCallback CacheCallback;

        internal CompletionCallback CompletionCallback;

        internal PriorityCallback PriorityCallback;

        internal DataRequestDescriptor(string source, string cacheLocation, CacheCallback callback) : this(source, cacheLocation)
        {
            CacheCallback = callback;
        }

        internal DataRequestDescriptor(string source, string cacheLocation)
        {
            this.Source = source;
            this.CacheLocation = cacheLocation;
            // cache never expires by default
            MaxCacheAge = null;
            CacheCallback = null;
            CompletionCallback = null;
            PriorityCallback = null;
            BasePriority = 0;
            Description = "";
        }
    }

    /// <summary>
    /// A 'handle' to the current state of a data request. May be polled from other threads.
    /// </summary>
	 public abstract class DataRequest : IComparable
    {
        #region Members
        protected Object m_lock;
        protected DataRequestDescriptor m_request;
        protected Stream m_contentStream;
        protected DataRequestState m_state;
        protected bool m_cacheHit;
        protected NameValueCollection m_headers;
        protected float m_priority;
        protected DateTime m_nextTry;
        #endregion

        #region Properties
        internal DataRequestDescriptor RequestDescriptor { get { return m_request; } }
        internal NameValueCollection Headers
        {
            get
            {
                return m_headers;
            }
        }
        internal String Source
        {
            get
            {
                return m_request.Source;
            }
        }
        internal DataRequestState State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }
        internal Stream Stream
        {
            get
            {
                return m_contentStream;
            }
        }
        internal string CacheLocation
        {
            get
            {
                return m_request.CacheLocation;
            }
        }
        internal bool CacheHit
        {
            get
            {
                return m_cacheHit;
            }
        }
        internal float Priority
        {
            get
            {
                return m_priority;
            }
        }
        internal DateTime NextTry
        {
            get { return m_nextTry; }
            set { m_nextTry = value; }
        }
        abstract internal float Progress
        {
            get;
        }

        #endregion

        #region Static Members
        static protected int m_cacheHits = 0;
        static protected int m_totalRequests = 0;
        static protected int m_totalBytes = 0;
        #endregion


        #region Static Properties
        public static int CacheHits { get { return m_cacheHits; } }
        public static int TotalRequests { get { return m_totalRequests; } }
        public static int TotalBytes { get { return m_totalBytes; } }
        #endregion

        internal DataRequest(DataRequestDescriptor request)
        {
            m_lock = new Object();
            m_request = request;
            m_contentStream = null;
            m_state = DataRequestState.Queued;
            m_cacheHit = false;
            m_headers = new NameValueCollection();
            m_priority = 50;
            m_totalRequests++;
            m_nextTry = DateTime.Now;
        }

        internal void UpdatePriority()
        {
            // postponed?
            if (m_nextTry > DateTime.Now)
            {
                m_priority = -1;
                return;
            }

            if (m_request.PriorityCallback != null)
                m_priority = m_request.PriorityCallback();
            else
                m_priority = 50;
        }

        internal void Cancel()
        {
            this.State = DataRequestState.Cancelled;
        }

        internal abstract void Start();

        /// <summary>
        /// Try to fulfill the request from cache, if a cache directory has been set.
        /// If the request can be served from cache, the content stream is initialised for the cache source,
        /// and the state is set to finished.
        /// </summary>
        /// <returns>True if the request could be served from the cache.</returns>
        internal bool TryCache()
        {
            try
            {
#if VERBOSE
            Log.Write(Log.Levels.Verbose, "DataRequest: trying to fulfill request for " + this.Source + " from cache");
            Log.Write(Log.Levels.Verbose, "DataRequest: trying " + m_request.CacheLocation);
#endif
                if (m_request.CacheCallback != null)
                {
#if VERBOSE
                   Log.Write(Log.Levels.Verbose, "DataRequest: asking cache callback for file stream");
#endif
                    m_contentStream = m_request.CacheCallback(m_request); //new FileStream(m_request.CacheLocation, FileMode.Open);
                    if (m_contentStream != null)
                    {
                        m_state = DataRequestState.Finished;
                        m_cacheHit = true;

                        m_cacheHits++;
                        return true;
                    }
                }
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }

        #region IComparable Members

		  public int CompareTo(object obj)
        {
            DataRequest dr = obj as DataRequest;
            if(dr == null)
                return 0;

            // sort delayed stuff to the back
            if (this.NextTry > DateTime.Now) return 1;
            if (dr.NextTry > DateTime.Now)   return -1;

            // sort by base priority first
            if (this.m_request.BasePriority != dr.m_request.BasePriority)
                return -Math.Sign(this.m_request.BasePriority - dr.m_request.BasePriority);
            
            // reverse sort order to start with high priority downloads
            return -Math.Sign(this.m_priority - dr.m_priority);
        }

        #endregion
    }
}
