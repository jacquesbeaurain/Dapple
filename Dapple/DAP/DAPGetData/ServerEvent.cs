using System;

namespace Geosoft.GX.DAPGetData
{
   /// <summary>
   /// Server select event delegate
   /// </summary>
   public delegate void ServerSelectHandler(object sender, Server e);   

   /// <summary>
   /// Server remove event delegate
   /// </summary>
   public delegate void RemoveServerHandler(object sender, Server e);

   /// <summary>
   /// Server login event delegate
   /// </summary>
   public delegate void ServerLoggedInHandler(object sender, Server e);

   /// <summary>
   /// Server cache changed
   /// </summary>
   public delegate void ServerCachedChangedHandler(object sender, Server e);
}
