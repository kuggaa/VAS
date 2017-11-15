
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;

namespace VAS.UI.Forms
{
	public class FormsUIToolkit : IGUIToolkit
	{
		public float DeviceScaleFactor => 1;

		public IMainController MainController { get; set; }

		public bool FullScreen { set => throw new NotImplementedException(); }

		public IVideoPlayerView GetPlayerView()
		{
			throw new NotImplementedException();
		}

		public void Invoke(EventHandler handler)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() => handler(this, null));
		}

		public Task<T> Invoke<T>(Func<Task<T>> handler)
		{
			throw new NotImplementedException();
		}

		public bool LoadPanel(IPanel panel)
		{
			throw new NotImplementedException();
		}

		public Task<bool> Quit()
		{
			throw new NotImplementedException();
		}

		public void Register<I, C>(int priority)
		{
			throw new NotImplementedException();
		}

		public string RemuxFile(string filePath, string outputFile, VideoMuxerType muxer)
		{
			throw new NotImplementedException();
		}

		public void RunLoop(Func<bool> condition)
		{
			throw new NotImplementedException();
		}

		public HotKey SelectHotkey(HotKey hotkey, object parent = null)
		{
			throw new NotImplementedException();
		}

		public bool SelectMediaFiles(MediaFileSet fileSet)
		{
			throw new NotImplementedException();
		}

		public void ShowProjectStats(Project project)
		{
			throw new NotImplementedException();
		}

		public Project ChooseProject(List<Project> projects)
		{
			throw new NotImplementedException();
		}

		public List<EditionJob> ConfigureRenderingJob(Playlist playlist)
		{
			throw new NotImplementedException();
		}

		public Task<bool> CreateNewTemplate<T>(IList<T> availableTemplates, string defaultName, string countText, string emptyText, CreateEvent<T> evt) where T : ITemplate
		{
			throw new NotImplementedException();
		}

		public EndCaptureResponse EndCapture(bool isCapturing)
		{
			throw new NotImplementedException();
		}

		public void ExportFrameSeries(TimelineEvent play, string snapshotDir)
		{
			throw new NotImplementedException();
		}

	}
}
