using System.Net;
using System.Text.Json;
using MovieCatalog.Models;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;

namespace MovieCatalog
{
    [TestFixture]
    public class MovieCatalogApiTests
    {
        private RestClient client;

        private static string createdMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIyZjRkYzE1NC1jMWMzLTQ1Y2EtODkwZi0yNDU5ZGRlYmU3OTQiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjM5OjM3IiwiVXNlcklkIjoiNjhjNjRkODItMjU4Mi00YWVhLTYyNWMtMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJzaW1vbnNAYWJ2LmJnIiwiVXNlck5hbWUiOiJzaW1vbnN2cCIsImV4cCI6MTc3NjUxNTk3NywiaXNzIjoiTW92aWVDYXRhbG9nX0FwcF9Tb2Z0VW5pIiwiYXVkIjoiTW92aWVDYXRhbG9nX1dlYkFQSV9Tb2Z0VW5pIn0.yumlgtiPiApvOr1Vn5avgYG3Bn5Dy2YFoOZvNr_BknQ";
        private const string LoginEmail = "simons@abv.bg";
        private const string LoginPassword = "123123";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new
            {
                email,
                password
            });

            var response = tempClient.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(
                    $"Failed to authenticate. Status code: {response.StatusCode}. Response: {response.Content}");
            }

            var content = JsonSerializer.Deserialize<JsonElement>(response.Content!);
            var token = content.GetProperty("accessToken").GetString();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("Access token not found in the response.");
            }

            return token;
        }

        [Test, Order(1)]
        public void Test_CreateMovieWithRequiredFields()
        {
            var request = new RestRequest("/api/Movie/Create", Method.Post);

            var newMovie = new MovieDTO
            {
                Title = "My API Movie",
                Description = "Movie created by automated API test.",
                PosterUrl = "",
                TrailerLink = "",
                IsWatched = false
            };

            request.AddJsonBody(newMovie);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseDto = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);

            Assert.That(responseDto, Is.Not.Null);
            Assert.That(responseDto!.Movie, Is.Not.Null);
            Assert.That(responseDto.Movie!.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(responseDto.Msg, Is.EqualTo("Movie created successfully!"));

            createdMovieId = responseDto.Movie.Id!;
        }

        [Test, Order(2)]
        public void Test_EditCreatedMovie()
        {
            Assert.That(createdMovieId, Is.Not.Null.And.Not.Empty);

            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", createdMovieId);

            var editedMovie = new MovieDTO
            {
                Title = "Edited API Movie",
                Description = "Movie edited by automated API test.",
                PosterUrl = "",
                TrailerLink = "",
                IsWatched = true
            };

            request.AddJsonBody(editedMovie);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseDto = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);

            Assert.That(responseDto, Is.Not.Null);
            Assert.That(responseDto!.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Test, Order(3)]
        public void Test_GetAllMovies()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var movies = JsonSerializer.Deserialize<List<MovieDTO>>(response.Content!);

            Assert.That(movies, Is.Not.Null);
            Assert.That(movies!.Count, Is.GreaterThan(0));
        }

        [Test, Order(4)]
        public void Test_DeleteCreatedMovie()
        {
            Assert.That(createdMovieId, Is.Not.Null.And.Not.Empty);

            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", createdMovieId);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseDto = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);

            Assert.That(responseDto, Is.Not.Null);
            Assert.That(responseDto!.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Test, Order(5)]
        public void Test_CreateMovieWithoutRequiredFields()
        {
            var request = new RestRequest("/api/Movie/Create", Method.Post);

            var invalidMovie = new MovieDTO
            {
                Title = "",
                Description = "",
                PosterUrl = "",
                TrailerLink = "",
                IsWatched = false
            };

            request.AddJsonBody(invalidMovie);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void Test_EditNonExistingMovie()
        {
            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", "123456789012345678901234");

            var editedMovie = new MovieDTO
            {
                Title = "Edited Non Existing Movie",
                Description = "This movie does not exist.",
                PosterUrl = "",
                TrailerLink = "",
                IsWatched = true
            };

            request.AddJsonBody(editedMovie);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var responseDto = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);

            Assert.That(responseDto, Is.Not.Null);
            Assert.That(responseDto!.Msg,
                Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Test, Order(7)]
        public void Test_DeleteNonExistingMovie()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", "123456789012345678901234");

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var responseDto = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);

            Assert.That(responseDto, Is.Not.Null);
            Assert.That(responseDto!.Msg,
                Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            client?.Dispose();
        }
    }
}