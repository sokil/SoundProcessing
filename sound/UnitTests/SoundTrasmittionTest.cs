using Sound.Model.Soundprocessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UnitTests
{
    
    
    /// <summary>
    ///Это класс теста для SoundTrasmittionTest, в котором должны
    ///находиться все модульные тесты SoundTrasmittionTest
    ///</summary>
    [TestClass()]
    public class SoundTrasmittionTest
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
        ///Тест для _getFreqRelatedRoom1PinkSignalLevels
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Sound.exe")]
        public void _getFreqRelatedRoom1PinkSignalLevelsTest()
        {
            SoundTrasmittion_Accessor target = new SoundTrasmittion_Accessor();

            // 6
            target.setElementsInFilterChain(6);
            SortedList<double, double> actual = target._getFreqRelatedRoom1PinkSignalLevels(70);
            Assert.AreEqual(52.05, Math.Round(actual[4000], 2));
        }

        /// <summary>
        ///Тест для _getFreqRelatedRoom1BrownSignalLevels
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Sound.exe")]
        public void _getFreqRelatedRoom1BrownSignalLevelsTest()
        {
            SoundTrasmittion_Accessor target = new SoundTrasmittion_Accessor();

            // 6
            target.setElementsInFilterChain(6);
            SortedList<double, double> actual = target._getFreqRelatedRoom1BrownSignalLevels(70);
            Assert.AreEqual(38.74, Math.Round(actual[4000], 2));
        }
    }
}
