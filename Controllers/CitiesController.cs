using System;
using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        //Injecting the Repository through Constructor Injection
        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); ;
        }

        [HttpGet] 
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterest>>> GetCities()
        {
            //calling the method from the Repository
            //The method will return an object of the type Entity (in this case a List of Entity Objects is returned)
            var cityEntities = await _cityInfoRepository.GetCitiesAsync();

            return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterest>>(cityEntities));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CityDto>> GetCity(int id, bool includePointsOfInterest = false)
        {
            var cityEntity = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);
            if (cityEntity is null)
            {
                return BadRequest($"There's no city for the Id {id}");
            }

            if (includePointsOfInterest)
            {
                return Ok(_mapper.Map<CityDto>(cityEntity));
            }

            return Ok(_mapper.Map<CityWithoutPointsOfInterest>(cityEntity));
        }
    }
}

