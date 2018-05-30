using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.IO;

namespace MediaWatch.Tests
{
    [TestClass]
    public class MediaWatchTests
    {
        public MediaWatcher mw { get; set; }

        [TestInitialize()]
        public void MediaWatchInitialise()
        {
            mw = new MediaWatcher();
            mw.Configure(ConfigurationManager.AppSettings);
        }

        [TestCleanup()]
        public void MediaWatchCleanup()
        {
            mw.DumpXml();
        }

        [TestMethod]
        public void MediaMasterShouldNotBeNull()
        {
            var sut = new MediaMaster(
                "Media", 
                mw.XmlDirectory, 
                "media.xml", 
                mw.Logger);
            Assert.IsNotNull(sut);
        }

        [TestMethod]
        public void ShouldProduceTheMovieReport()
        {
            mw.DoLatest = false;
            mw.DoTv = false;
            mw.DoMovies = true;
            mw.Execute();
            Assert.IsTrue(File.Exists($"{mw.OutputDirectory}{"Movies-TEST.htm"}"));
        }

        [TestMethod]
        public void ShouldProduceTheTvReport()
        {
            mw.DoLatest = false;
            mw.DoTv = true;
            mw.DoMovies = false;
            mw.Execute();
            Assert.IsTrue(File.Exists($"{mw.OutputDirectory}{"TV-TEST.htm"}"));
        }

        [TestMethod]
        public void ShouldProduceTheLatestReport()
        {
            mw.DoLatest = true;
            mw.DoTv = false;
            mw.DoMovies = false;
            mw.Execute();
            Assert.IsTrue(File.Exists($"{mw.OutputDirectory}{"Latest-TEST.htm"}"));
        }

        [TestMethod]
        public void ShouldProduceTheBookReports()
        {
            mw.DoLatest = false;
            mw.DoTv = false;
            mw.DoMovies = false;
            mw.DoBooks = true;
            mw.Execute();
            Assert.IsTrue(File.Exists($"{mw.OutputDirectory}{"IT-Books-TEST.htm"}"));
            Assert.IsTrue(File.Exists($"{mw.OutputDirectory}{"Inspire-Books-TEST.htm"}"));
            Assert.IsTrue(File.Exists($"{mw.OutputDirectory}{"Entrepreneurial-Books-TEST.htm"}"));
        }
    }
}