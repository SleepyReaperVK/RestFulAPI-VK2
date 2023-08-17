using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RestFulAPI_VK.Controllers
{
    /* unfortunately I am unable to access the API to Let's asume I have and move-on, thank you*/

    [Route("api/[controller]")]
    public class RandoUserController : Controller
    {
        private const int NumberOfGenderUsers = 10;
        private const int NumberOfCountryUsers = 5000;
        private const int NumberOfMailUsers = 30;
        private const int NumberOfOldestUsers = 100;

        private async Task<IActionResult> GenerateUsers(string condition) // generate the users based on the condition
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri("https://api.randomuser.me");
                    var response = await client.GetAsync("https://api.randomuser.me/" + (string.IsNullOrEmpty(condition) ? "" : condition));
                    response.EnsureSuccessStatusCode();

                    var stringResult = await response.Content.ReadAsStringAsync();
                    RootObject result = JsonConvert.DeserializeObject<RootObject>(stringResult);
                    return Ok(result.results);
                }
                catch (HttpRequestException httpRequestException)
                {
                    return BadRequest($"Error generating users: {httpRequestException.Message}");
                }
            }
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Generate()
        {
            return await GenerateUsers(null); // should display one user
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GenerateWithGender(string gender)
        {
            return await GenerateUsers($"/?results={NumberOfGenderUsers}&gender={gender}"); // should display 10 users by a spesific gender.
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetMostPopularCountry() // should display the most popular country after chaking 5000 users
        {
            var countryCounts = new Dictionary<string, int>();

            for (int i = 0; i < NumberOfCountryUsers; i++)
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri("https://api.randomuser.me");
                        var response = await client.GetAsync("https://api.randomuser.me/");
                        response.EnsureSuccessStatusCode();

                        var stringResult = await response.Content.ReadAsStringAsync();
                        RootObject result = JsonConvert.DeserializeObject<RootObject>(stringResult);

                        string country = result.results[0].location.country;
                        if (countryCounts.ContainsKey(country))
                        {
                            countryCounts[country]++;
                        }
                        else
                        {
                            countryCounts[country] = 1;
                        }
                    }
                    catch (HttpRequestException httpRequestException)
                    {
                        return BadRequest($"Error generating users: {httpRequestException.Message}");
                    }
                }
            }

            string mostPopularCountry = countryCounts.OrderByDescending(kvp => kvp.Value).First().Key;

            return Ok(new { MostPopularCountry = mostPopularCountry });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetRandomUserEmails() // should display the 30 mails 
        {
            var emails = new List<string>();

            for (int i = 0; i < NumberOfMailUsers; i++)
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri("https://api.randomuser.me");
                        var response = await client.GetAsync("https://api.randomuser.me/");
                        response.EnsureSuccessStatusCode();

                        var stringResult = await response.Content.ReadAsStringAsync();
                        RootObject result = JsonConvert.DeserializeObject<RootObject>(stringResult);

                        string email = result.results[0].email;
                        emails.Add(email);
                    }
                    catch (HttpRequestException httpRequestException)
                    {
                        return BadRequest($"Error generating users: {httpRequestException.Message}");
                    }
                }
            }

            return Ok(emails);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetOldestUser() // should display the most oldest user from 100 , name + age
        {
            DateTime oldestBirthdate = DateTime.MinValue;
            PersonAPIResponse oldestUser = null;

            for (int i = 0; i < NumberOfOldestUsers; i++)
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri("https://api.randomuser.me");
                        var response = await client.GetAsync("https://api.randomuser.me/");
                        response.EnsureSuccessStatusCode();

                        var stringResult = await response.Content.ReadAsStringAsync();
                        RootObject result = JsonConvert.DeserializeObject<RootObject>(stringResult);

                        DateTime birthdate = result.results[0].dob.date;
                        if (birthdate < oldestBirthdate)
                        {
                            oldestBirthdate = birthdate;
                            oldestUser = new PersonAPIResponse
                            {
                                Name = result.results[0].name,
                                Dob = result.results[0].dob,

                            };
                        }
                    }
                    catch (HttpRequestException httpRequestException)
                    {
                        return BadRequest($"Error generating users: {httpRequestException.Message}");
                    }
                }
            }

            return Ok(oldestUser);
        }

        public static List<MyUser> Users = new List<MyUser>(); // lest make a static list instead of static data

        [HttpPost("[action]")]
        public IActionResult AddUser([FromBody] MyUser newUser) // adding  the user
        {
            Users.Add(newUser);
            return Ok($"User {newUser.Name} added successfully!");
        }
        [HttpGet("[action]/{id}")]
        public IActionResult GetUser(int id) // requesting the user by id 
        {
            var user = Users.Find(u => u.Id == id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            return Ok(user);
        }
    }

    public class PersonAPIResponse
        {
            public string Gender { get; set; }
            public Name Name { get; set; }
            public Location Location { get; set; }
            public string Email { get; set; }
            public Login Login { get; set; }
            public Dob Dob { get; set; }
            public Registered Registered { get; set; }
            public string Phone { get; set; }
            public string Cell { get; set; }
            public Id Id { get; set; }
            public Picture Picture { get; set; }
            public string Nat { get; set; }
        }


        public class Name
        {
            public string title { get; set; }
            public string first { get; set; }
            public string last { get; set; }
        }

        public class Coordinates
        {
            public string latitude { get; set; }
            public string longitude { get; set; }
        }

        public class Timezone
        {
            public string offset { get; set; }
            public string description { get; set; }
        }

        public class Location
        {
            public string street { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public int postcode { get; set; }
            public Coordinates coordinates { get; set; }
            public Timezone timezone { get; set; }
        }

        public class Login
        {
            public string uuid { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string salt { get; set; }
            public string md5 { get; set; }
            public string sha1 { get; set; }
            public string sha256 { get; set; }
        }

        public class Dob
        {
            public DateTime date { get; set; }
            public int age { get; set; }
        }

        public class Registered
        {
            public DateTime date { get; set; }
            public int age { get; set; }
        }

        public class Id
        {
            public string name { get; set; }
            public object value { get; set; }
        }

        public class Picture
        {
            public string large { get; set; }
            public string medium { get; set; }
            public string thumbnail { get; set; }
        }

        public class Result
        {
            public string gender { get; set; }
            public Name name { get; set; }
            public Location location { get; set; }
            public string email { get; set; }
            public Login login { get; set; }
            public Dob dob { get; set; }
            public Registered registered { get; set; }
            public string phone { get; set; }
            public string cell { get; set; }
            public Id id { get; set; }
            public Picture picture { get; set; }
            public string nat { get; set; }
        }

        public class Info
        {
            public string seed { get; set; }
            public int results { get; set; }
            public int page { get; set; }
            public string version { get; set; }
        }

        public class RootObject
        {
            public List<Result> results { get; set; }
            public Info info { get; set; }
        }



        public class MyUser
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Gender { get; set; }
            public string Phone { get; set; }
            public string Country { get; set; }
        }
        




    }



}




