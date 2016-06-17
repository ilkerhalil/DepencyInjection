using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace IocContainer.Tests
{
    [TestClass()]
    public class ContainerTests
    {
        private  readonly IContainer _container;

        public ContainerTests()
        {
            _container = new Container();
            _container.Register<IBird, Bird>();
        }

        [TestMethod()]
        public void RegisterTest()
        {

            var bird = _container.Resolve<IBird>();
            Assert.IsInstanceOfType(bird,typeof(IBird));
        }

        [TestMethod()]
        public void HandedTest()
        {
            var bird = new Bird();
            Assert.IsInstanceOfType(bird, typeof(IBird));
        }
    }

    public interface IBird
    {
        void Fly();
    }

    public class Bird : IBird
    {
        public void Fly()
        {
            Debug.WriteLine("Uç uç");
        }
    }
}