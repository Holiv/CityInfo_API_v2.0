using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CityInfo.API.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using CityInfo.API.Models.Services;
using AutoMapper;
using CityInfo.API.DbContexts;
using CityInfo.API.Services;
using CityInfo.API.Entities;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : Controller
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _localMailService;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;
        private readonly CitiesDataStore _citiesDataStore;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService localMailService, ICityInfoRepository cityInfoRepository, IMapper mapper, CitiesDataStore citiesDataStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _localMailService = localMailService ?? throw new ArgumentNullException(nameof(localMailService));
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentException(nameof(mapper));
            _citiesDataStore = citiesDataStore;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointsOfInterestDto>>> GetPointsOfInterest(int cityId)
        {
            try
            {
                var cityEntity = await _cityInfoRepository.CityExists(cityId);
                if (cityEntity)
                {
                    var pointsOfInterest = await _cityInfoRepository.GetPointsOfInterestsAsync(cityId);
                    return Ok(_mapper.Map<IEnumerable<PointsOfInterestDto>>(pointsOfInterest));
                }
                throw new ArgumentException();
            }
            catch
            {
                _logger.LogCritical($"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound($"City with id {cityId} wasn't found when accessing points of interest.");
            }            
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointsOfInterestDto>> GetPointOfInterest(
            int cityId, int pointOfInterestId)
        {
            try
            {
                var cityExists = await _cityInfoRepository.CityExists(cityId);
                if (cityExists)
                {
                    var pointOfInterestExists = await _cityInfoRepository.PointOfInterestExist(cityId, pointOfInterestId);
                    if (pointOfInterestExists)
                    {
                        var pointOfInterest = await _cityInfoRepository.GetPointOfInterestsAsync(cityId, pointOfInterestId);
                        return Ok(_mapper.Map<PointsOfInterestDto>(pointOfInterest));
                    }
                    throw new ArgumentException(nameof(pointOfInterestId));
                }
                throw new ArgumentException(nameof(cityId));
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"The value of {ex.Message} is not valid");
                return NotFound($"The value of {ex.Message} is not valid");
            }
        }

        [HttpPost] 
        public async Task<ActionResult<PointsOfInterestDto>> CreatePointOfInterest(int cityId, PointsOfInterestForCreationDTO pointsOfInterest)
        {
            //check if the city exists
            var cityExists = await _cityInfoRepository.CityExists(cityId);
            if (!cityExists)
            {
                return NotFound();
            }

            //mapping the DTO Class received from the request into the respective Entity Class
            //Saving the new Entity in a variable 
            var finalPointOfInterest = _mapper.Map<PointOfInterest>(pointsOfInterest);
            
            await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);
            await _cityInfoRepository.SaveChangesAsync();

            var createdPointOfInterestToReturn = _mapper.Map<PointsOfInterestDto>(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId = cityId,
                    pointOfInterestId = createdPointOfInterestToReturn.Id
                },
                createdPointOfInterestToReturn);
            #region 
            //    if (ModelState.IsValid) { };
            //   //getting the list of cities in a variable to avoid repetition
            //    var cities = _citiesDataStore.Cities;

            //    //checks if city id sent in the URI(cityId) exists
            //    //if exists, get it into the variable
            //    var city = cities.FirstOrDefault(city => city.Id == cityId);
            //    if (city == null)
            //    {
            //        return NotFound();
            //    }

            //    //getting the max Id value from the existing cities points of interests
            //    var maxPointOfInterest = cities
            //                            .SelectMany(city => city.PointsOfInterest)
            //                            .Max(maxId => maxId.Id);

            //    //creating a new instance of the PointsOfInterestDTO class
            //    //feed the new instance with the data passed in the API request Body
            //    PointsOfInterestDto finalPointOfInteres = new PointsOfInterestDto()
            //    {
            //        Id = ++maxPointOfInterest,
            //        Name = pointsOfInterest.Name,
            //        Description = pointsOfInterest.Description
            //    };

            //    //Adding the new instance of the PointsOfInterest to the respective City
            //    city.PointsOfInterest.Add(finalPointOfInteres);

            //    //returning the status code to the user
            //    //CreatedAtRout returns the 204 Created status code and accepts three parameters
            //    //1 - Action identifier where the new added data can be found and requested
            //    //2 - Object data sent by the user can see exactly what was created
            //    //3 - The new created object 
            //    return CreatedAtRoute("GetPointOfInterest",
            //        new
            //        {
            //            cityId = cityId,
            //            pointOfInterestId = finalPointOfInteres.Id
            //        },
            //        finalPointOfInteres);

            #endregion
        }
        // 3 Parameters in the Put Action to update nested data
        // - cityId to get the correct city where the nested data lives
        // - pointOfInterestId to identify the data to be updated
        // - New instance of the object to be updated that is injected and receives the data from the request body

        [HttpPut("{pointofinterestid}")]
        public async Task<ActionResult> UpdatePointsOfInterest(int cityId, int pointOfInterestId, PointsOfInterestForUpdatingDto pointsOfInterestForUpdatingDto)
        {
            var cityExists = await _cityInfoRepository.CityExists(cityId);
            if (!cityExists)
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestsAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity is null)
            {
                return NotFound();
            }

            _mapper.Map(pointsOfInterestForUpdatingDto, pointOfInterestEntity);

            await _cityInfoRepository.SaveChangesAsync();

            return NoContent();

        }

        // Third parameter is JsonPatchDocument<T> type that will allows to work with the Json Path Document format sent in the request body.
        // The data sent is used to feed a new instance of the Dto object then the JsonPatchDocument instance is applied to it. We can also checks the ModelState in case of a user mistake in the request body
        [HttpPatch("{pointofinterestid}")]
        public async Task<ActionResult> PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId, JsonPatchDocument<PointsOfInterestForUpdatingDto> patchDocument)
        {
            var cityExists = await _cityInfoRepository.CityExists(cityId);
            if (!cityExists)
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestsAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity is null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = _mapper.Map<PointsOfInterestForUpdatingDto>(pointOfInterestEntity);
            
            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            // ModelState will check if the Model Input is valid
            // The user could be passed a property that does not exist
            // The Model input in this case is the patchDocument (JsonPatchDocument)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            };

            // Validate the Model for Validation Annotation on the Dto
            // The last validation does not checks for empty fields, only if the structure of the document to patch matches the structure of the Dto object
            // To check for validation annotation the TryValidateModel is used on the object created that is going to be used in the patch process
            if (!TryValidateModel(pointOfInterestToPatch))
                return BadRequest(ModelState);

            _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);
            await _cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{pointofinterestid}")]
        public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            var cityExists = await _cityInfoRepository.CityExists(cityId);
            if (!cityExists)
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestsAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity is null)
            {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
            await _cityInfoRepository.SaveChangesAsync();

            _localMailService.Send($"Point of interest deleted.", $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted");
            return NoContent();
        }
    }
}

