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

module OptimizedRouting
    open System
    type MapLocation = { Latitude : double; Longitude: double; Altitude : double } with override l.ToString() = String.Format("[{0:f3},{1:f3},{2:f3}]", l.Latitude, l.Longitude, l.Altitude)
    type Waypoint = { WaypointId : int; Name : string; Location : MapLocation; IsStartPoint : bool; IsEndPoint : bool; OrigRouteOrder: int; } with override w.ToString() = String.Format("{0} {1}", w.Name, w.Location)
    type RouteLeg = { FromPoint : Waypoint; ToPoint : Waypoint; mutable Distance : double; mutable TimeInSeconds : Int64; mutable CalcIsCompleted : bool; } with override r.ToString() = String.Format("{0} => {1} (Distance: {2:f4}", r.FromPoint, r.ToPoint, r.Distance)
    type Route (legs : RouteLeg array) = 
        member this.RouteLegs = legs
        member this.TotalDistance = legs |> Seq.map ( fun leg -> leg.Distance ) |> Seq.sum
    let CreateWaypoint ( id, name, latitude, longitude, isStart, isEnd, origOrder ) = { WaypointId = id; Name = name; Location = { Latitude = latitude; Longitude = longitude; Altitude = 0.00 }; IsStartPoint = isStart; IsEndPoint = isEnd; OrigRouteOrder = origOrder; }

    let GetAllRouteLegs ( waypoints : Waypoint seq ) = 
        seq { for x in waypoints do 
                for y in (waypoints |> Seq.filter ( fun pt -> not (pt.WaypointId = x.WaypointId) )) -> { FromPoint = x; ToPoint = y; Distance = 0.00; CalcIsCompleted = false; TimeInSeconds = Convert.ToInt64(0); } }

    let GetAllRoutes ( points : Waypoint seq, legs : RouteLeg seq ) = 
        let pts_array = points |> Seq.toArray
        let rec GetAllCombos ( pts : int list, options : int list ) = 
            match options.Length with 
            | 0 -> [ pts ]
            | _ -> [ for option in options do 
                        let subset = seq { for item in GetAllCombos ( pts @ [ option ], ( options |> List.filter ( fun x -> not (x = option) ) ) ) -> item }
                        for inner_option in subset -> inner_option ]                
        let routes = seq { for item in GetAllCombos ( List.empty, [ 1 .. (points |> Seq.length) ] ) -> (item |> List.toArray) } 
        let RouteLegFinder ( fromIndex, toIndex ) = 
            legs |> Seq.filter ( fun leg -> leg.FromPoint = pts_array.[fromIndex-1] && leg.ToPoint = pts_array.[toIndex-1] ) |> Seq.head
        routes |> Seq.map ( fun rt -> Route( ( seq { for x = 0 to (rt.Length - 2) do yield RouteLegFinder(rt.[x], rt.[x+1]) } |> Seq.toArray ) ) )
        
        
