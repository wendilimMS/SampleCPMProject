using Microsoft.CustomerPreferences.Api.Contracts;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace SampleCPMProject
{
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
        }

        /// <summary>
        ///  Does a soft delete of a contact point by marking them as 'inactive'.
        /// </summary>
        /// <param name="contactPointType">The type of contact point to delete eg. email</param>
        /// <param name="contactPointValue">The actual value of the contact point eg. email@address.com</param>
        /// <returns></returns>
        private static async Task DeleteContactPoint(ContactPointType contactPointType, string contactPointValue)
        {
            using (HttpClient client = await CPMClientGenerator.CreateHttpClientAsync())
            {
                ContactPointIdentity contactPointIdentity = new ContactPointIdentity
                {
                    ContactType = contactPointType,
                    ContactValue = contactPointValue
                };
                HttpResponseMessage response = await client.DeleteAsync("api/ContactPoints", contactPointIdentity);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Contact point successfully deleted");
                    Console.ReadKey();
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
                HttpResponseMessage message = await client.GetAsync("api/Topics");
                if (message.IsSuccessStatusCode)
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
                        Console.ReadKey();
                    }
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
            using (HttpClient client = await CPMClientGenerator.CreateHttpClientAsync())
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

                HttpResponseMessage response = await client.PatchAsync("api/ContactPoints", contactPoint);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                    Console.ReadKey();
                }
            }
        }
    }
}
