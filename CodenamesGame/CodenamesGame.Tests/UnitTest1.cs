using NUnit.Framework;
using System;

namespace CodenamesGame.Tests
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void TestMethod1()
        {
            int a = 1;
            int b = 2;
            int sum = a + b;
            Assert.That(sum.Equals(3));
        }
    }
}
