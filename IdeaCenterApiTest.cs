using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using ExamPrepIdeaCenter.Models;
using System.Net.NetworkInformation;

namespace ExamPrepIdeaCenter

{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string lastCreatedIdeaId;

        private const string BaseURL = "http://144.91.123.158:82";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJlYmRkZDNkZC1mMGI2LTQ0ZDQtYmJhZS1jMmFjMjA4MDQxYzciLCJpYXQiOiIwNC8xNi8yMDI2IDE2OjA1OjQxIiwiVXNlcklkIjoiNzQ3MzRhY2UtMDAzZS00Njc1LTUzYTMtMDhkZTc2YTJkM2VjIiwiRW1haWwiOiJNTTIwMjZRQUBlbWFpbC5jb20iLCJVc2VyTmFtZSI6Ik1NXzIwMjZfUUEiLCJleHAiOjE3NzYzNzcxNDE6IklkZWFDZW50ZXJfQXBwX1NvZnRVbmkiLCJhdWQiOiJJZGVhQ2VudGVyX1dlYkFQSV9Tb2Z0VW5pIn0.vr2sL6ODRdN9426Gr7JaNOmEzE5JBXCHfcx6jej99Ik";
        private const string LoginEmail = "MM2026QA@email.com";
        private const string LoginPassword = "mm_1strongPass";

        [OneTimeSetUp] //if the username or password fail this will take over
        public void Setup()
        {
            string jwtToken = GetJwtToken(LoginEmail, LoginPassword);

            var options = new RestClientOptions(BaseURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken) //creates new token
            };

            this.client = new RestClient(options);

        }

        private string GetJwtToken(string email, string password) //creating the method, which will recreate setup if authentication fails
        {
            var tempClient = new RestClient(BaseURL);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);


            if (response.StatusCode == HttpStatusCode.OK)
            {
                //Assert.Fail($"DEBUG JSON CONTENT: {response.Content}"); //to forse show the json output - check what exact name it uses = "accessToken"

                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("AccessToken property was empty.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status: {response.StatusCode}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateIdea_WithRequiredFileds_ShouldReturnSuccess()
        {
            var ideaData = new IdeaDTO
            {
                Title = "Test Idea",
                Description = "This is a test idea description.",
                URL = ""
            };

            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(ideaData);

            var response = this.client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Assert.Fail($"Request failed with status {response.StatusCode}. Content: {response.Content}");
            }

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"), "Response is as expected.");

        }
        [Order(2)]
        [Test]
        public void GetAllIdeas_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Idea/All", Method.Get);
            var response = this.client.Execute(request);

            var responseList = JsonSerializer.Deserialize<List<ApiResponseDTO>> (response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(responseList, Is.Not.Empty, "List should not be empty.");
            Assert.That(responseList, Is.Not.Null, "List should not be null.");

            lastCreatedIdeaId = responseList.LastOrDefault()?.Id;

        }

        [Order(3)]
        [Test]
        public void EditExistIdeas_ShouldReturnSuccess()
        {

            var editRequestData = new IdeaDTO //sending request body
            {
                Title = "Edited Idea",
                Description = "This is an edited idea description.",
                URL = ""
            };

            var request = new RestRequest("/api/Idea/Edit", Method.Put);

            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            request.AddJsonBody(editRequestData);

            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"), "Response is as expected.");

        }

        [Order(4)]
        [Test] //response is string
        public void DeleteExistIdeas_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(response.Content, Is.EqualTo("\"The idea is deleted!\""));
       
        }

        [Order(5)]
        [Test]

     public void CreateIdea_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var ideaData = new IdeaDTO
            {
                Title = "",
                Description = "This is a test idea description.",
                URL = ""
            };
            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(ideaData);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
        }

        [Order(6)]
        [Test]

        public void EditNonExistIdea_ShouldReturnNotFound()
        {
            var editRequestData = new IdeaDTO
            {
                Title = "Edited Idea",
                Description = "This is an edited idea description.",
                URL = ""
            };
            var request = new RestRequest("/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", "non-existent-id");
            request.AddJsonBody(editRequestData);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
        }     

        [Order(7)]
        [Test]
        public void DeleteNonExistIdea_ShouldReturnNotFound()
        {
            var request = new RestRequest("/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", "non-existent-id");
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
        }


        [OneTimeTearDown]
        public void TearDown()
            { 
            this.client?.Dispose(); 
        }

    }
    }

