﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SevenDigital.Api.Schema.ReleaseEndpoint;

namespace SevenDigital.Api.Schema.LockerEndpoint
{
	[Serializable]
	[XmlRoot("lockerRelease")]
	public class LockerRelease
	{
		[XmlElement("release")]
		public Release Release { get; set; }

		[XmlArray("lockerTracks")]
		[XmlArrayItem("lockerTrack")]
		public List<LockerTrack> LockerTracks { get; set; }

		[XmlElement("available")]
		public bool Available { get; set; }
	}
}