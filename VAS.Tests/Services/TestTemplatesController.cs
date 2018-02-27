//
//  Copyright (C) 2017 Fluendo S.A.
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Serialization;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;

namespace VAS.Tests.Services
{
	[TestFixture ()]
	public class TestTemplatesController
	{
		DummyTemplatesController templatesController;
		Mock<IDialogs> mockDialogs;
		string tempFile;

		[SetUp]
		public async Task SetUp ()
		{
			templatesController = new DummyTemplatesController ();
			await templatesController.Start ();
			mockDialogs = new Mock<IDialogs> ();
			App.Current.Dialogs = mockDialogs.Object;
			mockDialogs.Setup (m => m.QuestionMessage (It.IsAny<string> (), null, It.IsAny<DummyTemplatesController> ())).Returns (AsyncHelpers.Return (true));
			mockDialogs.Setup (m => m.QueryMessage (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (), It.IsAny<DummyTemplatesController> ())).Returns (AsyncHelpers.Return ("Test Team #2_copy"));
		}

		[TearDown]
		public async Task TearDown ()
		{
			await templatesController.Stop ();
			try {
				File.Delete (tempFile);
			} catch {
			}
		}

		[Test ()]
		public void TestHandleSelectionChanged_LoadedTemplateWithTeamAndPlayer ()
		{
			// Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam ();
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };

			templatesController.ViewModel.ViewModels.Add (teamVM);

			// Action
			templatesController.ViewModel.Selection.Replace (templatesController.ViewModel.ViewModels);

			// Assert
			Assert.IsNotNull (templatesController.ViewModel.LoadedItem,
						   "Loaded template model");
			Assert.IsNotEmpty (templatesController.ViewModel.LoadedItem.SubViewModel,
						   "Loaded template subVM");
		}

		[Test ()]
		public void TestHandleSelectionChanged_ClearSelectionWithTeamAndPlayer ()
		{
			// Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam ();
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };

			templatesController.ViewModel.ViewModels.Add (teamVM);
			templatesController.ViewModel.Selection.Replace (templatesController.ViewModel.ViewModels);

			// Action
			templatesController.ViewModel.Selection.Clear ();

			// Assert
			Assert.IsNull (templatesController.ViewModel.LoadedItem.Model,
						   "Loaded template model");
			Assert.IsEmpty (templatesController.ViewModel.LoadedItem.SubViewModel,
						   "Loaded template subVM");
		}

		[Test ()]
		public void TestHandleSave_ForcedNameChanged ()
		{
			// Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam ();
			team.Name = "Test Team #1";
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };

			templatesController.ViewModel.ViewModels.Add (teamVM);

			DummyTeam team2 = team.Clone ();
			string expectedName = "Test Team #2";
			team2.Name = expectedName;

			// Action
			App.Current.EventsBroker.Publish (new UpdateEvent<Team> () {
				Object = team2,
				Force = true
			});

			// Assert
			Assert.AreEqual (templatesController.ViewModel.ViewModels.First (x => x.ID == team.ID).Name,
						   expectedName);
			Assert.IsFalse (templatesController.ViewModel.SaveCommand.CanExecute ());
		}

		[Test ()]
		public void TestHandleSave_NonForcedNameChanged ()
		{
			// Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam ();
			team.Name = "Test Team #1";
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };

			templatesController.ViewModel.ViewModels.Add (teamVM);

			DummyTeam team2 = team.Clone ();
			string expectedName = "Test Team #2";
			team2.Name = expectedName;

			// Action
			App.Current.EventsBroker.Publish (new UpdateEvent<Team> () {
				Object = team2,
				Force = false
			});

			// Assert
			mockDialogs.Verify (m => m.QuestionMessage (It.IsAny<string> (), null, It.IsAny<DummyTemplatesController> ()), Times.Once ());
			Assert.AreEqual (templatesController.ViewModel.ViewModels.First (x => x.ID == team.ID).Name,
						   expectedName);
			Assert.IsFalse (templatesController.ViewModel.SaveCommand.CanExecute ());
		}

		[Test ()]
		public void TestHandleSave_SaveStatic ()
		{
			// Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam ();
			team.Name = "Test Team #1";
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };

			templatesController.ViewModel.ViewModels.Add (teamVM);

			DummyTeam team2 = team.Clone ();
			string expectedName = "Test Team #2";
			team2.Name = expectedName;
			team2.Static = true;
			var evt = new UpdateEvent<Team> () {
				Object = team2,
				Force = true
			};

			// Action
			App.Current.EventsBroker.Publish (evt);

			// Assert
			mockDialogs.Verify (m => m.QuestionMessage (It.IsAny<string> (), null, It.IsAny<DummyTemplatesController> ()), Times.Once ());
		}

		public TeamVM PrepareExport ()
		{
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam ();
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };
			templatesController.ViewModel.ViewModels.Add (teamVM);
			templatesController.ViewModel.Selection.Add (teamVM);
			tempFile = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			tempFile = Path.ChangeExtension (tempFile, null);
			mockDialogs.Setup (m => m.SaveFile (It.IsAny<string> (), It.IsAny<string> (),
												It.IsAny<string> (), It.IsAny<string> (),
												It.IsAny<string []> ())).
					   Returns (tempFile);
			return teamVM;
		}

		[Test ()]
		public void TestExport_TemplateSelectedToNewFile_Exported ()
		{
			// Arrange
			TeamVM team = PrepareExport ();

			// Act
			templatesController.ViewModel.ExportCommand.Execute ();
			var exportedTeam = Serializer.Instance.Load<DummyTeam> (tempFile);

			// Assert
			Assert.AreEqual (team.Model, exportedTeam);
		}

		[Test ()]
		public void TestExport_TemplateSelected_Overwrite ()
		{
			// Arrange
			TeamVM team = PrepareExport ();
			using (var stream = File.AppendText (tempFile)) {
				stream.WriteLine ("FOO");
			}

			// Act
			templatesController.ViewModel.ExportCommand.Execute ();
			var exportedTeam = Serializer.Instance.Load<DummyTeam> (tempFile);

			// Assert
			Assert.AreEqual (team.Model, exportedTeam);
		}

		[Test ()]
		public void TestExport_TemplateSelected_NotOverwrite ()
		{
			// Arrange
			TeamVM team = PrepareExport ();
			using (var stream = File.AppendText (tempFile)) {
				stream.WriteLine ("FOO");
			}
			mockDialogs.Setup (m => m.QuestionMessage (It.IsAny<string> (), null,
													   It.IsAny<DummyTemplatesController> ())).
					   Returns (AsyncHelpers.Return (false));


			templatesController.ViewModel.ExportCommand.Execute ();

			// Assert
			Assert.IsTrue (File.Exists (tempFile));
			Assert.Throws<JsonReaderException> (() => Serializer.Instance.Load<DummyTeam> (tempFile));
		}

		[Test ()]
		public void TestExport_WithoutTemplateSelected_NotExported ()
		{
			tempFile = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			tempFile = Path.ChangeExtension (tempFile, null);
			mockDialogs.Setup (m => m.SaveFile (It.IsAny<string> (), It.IsAny<string> (),
												It.IsAny<string> (), It.IsAny<string> (),
												It.IsAny<string []> ())).
					   Returns (tempFile);

			// Act
			templatesController.ViewModel.ExportCommand.Execute ();

			// Assert
			Assert.IsFalse (File.Exists (tempFile));
		}

		[Test]
		public void Search_TemplateExist_TemplateFound ()
		{
			//Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam {
				Name = "Dummy"
			};
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };

			templatesController.ViewModel.ViewModels.Add (teamVM);

			//Act
			var evt = new SearchEvent {
				TextFilter = "Dummy"
			};

			// Action
			App.Current.EventsBroker.Publish (evt);


			//Assert
			Assert.AreEqual (1, templatesController.ViewModel.VisibleViewModels.Count);
			Assert.AreEqual ("Dummy", templatesController.ViewModel.VisibleViewModels.FirstOrDefault ().Name);
		}

		[Test]
		public void Search_TemplateThatDoesntExist_VisibleViewModelsEmpty ()
		{
			//Arrange
			Utils.PlayerDummy player = new Utils.PlayerDummy ();
			DummyTeam team = new DummyTeam {
				Name = "Dummy"
			};
			team.List.Add (player);
			TeamVM teamVM = new TeamVM { Model = team };

			templatesController.ViewModel.ViewModels.Add (teamVM);

			//Act
			var evt = new SearchEvent {
				TextFilter = "This doesn't exist"
			};

			// Action
			App.Current.EventsBroker.Publish (evt);


			//Assert
			Assert.AreEqual (0, templatesController.ViewModel.VisibleViewModels.Count);
		}
	}
}
