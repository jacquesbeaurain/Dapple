using System;

namespace Geosoft.GX.DAPGetData
{
   /// <summary>
   /// Server select event delegate
   /// </summary>
   internal delegate void ServerSelectHandler(object sender, Server e);   

   /// <summary>
   /// Server remove event delegate
   /// </summary>
   internal delegate void RemoveServerHandler(object sender, Server e);

   /// <summary>
   /// Server login event delegate
   /// </summary>
   internal delegate void ServerLoggedInHandler(object sender, Server e);

   /// <summary>
   /// Server cache changed
   /// </summary>
   internal delegate void ServerCachedChangedHandler(object sender, Server e);
}
