using Microsoft.ACR.DISQO.Service.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace Microsoft.ACR.DISQO.Test
{
    [TestClass]
    public class UtilTests
    {
        [TestMethod]
        public void RESTResponse_CorrectResponse_Error()
        {
            //Arrange
            var exObject = new System.Exception("This is just a test exception");

            //Act
            var response = Util.RESTResponse(exObject);

            //Assert
            Assert.IsTrue(HttpStatusCode.InternalServerError == response.StatusCode);

        }

        [TestMethod]
        public void RESTResponse_CorrectResponse_BadRequest()
        {
            //Arrange
            object badRequestObject = null;

            //Act
            var response = Util.RESTResponse(badRequestObject);

            //Assert
            Assert.IsTrue(HttpStatusCode.OK == response.StatusCode);

        }

        [TestMethod]
        public void RESTResponse_CorrectResponse_GoodRequest()
        {
            //Arrange
            var goodRequestObject = new { data = "This is a working object" };

            //Act
            var response = Util.RESTResponse(goodRequestObject);

            //Assert
            Assert.IsTrue(HttpStatusCode.OK == response.StatusCode);

        }
    }
}
