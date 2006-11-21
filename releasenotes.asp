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
	<h2>1.0.8 Release Notes (Mon, 16 October 2006)</h2>
	<h3>New Features and Changes</h3>
   <ul>
      <li>Improve the estimation of current view extents (visually apparent with the red area on the overview map) which improves GeoTIFF export accuracy as well as filtering of datasets to what is visible in the view. </li>
      <li>Get rid of the home view concept and solve problems with people loosing servers they last worked with by always opening the last view. The question presented to the user still asks whether they want to see the last view or not (with an option to not ask again/control in the settings menu). When they say "yes" both the servers and the layers are loaded and the camera view point is changed. On "No" it just loads the last servers without the layers and does not change the camera view. </li>
      <li>Added support for GeoTIFF files with georeferenced coordinate systems which are equivalent to WGS 84 for Dapple display purposes (e.g. NAD83). </li>
      <li>Enhancements to the server tree inside Dapple to give better visual feedback and unclutter the tree view during browsing of catalogs. The tree collapses and expands as necessary and gives visual clues as to where and how many results (WMS and DAP) are available bot during searches and unfiltered browsing. For 6.3.1 DAP servers this should also allow for offline usage of Dapple and viewing of DAP layers in the cache. </li>
   </ul>
	<h3>Breaking Changes</h3>
   <ul>
      <li>Unfortunately DAP layers in Dapple views from previous Dapple release will no longer
         load because of a change in the format which was needed to implement the server tree changes. An appropriate message is displayed to the user during loading of views with old layers. </li>
   </ul>
	<h3>Bugs Fixed</h3>
   <ul>
      <li>Fix communication with DAP and WMS servers through proxy servers by making sure that the proxy options are used in all communications.</li><li>Broken WMS catalogs because XLink paths were not properly resolved during download time.</li><li>Crash in WMS server code where layers with incomplete ImageFormats. </li>
      <li>Crash viewing corrupted metadata from servers. </li>
      <li>A couple of other minor bugfixes found during development. </li>
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