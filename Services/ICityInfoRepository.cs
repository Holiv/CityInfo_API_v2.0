using CityInfo.API.Entities;
using System.ComponentModel;

namespace CityInfo.API.Services
{
    public interface ICityInfoRepository
    {
        Task<IEnumerable<City>> GetCitiesAsync();
        Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);
        Task<IEnumerable<PointOfInterest>> GetPointsOfInterestsAsync(int cityId);
        Task<PointOfInterest?> GetPointOfInterestsAsync(int cityId, int pointOfInterestId);
        Task<bool> CityExists(int cityId);
        Task<bool> PointOfInterestExist(int cityId, int pointOfInterestId);
        Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest);
        void DeletePointOfInterest(PointOfInterest pointOfInterest);
        Task<bool> SaveChangesAsync();
    }
}
