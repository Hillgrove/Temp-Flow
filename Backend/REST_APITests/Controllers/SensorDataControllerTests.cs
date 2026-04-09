using DataAccess.DTOs;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;


namespace REST_API.Controllers.Tests
{
    [TestClass()]
    public class SensorDataControllerTests
    {
        private Mock<ISensorDataRepository> _mockRepository;
        private SensorDataController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<ISensorDataRepository>();
            _controller = new SensorDataController(_mockRepository.Object);
        }

        [TestMethod()]
        public void Get_ReturnsAllSensorData()
        {
            // Arrange
            var sensorDataList = new List<SensorDataDto>
            {
                new SensorDataDto { Id = 1, SensorId = 1, Temperature = 25.0, Humidity = 50.0, Pressure = 1000.0, Timestamp = DateTime.UtcNow },
                new SensorDataDto { Id = 2, SensorId = 2, Temperature = 26.0, Humidity = 55.0, Pressure = 1005.0, Timestamp = DateTime.UtcNow }
            };
            _mockRepository.Setup(repo => repo.GetAll()).Returns(sensorDataList);

            // Act
            var result = _controller.Get();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod()]
        public void Get_SensorDataNotFound_ReturnsNotFound()
        {
            // Arrange
            int sensorDataId = 1;
            _mockRepository.Setup(repo => repo.Get(sensorDataId)).Returns((SensorData)null);

            // Act
            var result = _controller.Get(sensorDataId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod()]
        public void Get_ValidSensorData_ReturnsOk()
        {
            // Arrange
            int sensorDataId = 1;
            var sensorData = new SensorData { Id = sensorDataId, SensorId = 1, Temperature = 25.0, Humidity = 50.0, Pressure = 1000.0, Timestamp = DateTime.UtcNow };
            _mockRepository.Setup(repo => repo.Get(sensorDataId)).Returns(sensorData);

            // Act
            var result = _controller.Get(sensorDataId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod()]
        public void GetRecentSensorData_ValidDays_ReturnsOk()
        {
            // Arrange
            int days = 5;
            var sensorDataList = new List<SensorData>
            {
                new SensorData { Id = 1, SensorId = 1, Temperature = 25.0, Humidity = 50.0, Pressure = 1000.0, Timestamp = DateTime.UtcNow },
                new SensorData { Id = 2, SensorId = 1, Temperature = 26.0, Humidity = 55.0, Pressure = 1005.0, Timestamp = DateTime.UtcNow }
            };
            _mockRepository.Setup(repo => repo.GetRecentSensorData(days)).Returns(sensorDataList);

            // Act
            var result = _controller.GetRecentSensorData(days);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod()]
        public void GetRecentSensorData_NoDataFound_ReturnsNotFound()
        {
            // Arrange
            int days = 5;
            _mockRepository.Setup(repo => repo.GetRecentSensorData(days)).Returns((IEnumerable<SensorData>)null);

            // Act
            var result = _controller.GetRecentSensorData(days);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod()]
        public void DeleteOldSensorData_ValidDays_ReturnsOk()
        {
            // Arrange
            int days = 5;
            _mockRepository.Setup(repo => repo.DeleteOlderThan(It.IsAny<DateTime>())).Returns(10);

            // Act
            var result = _controller.DeleteOldSensorData(days);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod()]
        public void DeleteOldSensorData_NoDataDeleted_ReturnsNoContent()
        {
            // Arrange
            int days = 5;
            _mockRepository.Setup(repo => repo.DeleteOlderThan(It.IsAny<DateTime>())).Returns(0);

            // Act
            var result = _controller.DeleteOldSensorData(days);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod()]
        public void DeleteOldSensorData_NegativeDays_ReturnsBadRequest()
        {
            // Arrange
            int days = -1;

            // Act
            var result = _controller.DeleteOldSensorData(days);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }
    }
}