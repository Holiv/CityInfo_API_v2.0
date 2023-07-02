using System;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.DbContexts
{
	public class CityInfoContext : DbContext
	{
		public DbSet<City> Cities { get; set; } = null!;

		public DbSet<PointOfInterest> PointOfInterests { get; set; } = null!;


		public CityInfoContext(DbContextOptions<CityInfoContext> options) : base(options) 
		{

		}

		//this override gives us access to the ModelBuilder
		//It can be used to provide data to seed the database with initial data so we can test it
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
            modelBuilder.Entity<City>().HasData(
				new City("New York City")
				{
					Id = 1,
					Description = "The one with that big park"
				},
				new City("Brazil")
				{
					Id = 2,
					Description = "The one that has most corrupt politician as president"
                },
				new City("Paris")
				{
					Id = 3,
					Description = "The one with that big tower"
				});
            modelBuilder.Entity<PointOfInterest>().HasData(
				new PointOfInterest("Central Park")
				{
					Id = 1,
					CityId = 1,
					Description = "The most visited urban park in the United States"
				},
				new PointOfInterest("Empire State Building")
				{
					Id = 2,
					CityId = 1,
					Description = "A 102-story skyscraper located in Midtwon Manhattan"
				},
				new PointOfInterest("Rio de Janeiro")
				{
					Id = 3,
					CityId = 2,
					Description = "Brazil best beaches"
				},
				new PointOfInterest("Campos dos Goytacazes")
				{
					Id = 4,
					CityId = 2,
					Description = "My birth city. A lot of history"
				},
				new PointOfInterest("Eifel Tower")
				{
					Id = 5,
					CityId = 3,
					Description = "A wrought iron lattice tower on the Champ de Mars."
				},
				new PointOfInterest("The Louvre")
				{
					Id = 6,
					CityId = 3,
					Description = "The world's largest museum."
				});
			base.OnModelCreating(modelBuilder);
		}
	}
}

