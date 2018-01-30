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
using System;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.License;
using VAS.Core.ViewModel;
using VAS.Core.ViewModel.Statistics;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestCountLimitationBarChartVM
	{
		CountLimitationBarChartVM viewModel;
		TwoBarChartVM twoBarChartVM;
		CountLimitationVM countLimitationVM;

		void CreateViewModels (int max, int count, bool enabled, int showOnRemaining = -1)
		{
			countLimitationVM = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					Maximum = max,
					Count = count,
					Enabled = enabled,
					DisplayName = "testLimitation",
					RegisterName = "test"
				}
			};

			twoBarChartVM = new TwoBarChartVM (new SeriesVM {
												  Title = "Remaining", Elements = countLimitationVM.Remaining,
												  Color = Color.Green1
											  },
											  new SeriesVM {
												  Title = "Current", Elements = countLimitationVM.Count,
												  Color = Color.Transparent
											  }, countLimitationVM.Maximum);

			viewModel = new CountLimitationBarChartVM (showOnRemaining) {
				Limitation = countLimitationVM,
				BarChart = twoBarChartVM
			};
		}

		[Test]
		public void Initialize_LimitationNotEnabled_AllValuesAsExpected ()
		{
			CreateViewModels (4, 1, false);

			Assert.IsFalse (viewModel.Visible);
			Assert.IsFalse (viewModel.Limitation.Enabled);
			Assert.AreEqual (int.MaxValue, viewModel.Limitation.Maximum);
			Assert.AreEqual (1, viewModel.Limitation.Count);
			Assert.AreEqual (int.MaxValue - 1, viewModel.Limitation.Remaining);
		}

		[Test]
		public void Initialize_LimitationEnabled_AllValuesAsExpected ()
		{
			CreateViewModels (4, 1, true);

			Assert.IsTrue (viewModel.Visible);
			Assert.IsTrue (viewModel.Limitation.Enabled);
			Assert.AreEqual (4, viewModel.Limitation.Maximum);
			Assert.AreEqual (1, viewModel.Limitation.Count);
			Assert.AreEqual (3, viewModel.Limitation.Remaining);
		}

		[Test]
		public void Initialize_ShowOnRemainingMoreThanCount_AllValuesAsExpected ()
		{
			CreateViewModels (4, 1, true, 2);

			Assert.IsFalse (viewModel.Visible);
			Assert.IsTrue (viewModel.Limitation.Enabled);
			Assert.AreEqual (4, viewModel.Limitation.Maximum);
			Assert.AreEqual (1, viewModel.Limitation.Count);
			Assert.AreEqual (3, viewModel.Limitation.Remaining);
		}

		[Test]
		public void Initialize_ShowOnRemainingLessThanCount_AllValuesAsExpected ()
		{
			CreateViewModels (4, 3, true, 1);

			Assert.IsTrue (viewModel.Visible);
			Assert.IsTrue (viewModel.Limitation.Enabled);
			Assert.AreEqual (4, viewModel.Limitation.Maximum);
			Assert.AreEqual (3, viewModel.Limitation.Count);
			Assert.AreEqual (1, viewModel.Limitation.Remaining);
		}

		[Test]
		public void ChangeToNotEnabled_UpdatesVisibility ()
		{
			CreateViewModels (4, 1, true);

			countLimitationVM.Model.Enabled = false;

			Assert.IsFalse (viewModel.Visible);
			Assert.IsFalse (viewModel.Limitation.Enabled);
			Assert.AreEqual (int.MaxValue, viewModel.Limitation.Maximum);
			Assert.AreEqual (1, viewModel.Limitation.Count);
			Assert.AreEqual (int.MaxValue - 1, viewModel.Limitation.Remaining);
		}

		[Test]
		public void ChangeToEnabled_UpdatesVisibility ()
		{
			CreateViewModels (4, 1, false);

			countLimitationVM.Model.Enabled = true;

			Assert.IsTrue (viewModel.Visible);
			Assert.IsTrue (viewModel.Limitation.Enabled);
			Assert.AreEqual (4, viewModel.Limitation.Maximum);
			Assert.AreEqual (1, viewModel.Limitation.Count);
			Assert.AreEqual (3, viewModel.Limitation.Remaining);
		}

		[Test]
		public void ChangeInCount_ValuesChanges ()
		{
			CreateViewModels (4, 1, true);

			countLimitationVM.Model.Count = 2;

			Assert.IsTrue (viewModel.Visible);
			Assert.IsTrue (viewModel.Limitation.Enabled);
			Assert.AreEqual (4, viewModel.Limitation.Maximum);
			Assert.AreEqual (2, viewModel.Limitation.Count);
			Assert.AreEqual (2, viewModel.Limitation.Remaining);
		}

		[Test]
		public void LimitationChange_CountGreaterShowOnRemaining_Visible ()
		{
			CreateViewModels (4, 1, true, 1);

			Assert.IsFalse (viewModel.Visible);

			countLimitationVM.Model.Count = 3;

			Assert.IsTrue (viewModel.Visible);
			Assert.IsTrue (viewModel.Limitation.Enabled);
			Assert.AreEqual (4, viewModel.Limitation.Maximum);
			Assert.AreEqual (3, viewModel.Limitation.Count);
			Assert.AreEqual (1, viewModel.Limitation.Remaining);
		}

		[Test]
		public void LimitationChange_CountLowerShowOnRemaining_NotVisible ()
		{
			CreateViewModels (4, 3, true, 1);

			Assert.IsTrue (viewModel.Visible);

			countLimitationVM.Model.Count = 2;

			Assert.IsFalse (viewModel.Visible);
			Assert.IsTrue (viewModel.Limitation.Enabled);
			Assert.AreEqual (4, viewModel.Limitation.Maximum);
			Assert.AreEqual (2, viewModel.Limitation.Count);
			Assert.AreEqual (2, viewModel.Limitation.Remaining);
		}
	}
}
