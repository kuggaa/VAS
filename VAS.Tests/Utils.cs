//
//  Copyright (C) 2014 Andoni Morales Alastruey
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
//using LongoMatch.Core.Store;
//using LongoMatch.Core.Store.Templates;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using System.Collections.ObjectModel;

namespace VAS.Tests
{
	public static class Utils
	{
		public class PlayerDummy : Player
		{
			//dummy class for abstract validation. Copied from LongoMatch and adapted to VAS.
		}

		public class DashboardDummy : Dashboard
		{
			//dummy class for abstract validation. Copied from LongoMatch and adapted to VAS.
		}

		//dummy class for abstract validation. Copied ffrom LongoMatch and adapted to VAS.
		public class ProjectDummy : Project
		{
			#region implemented abstract members of Project

			public override TimelineEvent AddEvent (EventType type, Time start, Time stop, Time eventTime, Image miniature, bool addToTimeline = true)
			{
				TimelineEvent evt;
				string count;
				string name;

				count = String.Format ("{0:000}", EventsByType (type).Count + 1);
				name = String.Format ("{0} {1}", type.Name, count);
				evt = new TimelineEvent ();

				evt.Name = name;
				evt.Start = start;
				evt.Stop = stop;
				evt.EventTime = eventTime;
				evt.EventType = type;
				evt.Notes = "";
				evt.Miniature = miniature;
				evt.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
				evt.FileSet = FileSet;
				evt.Project = this;

				if (addToTimeline) {
					Timeline.Add (evt);
				}

				return evt;				
			}

			public override void AddEvent (TimelineEvent play)
			{
				play.FileSet = FileSet;
				play.Project = this;
				Timeline.Add (play);

			}

			#endregion
		}


		static bool debugLine = false;

		public static T SerializeDeserialize<T> (T obj)
		{
			var stream = new MemoryStream ();
			Serializer.Instance.Save (obj, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			if (debugLine) {
				var jsonString = new StreamReader (stream).ReadToEnd ();
				Console.WriteLine (jsonString);
			}
			stream.Seek (0, SeekOrigin.Begin);
			
			return Serializer.Instance.Load<T> (stream, SerializationType.Json);
		}

		public static void CheckSerialization<T> (T obj, bool ignoreIsChanged = false)
		{
			List<IStorable> children, changed;

			if (!ignoreIsChanged) {
				Assert.IsInstanceOf<IChanged> (obj);
			}
			var stream = new MemoryStream ();
			Serializer.Instance.Save (obj, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			var jsonString = new StreamReader (stream).ReadToEnd ();
			if (debugLine) {
				Console.WriteLine (jsonString);
			}
			stream.Seek (0, SeekOrigin.Begin);
			
			var newobj = Serializer.Instance.Load<T> (stream, SerializationType.Json);
			if (!ignoreIsChanged) {
				ObjectChangedParser parser = new ObjectChangedParser ();
				if (obj is IStorable) {
					StorableNode parentNode;
					Assert.IsTrue (parser.ParseInternal (out parentNode, newobj as IStorable, Serializer.JsonSettings));
					Assert.IsFalse (parentNode.HasChanges ());
				} else {
					Assert.IsFalse ((newobj as IChanged).IsChanged);
				}
			}

			stream = new MemoryStream ();
			Serializer.Instance.Save (newobj, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			var newJsonString = new StreamReader (stream).ReadToEnd ();
			if (debugLine) {
				Console.WriteLine (newJsonString);
			}
			Assert.AreEqual (jsonString, newJsonString);
		}

		public static Image LoadImageFromFile (bool scaled = false)
		{
			Image img = null;
			string tmpFile = Path.GetTempFileName ();

			using (Stream resource = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("dibujo.svg")) {
				using (Stream output = File.OpenWrite (tmpFile)) {
					resource.CopyTo (output);
				}
			}
			try {
				if (!scaled) {
					img = new Image (tmpFile);
				} else {
					img = new Image (tmpFile);
					img.ScaleInplace (20, 20);
				}
			} catch (Exception ex) {
				Assert.Fail (ex.Message);
			} finally {
				File.Delete (tmpFile);
			}
			return img;
		}


		public static Project CreateProject (bool withEvents = true)
		{
			TimelineEvent pl;
			Project p = new ProjectDummy ();
			p.FileSet = new MediaFileSet ();
			p.FileSet.Add (new MediaFile (Path.GetTempFileName (), 34000, 25, true, true, "mp4", "h264",
				"aac", 320, 240, 1.3, null, "Test asset 1"));
			p.FileSet.Add (new MediaFile (Path.GetTempFileName (), 34000, 25, true, true, "mp4", "h264",
				"aac", 320, 240, 1.3, null, "Test asset 2"));
			p.UpdateEventTypesAndTimers ();

			if (withEvents) {
				AnalysisEventButton b = p.Dashboard.List [0] as AnalysisEventButton;

				/* No tags, no players */
				pl = new TimelineEvent {
					EventType = b.EventType,
					Start = new Time (0),
					Stop = new Time (100),
					FileSet = p.FileSet
				};
				p.Timeline.Add (pl);
				/* tags, but no players */
				b = p.Dashboard.List [1] as AnalysisEventButton;
				pl = new TimelineEvent {
					EventType = b.EventType,
					Start = new Time (0),
					Stop = new Time (100),
					FileSet = p.FileSet
				};
				pl.Tags.Add (b.AnalysisEventType.Tags [0]);
				p.Timeline.Add (pl);
				/* tags and players */
				b = p.Dashboard.List [2] as AnalysisEventButton;
				pl = new TimelineEvent {
					EventType = b.EventType,
					Start = new Time (0),
					Stop = new Time (100),
					FileSet = p.FileSet
				};
				pl.Tags.Add (b.AnalysisEventType.Tags [1]);
				p.Timeline.Add (pl);
			}

			return p;
		}

		public static void DeleteProject (Project p)
		{
			foreach (MediaFile mf in p.FileSet) {
				if (File.Exists (mf.FilePath)) {
					File.Delete (mf.FilePath);
				}
			}
		}

		public static void AreEquals (IStorable obj1, IStorable obj2, bool areEquals = true)
		{
			var stream = new MemoryStream ();
			Serializer.Instance.Save (obj1, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			var obj1Str = new StreamReader (stream).ReadToEnd ();
			stream = new MemoryStream ();
			Serializer.Instance.Save (obj2, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			var obj2Str = new StreamReader (stream).ReadToEnd ();
			if (areEquals) {
				Assert.AreEqual (obj1Str, obj2Str);
			} else {
				Assert.AreNotEqual (obj1Str, obj2Str);
			}
		}

		public static string SaveResource (string name, string path)
		{
			string filePath;
			var assembly = Assembly.GetExecutingAssembly ();
			using (Stream inS = assembly.GetManifestResourceStream (name)) {
				filePath = Path.Combine (path, name);
				using (Stream outS = new FileStream (filePath, FileMode.Create)) {
					inS.CopyTo (outS);
				}
			}
			return filePath;
		}
	}
}

