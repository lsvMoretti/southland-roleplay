using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace Server.Models
{
    [Table("busroutes")]
    public class BusRoute
    {
        /// <summary>
        /// Bus Route Id
        /// </summary>
        [Key]
        public int Id { get; set; }

        public string? RouteName { get; set; }

        /// <summary>
        /// List of BusStop that are JSON'd
        /// </summary>
        public string? BusStops { get; set; }

        /// <summary>
        /// Adds a Bus Route to the Database
        /// </summary>
        /// <param name="busRoute"></param>
        /// <returns>-1 - Route already exists by Name, otherwise returns new route Id</returns>
        public static int AddBusRoute(BusRoute busRoute)
        {
            using Context context = new Context();

            if (context.BusRoutes.FirstOrDefault(x => x.RouteName == busRoute.RouteName) != null)
            {
                //A bus route already exists with the route name we've inputed.
                return -1;
            }

            if (string.IsNullOrWhiteSpace(busRoute.BusStops))
            {
                busRoute.BusStops = JsonConvert.SerializeObject(new List<BusStop>());
            }

            context.BusRoutes.Add(busRoute);
            context.SaveChanges();

            return busRoute.Id;
        }

        /// <summary>
        /// Fetches Bus Route by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>NULL if not found - BusRoute if found</returns>
        public static BusRoute FetchBusRoute(int id)
        {
            Context context = new Context();
            return context.BusRoutes.Find(id);
        }

        /// <summary>
        /// Fetches Bus Route by Route Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>NULL if not found - BusRoute if found</returns>
        public static BusRoute FetchBusRoute(string name)
        {
            Context context = new Context();
            return context.BusRoutes.FirstOrDefault(s => s.RouteName == name);
        }

        /// <summary>
        /// Fetch Bus Stops by Route Id
        /// </summary>
        /// <param name="routeId"></param>
        /// <returns>List of Bus Stops</returns>
        public static List<BusStop> FetchBusStops(int routeId)
        {
            using Context context = new Context();
            var busRoute = context.BusRoutes.Find(routeId);

            if (busRoute == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<List<BusStop>>(busRoute.BusStops);
        }

        public static void AddBusStop(int routeId, BusStop busStop)
        {
            Context context = new Context();
            var busRoute = context.BusRoutes.Find(routeId);

            if (busRoute == null) return;

            var stopList = JsonConvert.DeserializeObject<List<BusStop>>(busRoute.BusStops);

            stopList.Add(busStop);

            busRoute.BusStops = JsonConvert.SerializeObject(stopList);

            context.SaveChanges();
        }

        public static List<BusRoute> FetchBusRoutes()
        {
            using Context context = new Context();
            return context.BusRoutes.ToList();
        }
    }

    public class BusStop
    {
        /// <summary>
        /// Bus Stop Name (Street)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Position X
        /// </summary>
        public float PosX { get; set; }

        /// <summary>
        /// Position Y
        /// </summary>
        public float PosY { get; set; }

        /// <summary>
        /// Position Z
        /// </summary>
        public float PosZ { get; set; }

        public float RotZ { get; set; }
    }
}