using SignalProcessing.Filter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using System.Linq;

using SignalProcessing.Signal;

namespace TestSignalProcessing
{
    
    
    /// <summary>
    ///Это класс теста для KaiserBandpassChainTest, в котором должны
    ///находиться все модульные тесты KaiserBandpassChainTest
    ///</summary>
    [TestClass()]
    public class KaiserBandpassChainTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Дополнительные атрибуты теста
        // 
        //При написании тестов можно использовать следующие дополнительные атрибуты:
        //
        //ClassInitialize используется для выполнения кода до запуска первого теста в классе
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //TestInitialize используется для выполнения кода перед запуском каждого теста
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //TestCleanup используется для выполнения кода после завершения каждого теста
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        /// check median and variance formula
        /// </summary>
        [TestMethod()]
        public void medianTest()
        {
            double[] vector = new double[] { 1, 2, 3, 4, 5 };
            double expectedMean = 3;
            double expectedVariance = 2.5;

            double median, variance;

            // get median manually
            median = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                median += vector[i];
            }

            median /= vector.Length;

            // get variance manually
            variance = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                variance += Math.Pow(vector[i] - median, 2);
            }

            variance /= vector.Length - 1;

            Assert.AreEqual(expectedMean, median);
            Assert.AreEqual(expectedVariance, variance);
        }

        /// <summary>
        ///Тест для getFilteredSignal
        ///</summary>
        [TestMethod()]
        public void getFilteredSignalTest()
        {
            // get signal
            Wav wavSignal = (new Wav())
                .setSampleFrequency(1250)
                .setSignal(Enumerable.Repeat<double>(1, 5000).ToArray());

            // check median
            Assert.AreEqual(1, Math.Round(wavSignal.getMedian(), 3));

            // check variance
            Assert.AreEqual(0, Math.Round(1000000 * wavSignal.getVariance(), 3));

            // init filter chain
            KaiserBandpassChain filterChain = new KaiserBandpassChain();
            filterChain
                .setSampleFrequency(1250)
                .setChainFrequencies(new double[] {100, 125, 250, 500})
                .setFilterOrder(1000);

            Wav filteredSignal = filterChain.filter<Wav>(wavSignal).getFilteredSignal();

            // check median
            Assert.AreEqual(0.177, Math.Round(filteredSignal.getMedian(), 3));

            // check variance
            Assert.AreEqual(38.237, Math.Round(1000000*filteredSignal.getVariance(), 3));
        }
    }
}
