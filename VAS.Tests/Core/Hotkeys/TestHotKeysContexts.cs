using System;
using System.Collections.Generic;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Store;

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
		}

		[TearDown ()]
		public void TearDown ()
		{
			App.Current.KeyContextManager.GlobalKeyContext.KeyActions.Clear ();
		}

		void InitKeyActions ()
		{
			play = new KeyAction (PLAY_ACTION, spaceHotkey,
				() => countPlay++);
			forward = new KeyAction (FORWARD_ACTION, rightHotkey,
				() => countForward++);
			fastForward = new KeyAction (FAST_FORWARD_ACTION, shiftRightHotkey,
				() => countFastForward++);
			globalAction = new KeyAction (GLOBAL_ACTION, globalHotkey,
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
			KeyAction play2 = new KeyAction ("PLAY_2", spaceHotkey,
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
	}
}

