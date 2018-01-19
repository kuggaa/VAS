using System;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Store;

namespace VAS.Core.Interfaces.Multimedia
{
	/// <summary>
	/// Seeker interface used to throttle seeks.
	/// </summary>
	public interface ISeeker : IDisposable
	{
		/// <summary>
		/// Event called when the seeker decides it's time to seek
		/// </summary>
		event SeekHandler SeekEvent;

		/// <summary>
		/// Add the the specified Seek to the seeker.
		/// </summary>
		/// <param name="seekType">Seek type. SeekType.None means no seek.</param>
		/// <param name="start">Time to seek.</param>
		/// <param name="rate">Rate.</param>
		void Seek (SeekType seekType, Time start = null, float rate = 1);
	}
}