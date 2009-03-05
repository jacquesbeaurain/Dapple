using System;

namespace WorldWind
{
	/// <summary>
	/// Summary description for TimeKeeper.
	/// </summary>
	public class TimeKeeper
	{
		static System.DateTime m_currentTimeUtc = System.DateTime.Now.ToUniversalTime();
		static float m_timeMultiplier = 1.0f;
		static System.Timers.Timer m_timer = null;
		static float m_interval = 15;

        internal static event System.Timers.ElapsedEventHandler Elapsed;

		  public static System.DateTime CurrentTimeUtc
		{
			get
			{
				return m_currentTimeUtc;
			}
			set
			{
				m_currentTimeUtc = value;
			}
		}

		internal static float TimeMultiplier
		{
			get{ return m_timeMultiplier; }
			set{ m_timeMultiplier = value; }
		}

		public static void Start()
		{
			if(m_timer == null)
			{
				m_timer = new System.Timers.Timer(m_interval);
				m_timer.Elapsed += new System.Timers.ElapsedEventHandler(m_timer_Elapsed);
			}
			m_timer.Start();
		}

		internal static void Stop()
		{
			if(m_timer != null)
				m_timer.Stop();
		}

		private static void m_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			m_currentTimeUtc += System.TimeSpan.FromMilliseconds( m_interval * m_timeMultiplier );

            if (Elapsed != null)
                Elapsed(sender, e);
		}
	}
}
