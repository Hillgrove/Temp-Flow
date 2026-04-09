using DataAccess.DTOs;
using DataAccess.Models;

namespace DataAccess.Interfaces
{
    public interface ISensorDataRepository
    {
        SensorData Add(SensorData sensorData);
        int DeleteOlderThan(DateTime cutoffDate);
        SensorData? Get(int id);
        IEnumerable<SensorDataDto> GetAll();
        IEnumerable<SensorData> GetRecentSensorData(int days);
    }
}