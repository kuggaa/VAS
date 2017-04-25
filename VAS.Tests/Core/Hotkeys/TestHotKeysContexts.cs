using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using Timer = System.Timers.Timer;

namespace VAS.Tests.Core.HotKeys
{
	[TestFixture ()]
	public class TestHotKeysContexts
	{
		const string PLAY_ACTION = "PLAY";
		const string FORWARD_ACTION = "FORWARD";
		const string FAST_FORWARD_ACTION = "FAST_FORWARD";
		const string GLOBAL_ACTION = "GLOBAL";

		HotKey spaceHotkey = App.Current.Keyboard.ParseName ("space");
		HotKey rightHotkey = App.Current.Keyboard.ParseName ("Right");
		HotKey shiftRightHotkey = App.Current.Keyboard.ParseName ("<Shift_L>+Right");
		HotKey globalHotkey = App.Current.Keyboard.ParseName ("G");
		int countPlay = 0, countForward = 0, countFastForward = 0, countGlobal = 0;
		KeyAction play, forward, fastForward, globalAction;
		Mock<IGUIToolkit> mockToolkit;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			InitKeyActions ();
			App.Current.KeyContextManager.GlobalKeyContext.KeyActions.Clear ();
		}

		[SetUp ()]
		public void SetUp ()
		{
			countPlay = countForward = countFastForward = countGlobal = 0;
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> ());

			mockToolkit = new Mock<IGUIToolkit> ();
			App.Current.GUIToolkit = mockToolkit.Object;

			App.Current.DependencyRegistry.Register<ITimer, Timer> (1);
		}

		[TearDown ()]
		public void TearDown ()
		{
			App.Current.KeyContextManager.GlobalKeyContext.KeyActions.Clear ();
		}

		void InitKeyActions ()
		{
			play = new KeyAction (new KeyConfig { Name = PLAY_ACTION, Key = spaceHotkey },
				() => countPlay++);
			forward = new KeyAction (new KeyConfig { Name = FORWARD_ACTION, Key = rightHotkey },
				() => countForward++);
			fastForward = new KeyAction (new KeyConfig { Name = FAST_FORWARD_ACTION, Key = shiftRightHotkey },
				() => countFastForward++);
			globalAction = new KeyAction (new KeyConfig { Name = GLOBAL_ACTION, Key = globalHotkey },
				() => countGlobal++);

		}

		[Test ()]
		public void TestKeyContextWithAllActions ()
		{
			KeyContext context = new KeyContext ();
			context.AddAction (play);
			context.AddAction (forward);
			context.AddAction (fastForward);
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });

			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (rightHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (shiftRightHotkey);

			Assert.AreEqual (2, countPlay);
			Assert.AreEqual (1, countForward);
			Assert.AreEqual (1, countFastForward);

		}

		[Test ()]
		public void TestKeyContextWithOnlyPlayAction ()
		{
			KeyContext context = new KeyContext ();
			context.AddAction (play);
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });

			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (rightHotkey);

			Assert.AreEqual (1, countPlay);
			Assert.AreEqual (0, countForward);
		}

		[Test ()]
		public void TestHotkeyFallbacksCorrectly ()
		{
			bool fallback = false;
			VAS.App.Current.EventsBroker.Subscribe<KeyPressedEvent> (
				(e) => fallback = true
			);
			KeyContext context = new KeyContext ();
			context.AddAction (play);
			context.AddAction (forward);
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });

			App.Current.KeyContextManager.HandleKeyPressed (shiftRightHotkey);

			Assert.AreEqual (true, fallback);
		}

		[Test ()]
		public void TestHotkeyNotFallbacksCorrectly ()
		{
			bool fallback = false;
			VAS.App.Current.EventsBroker.Subscribe<KeyPressedEvent> (
				(e) => fallback = true
			);
			KeyContext context = new KeyContext ();
			context.AddAction (play);
			context.AddAction (forward);
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });

			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);

			Assert.AreEqual (false, fallback);
		}

		[Test ()]
		public void TestAddTemporalContext ()
		{
			KeyContext context = new KeyContext ();
			context.AddAction (forward);
			context.AddAction (play);
			KeyContext tempContext = new KeyContext ();
			tempContext.AddAction (fastForward);

			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });
			App.Current.KeyContextManager.AddContext (tempContext);

			Assert.AreEqual (tempContext,
				App.Current.KeyContextManager.CurrentKeyContexts [
					App.Current.KeyContextManager.CurrentKeyContexts.Count - 1]);
		}

		[Test ()]
		public void TestRemoveTemporalContext ()
		{
			KeyContext context = new KeyContext ();
			context.AddAction (forward);
			context.AddAction (play);
			KeyContext tempContext = new KeyContext ();
			tempContext.AddAction (fastForward);

			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });
			App.Current.KeyContextManager.AddContext (tempContext);
			App.Current.KeyContextManager.RemoveContext (tempContext);

			Assert.AreEqual (context,
				App.Current.KeyContextManager.CurrentKeyContexts [
					App.Current.KeyContextManager.CurrentKeyContexts.Count - 1]);
		}

		[Test ()]
		public void TestContextPriorities1 ()
		{
			KeyContext context = new KeyContext ();
			context.AddAction (forward);
			context.AddAction (play);
			KeyContext tempContext = new KeyContext ();
			tempContext.AddAction (fastForward);

			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });
			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (rightHotkey);
			App.Current.KeyContextManager.AddContext (tempContext);
			App.Current.KeyContextManager.HandleKeyPressed (shiftRightHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (rightHotkey);
			App.Current.KeyContextManager.RemoveContext (tempContext);
			App.Current.KeyContextManager.HandleKeyPressed (shiftRightHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (rightHotkey);

			Assert.AreEqual (3, countPlay);
			Assert.AreEqual (3, countForward);
			Assert.AreEqual (1, countFastForward);
		}

		[Test ()]
		public void TestContextPriorities2 ()
		{
			int countPlay2 = 0;
			KeyAction play2 = new KeyAction (new KeyConfig { Name = "PLAY_2", Key = spaceHotkey },
								  () => countPlay2++);

			KeyContext context = new KeyContext ();
			context.AddAction (play);
			context.AddAction (forward);
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });
			KeyContext context2 = new KeyContext ();
			context2.AddAction (play2);
			App.Current.KeyContextManager.AddContext (context2);

			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (rightHotkey);
			App.Current.KeyContextManager.RemoveContext (context2);
			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);

			Assert.AreEqual (1, countPlay2);
			Assert.AreEqual (1, countPlay);
			Assert.AreEqual (1, countForward);
		}

		[Test ()]
		public void TestGlobalContext ()
		{
			App.Current.KeyContextManager.GlobalKeyContext.AddAction (globalAction);

			App.Current.KeyContextManager.HandleKeyPressed (globalHotkey);

			Assert.AreEqual (1, countGlobal);
		}

		[Test ()]
		public void TestGlobalContextWithACurrentContextPriorities ()
		{
			App.Current.KeyContextManager.GlobalKeyContext.AddAction (globalAction);
			App.Current.KeyContextManager.GlobalKeyContext.AddAction (fastForward);
			KeyContext context = new KeyContext ();
			context.AddAction (play);
			context.AddAction (forward);
			context.AddAction (fastForward);
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });

			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (globalHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (rightHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (globalHotkey);
			App.Current.KeyContextManager.HandleKeyPressed (shiftRightHotkey);

			Assert.AreEqual (2, countPlay);
			Assert.AreEqual (1, countForward);
			Assert.AreEqual (1, countFastForward);
			Assert.AreEqual (2, countGlobal);
		}

		[Test ()]
		public void AddContext_TempContextAdded_RemovedAfterTimeExpires ()
		{
			// Arrange
			AutoResetEvent resetEvent = new AutoResetEvent (false);
			KeyTemporalContext context = new KeyTemporalContext ();
			context.AddAction (play);
			context.Duration = 100;
			context.ExpiredTimeAction = () => resetEvent.Set ();

			mockToolkit.Setup (x => x.Invoke (It.IsAny<EventHandler> ())).Callback (() => Task.Factory.StartNew (() => context.ExpiredTimeAction ()));

			// Act
			App.Current.KeyContextManager.AddContext (context);
			Assert.AreEqual (1, App.Current.KeyContextManager.CurrentKeyContexts.Count);

			// Assert
			resetEvent.WaitOne ();
			Assert.AreEqual (0, App.Current.KeyContextManager.CurrentKeyContexts.Count);

			resetEvent.Dispose ();
		}

		[Test ()]
		public void NewKeyContexts_TempContextAdded_RemoveAfterTimeExpires ()
		{
			// Arrange
			AutoResetEvent resetEvent = new AutoResetEvent (false);
			KeyTemporalContext tmpContext = new KeyTemporalContext ();
			tmpContext.AddAction (play);
			tmpContext.Duration = 100;
			tmpContext.ExpiredTimeAction = () => resetEvent.Set ();

			KeyContext context = new KeyContext ();
			context.AddAction (forward);

			mockToolkit.Setup (x => x.Invoke (It.IsAny<EventHandler> ())).Callback (() => Task.Factory.StartNew (() => tmpContext.ExpiredTimeAction ()));

			// Act
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { tmpContext, context });
			Assert.AreEqual (2, App.Current.KeyContextManager.CurrentKeyContexts.Count);

			// Assert
			resetEvent.WaitOne ();
			Assert.AreEqual (1, App.Current.KeyContextManager.CurrentKeyContexts.Count);

			resetEvent.Dispose ();
		}

		[Test ()]
		public void TestKeyActionPriority ()
		{
			int countPlay2 = 0;
			KeyAction play2 = new KeyAction (new KeyConfig { Name = "PLAY_2", Key = spaceHotkey },
								  () => countPlay2++, 1);

			KeyContext context = new KeyContext ();
			context.AddAction (play);
			context.AddAction (play2);
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });
			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);

			context = new KeyContext ();
			context.AddAction (play2);
			context.AddAction (play);
			App.Current.KeyContextManager.NewKeyContexts (new List<KeyContext> { context });
			App.Current.KeyContextManager.HandleKeyPressed (spaceHotkey);

			Assert.AreEqual (2, countPlay2);
			Assert.AreEqual (0, countPlay);
		}
	}
}

