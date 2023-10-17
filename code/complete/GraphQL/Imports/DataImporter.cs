using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ConferencePlanner.GraphQL.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConferencePlanner.GraphQL.Imports
{
    public class DataImporter
    {
        public async Task LoadDataAsync(ApplicationDbContext db)
        {
            await using FileStream stream = File.OpenRead("Imports/NDC_London_2019.json");
            using JsonTextReader reader = new JsonTextReader(new StreamReader(stream));

            JArray conference = await JArray.LoadAsync(reader);
            Dictionary<string, Speaker> speakers = new Dictionary<string, Speaker>();

            foreach (JToken conferenceDay in conference)
            {
                foreach (JToken roomData in conferenceDay["rooms"]!)
                {
                    Track track = new Track
                    {
                        Name = roomData["name"]!.ToString(),
                    };

                    foreach (JToken sessionData in roomData["sessions"]!)
                    {
                        Session session = new Session
                        {
                            Title = sessionData["title"]!.ToString(),
                            Abstract = sessionData["description"]!.ToString(),
                            StartTime = sessionData["startsAt"]!.Value<DateTime>(),
                            EndTime = sessionData["endsAt"]!.Value<DateTime>(),
                        };

                        track.Sessions.Add(session);

                        foreach (JToken speakerData in sessionData["speakers"]!)
                        {
                            string id = speakerData["id"]!.ToString();

                            if (!speakers.TryGetValue(id, out Speaker? speaker))
                            {
                                speaker = new Speaker
                                {
                                    Name = speakerData["name"]!.ToString(),
                                };

                                speakers.Add(id, speaker);
                                db.Speakers.Add(speaker);
                            }

                            session.SessionSpeakers.Add(new SessionSpeaker
                            {
                                Speaker = speaker, Session = session,
                            });
                        }
                    }

                    db.Tracks.Add(track);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
