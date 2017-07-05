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
using NUnit.Framework;
using VAS.Core.Common;
using System.Linq;

namespace VAS.Tests.Core.Common
{
	public interface IDummyObjectInterface
	{
	}

	public class DummyObjectLow : IDummyObjectInterface
	{
	}

	public class DummyObjectHigh : IDummyObjectInterface
	{
	}

	[TestFixture]
	public class TestRegistry
	{
		[Test]
		public void TestRetrieveNewInstance ()
		{
			Registry reg = new Registry ("test");
			reg.Register<IDummyObjectInterface, DummyObjectLow> (0);
			reg.Register<IDummyObjectInterface, DummyObjectHigh> (1);

			IDummyObjectInterface o1 = reg.Retrieve<IDummyObjectInterface> (InstanceType.New);
			IDummyObjectInterface o2 = reg.Retrieve<IDummyObjectInterface> (InstanceType.New);

			Assert.IsNotNull (o1);
			Assert.IsNotNull (o2);
			Assert.IsInstanceOf (typeof (DummyObjectHigh), o1);
			Assert.IsInstanceOf (typeof (DummyObjectHigh), o2);
			Assert.AreNotSame (o1, o2);
		}

		[Test]
		public void TestRetrieveSameInstance ()
		{
			Registry reg = new Registry ("test");
			reg.Register<IDummyObjectInterface, DummyObjectLow> (0);

			IDummyObjectInterface o1 = reg.Retrieve<IDummyObjectInterface> (InstanceType.Default);
			IDummyObjectInterface o2 = reg.Retrieve<IDummyObjectInterface> (InstanceType.Default);
			IDummyObjectInterface o3 = reg.Retrieve<IDummyObjectInterface> (InstanceType.New);

			Assert.IsNotNull (o1);
			Assert.IsNotNull (o2);
			Assert.AreSame (o1, o2);
			Assert.AreNotSame (o1, o3);
		}

		[Test]
		public void TestRetrieveAllNewInstance ()
		{
			Registry reg = new Registry ("test");
			reg.Register<IDummyObjectInterface, DummyObjectLow> (0);
			reg.Register<IDummyObjectInterface, DummyObjectHigh> (1);

			List<IDummyObjectInterface> elements1 = reg.RetrieveAll<IDummyObjectInterface> (InstanceType.New);
			List<IDummyObjectInterface> elements2 = reg.RetrieveAll<IDummyObjectInterface> (InstanceType.New);

			Assert.AreEqual (2, elements1.Count);
			Assert.AreEqual (2, elements2.Count);
			Assert.AreEqual (1, elements1.OfType<DummyObjectLow> ().Count ());
			Assert.AreEqual (1, elements1.OfType<DummyObjectHigh> ().Count ());
			CollectionAssert.AreNotEqual (elements1.OfType<DummyObjectLow> (), elements2.OfType<DummyObjectLow> ());
			CollectionAssert.AreNotEqual (elements1.OfType<DummyObjectHigh> (), elements2.OfType<DummyObjectHigh> ());
		}

		[Test]
		public void TestRetrieveAllSameInstance ()
		{
			Registry reg = new Registry ("test");
			reg.Register<IDummyObjectInterface, DummyObjectLow> (0);
			reg.Register<IDummyObjectInterface, DummyObjectHigh> (1);

			List<IDummyObjectInterface> elements1 = reg.RetrieveAll<IDummyObjectInterface> (InstanceType.Default);
			List<IDummyObjectInterface> elements2 = reg.RetrieveAll<IDummyObjectInterface> (InstanceType.Default);

			Assert.AreEqual (2, elements1.Count);
			Assert.AreEqual (2, elements2.Count);
			Assert.AreEqual (1, elements1.OfType<DummyObjectLow> ().Count ());
			Assert.AreEqual (1, elements1.OfType<DummyObjectHigh> ().Count ());
			CollectionAssert.AreEqual (elements1.OfType<DummyObjectLow> (), elements2.OfType<DummyObjectLow> ());
			CollectionAssert.AreEqual (elements1.OfType<DummyObjectHigh> (), elements2.OfType<DummyObjectHigh> ());
		}

		[Test]
		public void Register_TwiceWithSamePriority_OnlyLastRegistered ()
		{
			Registry reg = new Registry ("test");
			reg.Register<IDummyObjectInterface, DummyObjectLow> (0);
			reg.Register<IDummyObjectInterface, DummyObjectHigh> (0);

			List<IDummyObjectInterface> elements1 = reg.RetrieveAll<IDummyObjectInterface> (InstanceType.New);

			Assert.AreEqual (1, elements1.Count);
			Assert.AreEqual (0, elements1.OfType<DummyObjectLow> ().Count ());
			Assert.AreEqual (1, elements1.OfType<DummyObjectHigh> ().Count ());
		}

		[Test]
		public void Register_ExternalInstance_RetrieveSame ()
		{
			Registry reg = new Registry ("test");
			DummyObjectLow instance = new DummyObjectLow ();
			reg.Register<IDummyObjectInterface> (instance, 0);

			IDummyObjectInterface o1 = reg.Retrieve<IDummyObjectInterface> (InstanceType.Default);
			IDummyObjectInterface o2 = reg.Retrieve<IDummyObjectInterface> (InstanceType.Default);

			Assert.IsNotNull (o1);
			Assert.IsNotNull (o2);
			Assert.AreSame (o1, o2);
		}
	}
}

