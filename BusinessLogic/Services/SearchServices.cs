﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;

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
        public List<string> SearchArtistAllSongsByName(string artistNameParam, int offsetParam, int limitParam)
        {
            // query is a Lucene querry and limit is 1 to 100 these values sent as querry string
            string ApiUrlToGetArtistsAllSongs = String.Format("https://musicbrainz.org/ws/2/recording/?limit={0}&offset={1}&query=artist:{2}", limitParam, offsetParam, artistNameParam);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiUrlToGetArtistsAllSongs);
            request.Method = "GET";
            // https://musicbrainz.org/doc/XML_Web_Service/Rate_Limiting due to rate limiting I am asked to add userAgent info. Otherwise I was getting 403 forbidden error
            request.UserAgent = "Tech Test/<1.0> ( ozgurtezel89@gmail.com )";
            request.Accept = "application/json";

            HttpWebResponse response = null;
            response = (HttpWebResponse)request.GetResponse();
            string responseJsonString = "";
            using (Stream stream = response.GetResponseStream() )
            {
                StreamReader streamReader = new StreamReader(stream);
                responseJsonString =  streamReader.ReadToEnd();
                streamReader.Close();
            }

            dynamic json = JsonConvert.DeserializeObject(responseJsonString);
            List<string> songNames = new List<string>();
            foreach (var item in json.recordings)
            {
                songNames.Add(item.title.Value);
            }

            return songNames;
        }

    }
}