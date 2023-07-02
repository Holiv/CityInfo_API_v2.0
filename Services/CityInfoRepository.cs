using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services
{
    //Here is the place where we implements persistency logic
    //For each method we can use different technologies, EntityFramework Core, Ado.Net
    //For the consumer of the API the implementations does not matter at all
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext _cityInfoContext;

        public CityInfoRepository(CityInfoContext cityInfoContext)
        {
            _cityInfoContext = cityInfoContext ?? throw new ArgumentNullException(nameof(cityInfoContext));
        }

        public async Task<IEnumerable<City>> GetCitiesAsync() 
        {
            return await _cityInfoContext.Cities.OrderBy(city => city.Name).ToListAsync();
        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
        {
            if (includePointsOfInterest)
            {
                return await _cityInfoContext.Cities
                    .Include(city => city.PointsOfInterest)
                    .Where(city => city.Id == cityId).FirstOrDefaultAsync();
            }
            return await _cityInfoContext.Cities.Where(city => city.Id == cityId).FirstOrDefaultAsync();
        }

        public async Task<PointOfInterest?> GetPointOfInterestsAsync(int cityId, int pointOfInterestId)
        {
            return await _cityInfoContext.PointOfInterests
                .Where(pInt => pInt.CityId == cityId && pInt.Id == pointOfInterestId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestsAsync(int cityId)
        {
            return await _cityInfoContext.PointOfInterests
                .Where(pInt => pInt.CityId == cityId).ToListAsync();
        }

        public async Task<bool> CityExists(int cityId)
        {
            return await _cityInfoContext.Cities.AnyAsync(city => city.Id == cityId);
        }

        public async Task<bool> PointOfInterestExist(int cityId, int pointOfInterest)
        {
            return await _cityInfoContext.PointOfInterests.AnyAsync(pInt => pInt.Id == pointOfInterest && pInt.CityId == cityId);
        }

        public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false);
            if (city is not null)
            {
                city.PointsOfInterest.Add(pointOfInterest);
            }
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        { 
            _cityInfoContext.PointOfInterests.Remove(pointOfInterest);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _cityInfoContext.SaveChangesAsync() >= 0);
        }
    }
}
