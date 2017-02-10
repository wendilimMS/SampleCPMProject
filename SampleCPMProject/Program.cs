namespace SampleCPMProject
{
    using Microsoft.CustomerPreferences.Api.Contracts;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class Program
    {
        // swap out the methods to test out each functionality
        // don't forget to populate the client secret and client id in App.Config
        public static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                await PatchContactPoint(ContactPointType.Email, "test@microsoft.com", new Guid("00000000-0000-0000-0000-000000000001"), ContactPointTopicSettingState.OptInExplicit);
            }).Wait();
            Console.ReadKey();
        }

        /// <summary>
        /// Given a list of contact values and a particular topic id (or the keyword "transactional"), check if those contact values
        /// can be contacted by the topic id.
        /// This end point is pessimistic, in that if CPM does not know about the contact value or if the contact value does not know about the topic,
        /// the "CanContact" value will always return false.
        /// </summary>
        /// <param name="contactValues">The list of contact values to check. Maximum size 100.</param>
        /// <param name="contactPointType">The type of contact point to check.</param>
        /// <param name="topicId">The topic id, or the keyword "transactional".</param>
        /// <returns></returns>
        private static async Task GetContactabilities(List<string> contactValues, ContactPointType contactPointType, string topicId)
        {
            CheckContactabilitiesRequest2 request = new CheckContactabilitiesRequest2()
            {
                ContactType = contactPointType,
                TargetedTopic = topicId
            };

            foreach (string contactValue in contactValues)
            {
                request.ContactPoints.Add(contactValue);
            }

            using (HttpClient client = await CPMClientGenerator.CreateHttpClientAsync())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync("api/contactabilities?api-version=2.0", request);

                if (response.IsSuccessStatusCode)
                {
                    CheckContactabilitiesResult2 result = await response.Content.ReadAsAsync<CheckContactabilitiesResult2>();
                    Console.WriteLine(JsonConvert.SerializeObject(result));
                }
                else
                {
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }
            }
        }

        /// <summary>
        /// Given an existing contact point type and contact point value, looks up the contact point and prints it out.
        /// </summary>
        /// <param name="contactPointType">The type of contact point to retrieve eg. email</param>
        /// <param name="contactPointValue">The actual value of the contact point eg. email@address.com</param>
        /// <returns></returns>
        private static async Task GetContactPoint(ContactPointType contactPointType, string contactPointValue)
        {
            using (HttpClient client = await CPMClientGenerator.CreateHttpClientAsync())
            {
                HttpMethod get = new HttpMethod("GET");
                HttpRequestMessage request = new HttpRequestMessage(get, "api/ContactPoints");
                request.Headers.Add("x-ms-contact-value", contactPointValue);
                request.Headers.Add("x-ms-contact-type", contactPointType.ToString());
                request.Headers.Add("Accept", "application/json");
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    LookupContactPoint contactPoint = await response.Content.ReadAsAsync<LookupContactPoint>();
                    Console.WriteLine(JsonConvert.SerializeObject(contactPoint));
                } 
                else
                {
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }
            }
        }

        /// <summary>
        /// Patches a contact point with a subscription to a topic. Will create a new contact point if the contact point does not exist. Cannot be used to mark a contact point as inactive <see cref="DeleteContactPoint"/>.
        /// </summary>
        /// <param name="contactPointType">The type of contact point to modify eg. email</param>
        /// <param name="contactPointValue">The actual value of the contact point eg. email@address.com</param>
        /// <param name="topicId">the identifier of the topic</param>
        /// <param name="state">The state the subscription should be in eg. OptInExplicit</param>
        /// <returns></returns>
        private static async Task PatchContactPoint(ContactPointType contactPointType, string contactPointValue, Guid topicId, ContactPointTopicSettingState state)
        {
            ContactPoint contactPoint = new ContactPoint()
            {
                ContactType = contactPointType,
                ContactValue = contactPointValue
            };
            contactPoint.TopicSettings.Add(new ContactPointTopicSetting
            {
                TopicId = topicId,
                CultureName = CultureInfo.CurrentCulture.ToString(),
                LastSourceSetDate = DateTime.UtcNow,
                OriginalSource = "SampleCPMProject",
                State = state
            });
            using (HttpClient client = await CPMClientGenerator.CreateHttpClientAsync())
            {
                HttpResponseMessage response = await client.PatchAsync("api/ContactPoints", contactPoint);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }
            }
        }
    }
}
