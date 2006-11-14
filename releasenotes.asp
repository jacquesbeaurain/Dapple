<%
Dim title
title="Release Notes"
%>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>Dapple Project Release Notes</title>
<link rel="stylesheet" type="text/css" href="/styles/styles-dapple.css"/>
</head>
<body>
<!--#include virtual="/_private/dapple/dapple1.asp"-->
   <p>
      <strong>A note on the Dapple versioning policy:</strong> <em>Builds with even minor numbers are considered releases or release candidates (e.g. 1.0.4, 1.0.6 and 1.0.8). Odd numbered released are used for developer testing and distribution (e.g. 1.0.5 and 1.0.7).</em></p>	
	<h2><a name="1010">1.0.10 Release Notes (Tue, 14 November 2006)</a></h2>
	<h3>Bugs Fixed</h3>
   <ul>
      <li>Fix crash that occured fairly often, especially looking at Dapple views with many layers and downloads in progress.</li>
      <li>Failed WMS catalogs could not be removed from the tree.</li>
      <li>Refresh overview window contents when resized horizontally.</li>
   </ul>
	<h2><a name="106">1.0.6 Release Notes (Mon, 31 July 2006)</a></h2>
	<h3>New Features</h3>
   <ul>
	   <li>Install now supports Windows 2000</li>
	   <li>New command line flag for displaying temporary GeoTIF files</li>
	   <li>Better DAP dataset metadata stylesheet</li>
	   <li>WMS server metadata includes dataset lists</li>
	   <li>WMS individual dataset metadata support</li>
   </ul>
	<h3>Bugs Fixed</h3>
   <ul>
	   <li>WMS capability URL problems</li>
	   <li>False positive Win32.KME virus detection of listgeo.exe command line utility</li>
	   <li>Fix abort refreshing/clearing WMS layer cache</li>
	   <li>"Error in the application." exception some people are still getting in Microsoft.DirectX.Direct3D.Device.Reset</li>
   </ul>
	<h2><a name="104">1.0.4 - Initial Public Release (Mon, 24 July 2006)</a></h2>
   <p>&nbsp;</p>

</body>
</html>