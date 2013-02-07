// Learn more about F# at http://fsharp.net
#light
open System
open System.IO
open System.Net

// base url
let baseUrl = "http://maps.googleapis.com/maps/api/directions/"
// func to parse querystring and format (json/xml) into actionable url
let getUrl (format, querystring) = ("{0}{1}?{2}", baseUrl, format, querystring) |> String.Format
// func specially for json resultsets
let getJsonUrl (querystring) = ("json", querystring) |> getUrl
// Create Web Request and Get Response Text
let getResponseText ( querystring ) = 
    let req = (querystring |> getJsonUrl |> HttpWebRequest.Create) :?> HttpWebRequest
    let resp = req.GetResponse() :?> HttpWebResponse
    let reader = new StreamReader ( resp.GetResponseStream() )
    reader.ReadToEnd()

// walking directions
let getWalkingDirectionsAsJson ( querystring : String ) = 
    let w_dir = ("{0}&mode=walking", querystring ) |> String.Format
    getResponseText ( w_dir )

// Get Directions From / To
let getDirections (from_loc, to_loc) = 
    let url = ("origin={0}&destination={1}&sensor=false", from_loc, to_loc) |> String.Format
    url |> getWalkingDirectionsAsJson



    


