using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using TechTest.BusinessLogic.Models;

namespace TechTest.BusinessLogic.Services
{
    public class SearchServices 
    {
        /// <summary>
        /// takes the artist name and returns its first songs' names depending on given limit
        /// </summary>
        /// <param name="artistNameParam"></param>
        /// <param name="offsetParam"></param>
        /// <param name="limitParam"></param>
        /// <returns></returns>
        public ArtistAndSongs SearchArtistAllSongsByName(string artistNameParam, int offsetParam, int limitParam)
        {
            System.Web.Caching.Cache cache = new System.Web.Caching.Cache();
            // Server side caching machanism added
            List<ArtistAndSongs> artistNamesAndSongs = cache["artistNamesAndSongs"] as List<ArtistAndSongs>;

            ArtistAndSongs artistAndSongsToReturn = null ;

            if (artistNamesAndSongs != null)
            {
                artistAndSongsToReturn = artistNamesAndSongs.FirstOrDefault(x => x.ArtistName.Equals(artistNameParam));
            }

            if(artistAndSongsToReturn == null)
            {
                artistAndSongsToReturn = new ArtistAndSongs();
                artistNamesAndSongs = new List<ArtistAndSongs>();
                artistAndSongsToReturn = new ArtistAndSongs();
                artistAndSongsToReturn.Songs = new List<Song>();

                bool artistHasMoreResulsts = true;
                int offset = offsetParam;
                int limit = limitParam;

                // as there is a limit on the web-api to 100 below is the logic to get all the song names for the artist.
                while (artistHasMoreResulsts)
                {
                    // query is a Lucene querry and limit is 1 to 100 these values sent as querry string
                    string ApiUrlToGetArtistsAllSongs = String.Format("https://musicbrainz.org/ws/2/recording/?limit={0}&offset={1}&query=artist:{2}", limitParam, offsetParam, artistNameParam);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiUrlToGetArtistsAllSongs);
                    request.Method = "GET";
                    // https://musicbrainz.org/doc/XML_Web_Service/Rate_Limiting due to rate limiting I am asked to add userAgent info. Otherwise I was getting 403 forbidden error
                    request.UserAgent = "Tech Test/<1.1> ( ozgur.tezel@outlook.com )";
                    request.Accept = "application/json";

                    HttpWebResponse response = null;
                    response = (HttpWebResponse)request.GetResponse();
                    string responseJsonString = "";
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader streamReader = new StreamReader(stream);
                        responseJsonString = streamReader.ReadToEnd();
                        streamReader.Close();
                    }

                    dynamic json = JsonConvert.DeserializeObject(responseJsonString);
                    List<string> songNames = new List<string>();
                    foreach (var item in json.recordings)
                    {
                        artistAndSongsToReturn.Songs.Add(new Song { Title = item.title.Value });
                    }

                    offset += limit;

                    if (json.recordings.Count == 0)
                        artistHasMoreResulsts = false;
                }

                // as there might be duplicate records due to data on the web-api it is good to get rid off them (as per search term artist:tarkan there were 358 results before running the distinct method, now results are reduced to 203, will be more resource efficient when we use this song names to get the count of words)
                //artistAndSongsToReturn.Songs = (from d in artistAndSongsToReturn.Songs select d).GroupBy(x=>x.Title).ToList();
                artistAndSongsToReturn.Songs = artistAndSongsToReturn.Songs.GroupBy(x => new { x.Title }).Select(x => x.FirstOrDefault()).ToList();
                artistAndSongsToReturn.ArtistName = artistNameParam;

                // add new value to the cache
                artistNamesAndSongs.Add(artistAndSongsToReturn);
                cache["artistNamesAndSongs"] = artistNamesAndSongs;
            }

            return artistAndSongsToReturn;
        }

        public int GetCountOfWordsOfSong(string artistNameParam, string songNameParam)
        {            
            try
            {
                // query to get lrycs of a song
                string ApiUrlToGetLrycsOfSong = String.Format("https://api.lyrics.ovh/v1/{0}/{1}", artistNameParam, songNameParam);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiUrlToGetLrycsOfSong);
                request.Method = "GET";
                request.Accept = "application/json";

                HttpWebResponse response = null;
                response = (HttpWebResponse)request.GetResponse();
                string responseJsonString = "";
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader streamReader = new StreamReader(stream);
                    responseJsonString = streamReader.ReadToEnd();
                    streamReader.Close();
                }
                dynamic json = JsonConvert.DeserializeObject(responseJsonString);

                string lyrics = Regex.Replace(json.lyrics.Value, "\n", " ");

                string[] words = lyrics.Split(' ');

                // there are some empty elements in the array due to new line chars being replaced with " ", I need to ignore them - will add extra pressure to memory but will return accurate results

                return words.Count(x => x != "");
            }
            catch (Exception)
            {
                // I am not throwing any exception is here as I usualy would or at least write the song name to a spearate list to give user a notice saying this song could not be calculated
                return 0;
            }
        }
    }
}