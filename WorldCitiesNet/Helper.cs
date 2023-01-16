using CsvHelper;
using CsvHelper.Configuration;
using GraphEx;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using WorldCitiesNet.Models;

namespace WorldCitiesNet
{
    public static class Helper
    {
        public static double CalcDistInternal(double lng1, double lat1, double lng2, double lat2)
        {
            double longA = DegToRad(lng1);
            double latA = DegToRad(lat1);

            double longB = DegToRad(lng2);
            double latB = DegToRad(lat2);

            double x = (longB - longA) * Math.Cos((latA + latB) / 2);
            double y = latB - latA;

            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) * 6371;
        }

        //public static double CalcDistCity(Route route)
        //{
        //    var node1 = (City)route.From;
        //    var node2 = (City)route.To;
        //    return Helper.CalcDistInternal(node1.lng, node1.lat, node2.lng, node2.lat);
        
        public static double CalcDistPoint<TNodePayloadType, TEdgePayloadType>(
            Edge<Node<Point, TNodePayloadType, TEdgePayloadType>, TEdgePayloadType> edge, 
            Func<int, int, int, int, int> distFunc)
        {
            var from = edge.From;
            var to = edge.To;
            return distFunc(from.Id.X, to.Id.X, from.Id.Y, to.Id.Y);
        }


        //Func<Edge<Node<TKeyNode, TNodePayload, TEdgePayload>, TEdgePayload>, double> distFunc
        public static double CalcDistStation(Edge<Node<string,Station,Route>, Route> route)
        {
            var start = route.From;
            var stop = route.To;
            return CalcDistInternal(start.Content.Lon, start.Content.Lat, stop.Content.Lon, stop.Content.Lat);
        }

        private static double DegToRad(double degrees)
        {
            return (Math.PI / 180) * degrees;
        }

        public static Graph<string, City, Route> PrepareCityGraph()
        {
            var gr = new Graph<string, City, Route>();
            var cities = LoadCities();

            foreach (var city in cities)
            {
                var newCity = gr.AddNode(city.city);
                //newCity.CopyFrom(city);
            }

            //var countryAdminGroups =
            //    cities.GroupBy(city => new { city.country, city.admin_name }).ToList();

            var countryGroups =
                cities.GroupBy(city => city.country).ToList();

            foreach (var countryGroup in countryGroups)
            {
                var adminAndCapitalGroup =
                    countryGroup.Where(
                        city => string.Equals(city.capital, "primary", StringComparison.InvariantCultureIgnoreCase) ||
                        string.Equals(city.capital, "admin", StringComparison.InvariantCultureIgnoreCase));

                CreateCrossJoindEdges(adminAndCapitalGroup, gr);

                var adminGroups = countryGroup.
                    GroupBy(city => city.admin_name).ToList();

                foreach (var adminGroup in adminGroups)
                {
                    CreateCrossJoindEdges(adminGroup, gr);
                }
            }

            //Connect all capitals together
            var capitals = cities.Where(city => string.Equals(city.capital, "primary", StringComparison.InvariantCultureIgnoreCase)).ToList();
            CreateCrossJoindEdges(capitals, gr);

            gr.BuildGraph();

            return gr;
        }

        private static void CreateCrossJoindEdges(IEnumerable<City> cities, Graph<string, City, Route> gr)
        {
            foreach (var innerCity in cities)
            {
                foreach (var outerCity in cities)
                {
                    gr.AddEdge(innerCity.city, outerCity.city);
                    gr.AddEdge(outerCity.city, innerCity.city);
                }
            }
        }

        private static List<City> LoadCities()
        {
            List<City> cities = new List<City>();

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MemberTypes = CsvHelper.Configuration.MemberTypes.Fields,
            };
            using var streamReader = File.OpenText(GetFolderPath("Data/worldcities.csv"));
            using var csvReader = new CsvReader(streamReader, csvConfig);
            cities = csvReader.GetRecords<City>().ToList();

            return cities;
        }

        private static string GetFolderPath(string relPath)
        {
            var currentAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace(@"file:\", string.Empty);
            var relativePath = Path.Combine(currentAssemblyPath, relPath);
            return Path.GetFullPath(relativePath);
        }
    }
}
