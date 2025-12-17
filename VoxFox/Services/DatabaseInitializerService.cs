// using Microsoft.EntityFrameworkCore;
// using VoxFox.Data.Seeders;
// using VoxFox.Interfaces.Services;
// using VoxFox.Models.Entities;
// using VoxFox.Utils;

// namespace VoxFox.Services
// {
// 	public class DatabaseInitializerService : BackgroundService
// 	{
// 		private readonly IServiceProvider _serviceProvider;
// 		private ILanguageService _languageService;
// 		private readonly IServiceScopeFactory _serviceScopeFactory;


// 		private const string CountryFranceKey = "CountryFranceKey";
// 		private const string CountryUnitedArabEmiratesKey = "CountryUnitedArabEmiratesKey";
// 		private const string CountryChinaKey = "CountryChinaKey";
// 		private const string CountryRussiaKey = "CountryRussiaKey";
// 		private const string CountryKazakhstanKey = "CountryKazakhstanKey";

// 		private const string CityRussiaMoscowKey = "CityRussiaMoscowKey";
// 		private const string CityFranceParisKey = "CityFranceParisKey";
// 		private const string CityFranceLyonKey = "CityFranceLyonKey";
// 		private const string CityUAEAbuDhabiKey = "CityUAEAbuDhabiKey";
// 		private const string CityUAEDubaiKey = "CityUAEDubaiKey";
// 		private const string CityChinaBeijingKey = "CityChinaBeijingKey";
// 		private const string CityChinaShanghaiKey = "CityChinaShanghaiKey";
// 		private const string CityKazakhstanAlmatyKey = "CityKazakhstanAlmatyKey";
// 		private const string CityKazakhstanAstanaKey = "CityKazakhstanAstanaKey";

// 		private static Dictionary<string, List<string>> countriesCity = new()
// 		{
// 			{ CountryRussiaKey, new List<string> { CityRussiaMoscowKey } },
// 			{ CountryFranceKey, new List<string> { CityFranceParisKey, CityFranceLyonKey } },
// 			{ CountryUnitedArabEmiratesKey, new List<string> { CityUAEAbuDhabiKey, CityUAEDubaiKey } },
// 			{ CountryChinaKey, new List<string> { CityChinaBeijingKey, CityChinaShanghaiKey } },
// 			{ CountryKazakhstanKey, new List<string> { CityKazakhstanAlmatyKey, CityKazakhstanAstanaKey } }
// 		};

// 		private static List<string> EnglishRoomTypeNames = new List<string>
// 		{
// 			"Bedroom",
// 			"Dining room",
// 			"Elevator",
// 			"Fitness",
// 			"Hall",
// 			"Hallway",
// 			"Kitchen",
// 			"Laundry",
// 			"Library",
// 			"Living room",
// 			"Massage room",
// 			"Master Bedroom",
// 			"Music Hall",
// 			"Pandus",
// 			"Parking",
// 			"Pool",
// 			"Porch",
// 			"Rest zone",
// 			"Salon",
// 			"Sauna",
// 			"Shower room",
// 			"Stair",
// 			"Staff room",
// 			"Tech.Room",
// 			"Terrace",
// 			"Wardrobe",
// 			"WC"
// 		};

// 		public DatabaseInitializerService(IServiceProvider serviceProvider, IServiceScopeFactory serviceScopeFactory)
// 		{
// 			_serviceProvider = serviceProvider;
// 			_serviceScopeFactory = serviceScopeFactory;
// 		}

// 		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
// 		{
// 			// Console.WriteLine("============== Database initialization in progress ==============");

// 			// using (var scope = _serviceProvider.CreateScope())
// 			// {
// 			// 	var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
// 			// 	var languageService = scope.ServiceProvider.GetRequiredService<ILanguageService>();

// 			// 	await FilledLanguages(context);
// 			// 	await FilledCountries(context);
// 			// 	await FilledCities(context);
// 			// 	await FillRoomTypesAsync(context, languageService);
// 			// 	await FilledAdmin(context);
// 			// 	await FilledTestUsers(20, context);

// 			// 	await FillHouseholdItemCategories(context);
// 			// 	await FillHouseholdItemCategoriesNewTranslates(context);
// 			// 	await PermissionSeeder.SeedAsync(context);
// 			// 	await RoleSeeder.SeedAsync(context);
// 			// 	await PositionSeeder.SeedAsync(context);

// 			// 	//var taskDistributionService = scope.ServiceProvider.GetRequiredService<TaskDistributionService>();

// 			// 	//await taskDistributionService.StartAsync();
// 			// }
// 		}




// 		private async Task FilledTestUsers(int count, ApplicationDbContext context)
// 		{
// 			var users = new List<User>();

// 			using (var scope = _serviceProvider.CreateScope())
// 			{
// 				for (int i = 1; i < count + 1; i++)
// 				{
// 					var email = $"test{i}@bafid.com";
// 					var password = $"PasswordTest{i}@!";
// 					var salt = PasswordHasher.GenerateSalt();
// 					var passwordHash = PasswordHasher.HashPassword(password, salt);

// 					var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email);
// 					if (user != null) continue;

// 					var newUser = new User
// 					{
// 						Birthday = DateTime.UtcNow,
// 						Email = email,
// 						PasswordHash = passwordHash,
// 						FirstName = "Test",
// 						LastName = "Test",
// 						Salt = salt,
// 						//UserRole = Enums.UserRole.User,
// 						CreatedAt = DateTime.UtcNow,
// 					};

// 					users.Add(newUser);
// 				}
// 			}

// 			context.Users.AddRange(users);
// 			await context.SaveChangesAsync();
// 		}

// 		private async Task FillHouseholdItemCategoriesNewTranslates(ApplicationDbContext context)
// 		{
// 		}

// 		private async Task FilledHouseholditem(ApplicationDbContext context)
// 		{
// 		}

// 		private async Task FillHouseholdItemCategories(ApplicationDbContext context)
// 		{
// 			try
// 			{
// 				string projectRoot = Directory.GetCurrentDirectory();

// 				string filePath = Path.Combine(projectRoot, "only equipment and cathegory.xlsx");

// 				var equipmentsExcelParser = new Utils.EquipmentsExcelParser();
// 				var householdItemInfos = equipmentsExcelParser.ParseAndGroup(filePath);

// 				if (!householdItemInfos.Any())
// 					return;

// 				var householdItemCategories = new List<HouseholdItemCategory>();

// 				//foreach (var householdItemInfo in householdItemInfos)
// 				//{
// 				//	householdItemCategories.Add(new HouseholdItemCategory
// 				//	{
// 				//		Translations = ,
// 				//		HouseholdItems = householdItemInfo.Select(x => new HouseholdItem
// 				//		{
// 				//			NameEnglishTemp = x.HouseholdItem
// 				//		})
// 				//		  .ToList()
// 				//	});
// 				//}

// 				using var scope = _serviceScopeFactory.CreateScope();
// 				var _languageService = scope.ServiceProvider.GetRequiredService<ILanguageService>();
// 				var languageId = await _languageService.GetLanguageIdEnglishAsync();

// 				//foreach (var newCategory in householdItemCategories)
// 				//{
// 				//	var translationName = newCategory.NameEnglishTemp;

// 				//	var existingCategory = await context.HouseholdItemCategories
// 				//		.Include(x => x.HouseholdItems)
// 				//		.FirstOrDefaultAsync(c => c.NameEnglishTemp == translationName);

// 				//	if (existingCategory == null)
// 				//	{
// 				//		var category = new HouseholdItemCategory
// 				//		{
// 				//			IsActive = true,
// 				//			CreatedAt = DateTime.UtcNow,
// 				//			UpdatedAt = DateTime.UtcNow,
// 				//			NameEnglishTemp = translationName,
// 				//			HouseholdItems = newCategory.HouseholdItems.Select(i => new HouseholdItem
// 				//			{
// 				//				NameLocalizationKey = "",
// 				//				DescriptionKey = "",
// 				//				NameEnglishTemp = i.NameEnglishTemp,
// 				//				//CreatedAt = DateTime.UtcNow,
// 				//				//UpdatedAt = DateTime.UtcNow,
// 				//				//IsActive = true,
// 				//			}).ToList()
// 				//		};

// 				//		context.HouseholdItemCategories.Add(category);
// 				//		await context.SaveChangesAsync();
// 				//	}
// 				//	else
// 				//	{
// 				//		foreach (var householdItemTemp in newCategory.HouseholdItems)
// 				//		{
// 				//			var existstsss11 = existingCategory.HouseholdItems.Any(x => x.NameEnglishTemp == householdItemTemp.NameEnglishTemp);
// 				//			var householdItem123123123 = existingCategory.HouseholdItems.FirstOrDefault(x => x.NameEnglishTemp == existingCategory.NameEnglishTemp);
// 				//			if (!existstsss11)
// 				//			{
// 				//				context.HouseholdItems.Add(new HouseholdItem
// 				//				{
// 				//					NameEnglishTemp = householdItemTemp.NameEnglishTemp,
// 				//					NameLocalizationKey = "",
// 				//					DescriptionKey = "",
// 				//					HouseholdItemCategoryId = existingCategory.HouseholdItemCategoryId
// 				//				});
// 				//				await context.SaveChangesAsync();
// 				//			}
// 				//		}
// 				//	}
// 				//}
// 			}
// 			catch (Exception ex)
// 			{
// 				throw;
// 			}
// 		}



// 		private async Task FilledLanguages(ApplicationDbContext context)
// 		{
// 			var languages = new List<Language>
// 			{
// 				new Language { Code = "ru-RU", Name = "Russian (Russia)" },
// 				new Language { Code = "en-US", Name = "English (United States)" },
// 				new Language { Code = "fr-FR", Name = "French (France)" },
// 				new Language { Code = "zh-CN", Name = "Chinese (Simplified, China)" },
// 				new Language { Code = "ar-SA", Name = "Arabic (Saudi Arabia)" }
// 			};

// 			foreach (var language in languages)
// 			{
// 				var existing = await context.Languages
// 					.FirstOrDefaultAsync(l => l.Code == language.Code);

// 				if (existing == null)
// 				{
// 					context.Languages.Add(language);
// 				}
// 				else if (existing.Name != language.Name)
// 				{
// 					existing.Name = language.Name;
// 					context.Languages.Update(existing);
// 				}
// 			}

// 			await context.SaveChangesAsync();
// 		}

// 		private async Task FilledCities(ApplicationDbContext context)
// 		{
// 			var countries = await context.Countries.ToListAsync();
// 			var existingCities = await context.Cities.ToListAsync();

// 			foreach (var country in countriesCity)
// 			{
// 				var countryId = countries.FirstOrDefault(c => c.NameLocalizationKey == country.Key)?.Id ?? 0;

// 				if (countryId != 0)
// 				{
// 					foreach (var cityKey in country.Value)
// 					{
// 						if (!existingCities.Any(c => c.NameLocalizationKey == cityKey))
// 						{
// 							var city = new Models.Entities.City
// 							{
// 								NameLocalizationKey = cityKey,
// 								CountryId = countryId
// 							};

// 							context.Cities.Add(city);
// 						}
// 					}
// 				}
// 			}

// 			await context.SaveChangesAsync();
// 		}

// 		private async Task FilledAdmin(ApplicationDbContext context)
// 		{
// 			var email = "ot@bafid.app";
// 			var password = "WRv(cSaO_M@*L8zB";

// 			var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email);
// 			if (user != null) return;

// 			using (var scope = _serviceProvider.CreateScope())
// 			{
// 				var salt = PasswordHasher.GenerateSalt();
// 				var passwordHash = PasswordHasher.HashPassword(password, salt);
// 				user = new User
// 				{
// 					Email = email,
// 					PasswordHash = passwordHash,
// 					//Permissions = (int)Permission.All,
// 					FirstName = "admin",
// 					Birthday = DateTime.UtcNow,
// 					LastName = "admin",
// 					Salt = salt,
// 					//UserRole = Enums.UserRole.SuperAdmin,
// 				};

// 				context.Add(user);
// 				await context.SaveChangesAsync();
// 			}
// 		}

// 		private async Task FillRoomTypesAsync(ApplicationDbContext context, ILanguageService languageService)
// 		{
// 			var englishLanguageId = await languageService.GetLanguageIdEnglishAsync();

// 			var existingTranslations = await context.RoomTypeTranslates
// 				.Where(rt => rt.LanguageId == englishLanguageId)
// 				.Select(rt => rt.TranslatedName)
// 				.ToListAsync();

// 			var newRoomTypes = EnglishRoomTypeNames
// 				.Where(name => !existingTranslations.Contains(name))
// 				.Select(name => new RoomType
// 				{
// 					RoomTypeTranslates = new List<RoomTypeTranslate>
// 					{
// 						new RoomTypeTranslate
// 						{
// 							TranslatedName = name,
// 							LanguageId = englishLanguageId
// 						}
// 					}
// 				})
// 				.ToList();

// 			if (newRoomTypes.Any())
// 			{
// 				context.RoomTypes.AddRange(newRoomTypes);
// 				await context.SaveChangesAsync();
// 			}
// 		}


// 		private static async Task FilledCountries(ApplicationDbContext context)
// 		{
// 			var countriesToAdd = new List<Country>
// 			{
// 				new() { NameLocalizationKey = CountryUnitedArabEmiratesKey },
// 				new() { NameLocalizationKey = CountryChinaKey },
// 				new() { NameLocalizationKey = CountryRussiaKey },
// 				new() { NameLocalizationKey = CountryKazakhstanKey },
// 				new() { NameLocalizationKey = CountryFranceKey },
// 			};

// 			var existingCountries = await context.Countries.ToListAsync();

// 			foreach (var country in countriesToAdd)
// 			{
// 				if (!existingCountries.Any(c => c.NameLocalizationKey == country.NameLocalizationKey))
// 				{
// 					context.Countries.Add(country);
// 				}
// 			}

// 			await context.SaveChangesAsync();
// 		}
// 	}
// }
