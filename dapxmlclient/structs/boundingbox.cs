using System;

namespace Geosoft.Dap.Common
{
	/// <summary>
	/// Define an rectangular area within a particular coordinate system
	/// </summary>
	[Serializable]
	public class BoundingBox
	{
		#region Statics
		/// <summary>
		/// Equivalent to GXNet.Constant.rDUMMY
		/// </summary>
		public const double DOUBLE_DUMMY = -1.0E32;
		#endregion

		#region Member Variables
		/// <summary>
		/// Maximum x coordinate
		/// </summary>
		protected double m_dMaxX;

		/// <summary>
		/// Maximum y coordinate
		/// </summary>
		protected double m_dMaxY;

		/// <summary>
		/// Maximum z coordinate
		/// </summary>
		protected double m_dMaxZ;

		/// <summary>
		/// Minimum x coordiante
		/// </summary>
		protected double m_dMinX;

		/// <summary>
		/// Minimum y coordinate
		/// </summary>
		protected double m_dMinY;

		/// <summary>
		/// Minimum z coordinate
		/// </summary>
		protected double m_dMinZ;

		/// <summary>
		/// The coordinate system of the bounding box
		/// </summary>
		protected CoordinateSystem m_hCoordinateSystem;
		#endregion

		#region Properties
		/// <summary>
		/// Get or set the maximum x coordinate
		/// </summary>
		public double MaxX
		{
			set { m_dMaxX = value; }
			get { return m_dMaxX; }
		}

		/// <summary>
		/// Get or set the maximum y coordinate
		/// </summary>
		public double MaxY
		{
			set { m_dMaxY = value; }
			get { return m_dMaxY; }
		}

		/// <summary>
		/// Get or set the maximum z coordinate
		/// </summary>
		public double MaxZ
		{
			set { m_dMaxZ = value; }
			get { return m_dMaxZ; }
		}

		/// <summary>
		/// Get or set the minimum x coordinate
		/// </summary>
		public double MinX
		{
			set { m_dMinX = value; }
			get { return m_dMinX; }
		}

		/// <summary>
		/// Get or set the minimum y coordinate
		/// </summary>
		public double MinY
		{
			set { m_dMinY = value; }
			get { return m_dMinY; }
		}

		/// <summary>
		/// Get or set the minimum z coordinate
		/// </summary>
		public double MinZ
		{
			set { m_dMinZ = value; }
			get { return m_dMinZ; }
		}

		/// <summary>
		/// Get or set the coordinate system
		/// </summary>
		public CoordinateSystem CoordinateSystem
		{
			set { m_hCoordinateSystem = value; }
			get { return m_hCoordinateSystem; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Default construtor
		/// </summary>
		public BoundingBox()
		{
			m_dMaxX = DOUBLE_DUMMY;
			m_dMaxY = DOUBLE_DUMMY;
			m_dMinX = DOUBLE_DUMMY;
			m_dMinY = DOUBLE_DUMMY;
			m_dMinZ = DOUBLE_DUMMY;
			m_dMaxZ = DOUBLE_DUMMY;

			// --- set the default coordinate system to WGS 84 ---
			m_hCoordinateSystem = new CoordinateSystem();
			m_hCoordinateSystem.Datum = "WGS 84";
		}

		/// <summary>
		/// Extended constructor
		/// </summary>
		/// <param name="dMaxX">The maximum x coordinate</param>
		/// <param name="dMaxY">The maximum y coordinate</param>
		/// <param name="dMinX">The minimum x coordiante</param>
		/// <param name="dMinY">The minimum y coordinate</param>
		public BoundingBox(double dMaxX, double dMaxY, double dMinX, double dMinY)
		{
			m_dMaxX = dMaxX;
			m_dMaxY = dMaxY;
			m_dMaxZ = DOUBLE_DUMMY;
			m_dMinX = dMinX;
			m_dMinY = dMinY;
			m_dMinZ = DOUBLE_DUMMY;

			// --- set the default coordinate system to WGS 84 ---
			m_hCoordinateSystem = new CoordinateSystem();
			m_hCoordinateSystem.Datum = "WGS 84";
		}

		/// <summary>
		/// Extended constructor
		/// </summary>
		/// <param name="dMaxX">The maximum x coordinate</param>
		/// <param name="dMaxY">The maximum y coordinate</param>
		/// <param name="dMaxZ">The maximum z coordinate</param>
		/// <param name="dMinX">The minimum x coordiante</param>
		/// <param name="dMinY">The minimum y coordinate</param>
		/// <param name="dMinZ">The minimum z coordinate</param>
		public BoundingBox(double dMaxX, double dMaxY, double dMaxZ, double dMinX, double dMinY, double dMinZ)
		{
			m_dMaxX = dMaxX;
			m_dMaxY = dMaxY;
			m_dMaxZ = dMaxZ;
			m_dMinX = dMinX;
			m_dMinY = dMinY;
			m_dMinZ = dMinZ;

			// --- set the default coordinate system to WGS 84 ---
			m_hCoordinateSystem = new CoordinateSystem();
			m_hCoordinateSystem.Datum = "WGS 84";
		}

		/// <summary>
		/// Extended constructor
		/// </summary>
		/// <param name="hBox">Bounding Box to copy</param>
		public BoundingBox(BoundingBox hBox)
		{
			m_dMaxX = hBox.m_dMaxX;
			m_dMaxY = hBox.m_dMaxY;
			m_dMaxZ = hBox.m_dMaxZ;
			m_dMinX = hBox.m_dMinX;
			m_dMinY = hBox.m_dMinY;
			m_dMinZ = hBox.m_dMinZ;

			// --- set the default coordinate system to WGS 84 ---
			m_hCoordinateSystem = new CoordinateSystem(hBox.CoordinateSystem);
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Check to see if the 3d extents are set
		/// </summary>
		/// <returns></returns>
		public bool bIs3D()
		{
			if (m_dMinZ != DOUBLE_DUMMY && m_dMaxZ != DOUBLE_DUMMY)
				return true;
			return false;
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Get the hash code for this object
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return m_dMaxX.GetHashCode() ^ m_dMaxY.GetHashCode() ^ m_dMaxZ.GetHashCode() ^ m_dMinX.GetHashCode() ^ m_dMinY.GetHashCode() ^ m_dMinZ.GetHashCode() ^ m_hCoordinateSystem.GetHashCode();
		}

		/// <summary>
		/// Compare two bounding box objects
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj == null) return false;

			if (obj.GetType() == typeof(BoundingBox))
			{
				BoundingBox hBB = (BoundingBox)obj;

				if (hBB.m_dMaxX == m_dMaxX && hBB.m_dMaxY == m_dMaxY && hBB.m_dMaxZ == m_dMaxZ && hBB.m_dMinX == m_dMinX && hBB.m_dMinY == m_dMinY && m_dMinZ == hBB.m_dMinZ && m_hCoordinateSystem == hBB.m_hCoordinateSystem)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Overload == operator
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns></returns>
		public static bool operator ==(BoundingBox c1, BoundingBox c2)
		{
			if ((object)c1 == null && (object)c2 == null) return true;
			if ((object)c1 == null) return false;

			return c1.Equals(c2);
		}

		/// <summary>
		/// Overload != operator
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns></returns>
		public static bool operator !=(BoundingBox c1, BoundingBox c2)
		{
			if ((object)c1 == null && (object)c2 == null) return false;
			if ((object)c1 == null) return true;

			return !c1.Equals(c2);
		}
		#endregion
	}
}
