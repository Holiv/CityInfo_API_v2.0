﻿using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;

namespace CityInfo.API.Profiles
{
    public class PointOfInterestProfile : Profile
    {
        public PointOfInterestProfile()
        {
            CreateMap<PointOfInterest, PointsOfInterestDto>();
            CreateMap<PointsOfInterestForCreationDTO, PointOfInterest>();
            CreateMap<PointsOfInterestForUpdatingDto, PointOfInterest>();
            CreateMap<PointOfInterest, PointsOfInterestForUpdatingDto>();
        }
    }
}
