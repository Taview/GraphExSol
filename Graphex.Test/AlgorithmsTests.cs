using CsvHelper;
using CsvHelper.Configuration;
using GraphEx;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using WorldCitiesNet;
using WorldCitiesNet.Models;

namespace Graphex.Test
{
    public class AlgorithmsTests
    {
        private Graph<string> cityGraph;

        [SetUp]
        public void Setup()
        {
        }

        [OneTimeSetUp]
        public void SetupOnce()
        {
            cityGraph = PrepareCityGraph();

            Console.WriteLine($"Total cities {cityGraph.GetNodeCount()}");
            Console.WriteLine($"Total routes {cityGraph.GetEdgeCount()}");
        }

        [Test]
        [TestCase("Brno", "Kotsyubyns’ke", new string[] { "Brno", "Prague", "Kyiv", "Kotsyubyns’ke" })]
        public void ShortestDistanceDejikstra(string startCity, string endCity, string[] stationsToValidate)
        {
            Console.WriteLine($"Total cities {cityGraph.GetNodeCount()}");
            Console.WriteLine($"Total routes {cityGraph.GetEdgeCount()}");

            int firstCity = cityGraph.GetNodeIndex(startCity);
            int secondCity = cityGraph.GetNodeIndex(endCity);
            //"Kotsyubyns’ke"

            var nodes = cityGraph.Nodes;

            Console.WriteLine($"Dejikstra Starts here");
            var dejikstraWatchTime = new System.Diagnostics.Stopwatch();
            dejikstraWatchTime.Start();

            int[] shortestIndexes;
            var distances = Algorithms.FindShortestPathDejikstraFromNode<string>(firstCity, cityGraph, route => CalculateGeoDistance(route), out shortestIndexes);

            dejikstraWatchTime.Stop();
            Console.WriteLine($"Dejikstra Execution Time: {dejikstraWatchTime.Elapsed.TotalSeconds} secs");

            var pathStations = Algorithms.GetShortestPath(shortestIndexes, firstCity, secondCity);

            int resIndex = 0;
            foreach (var pathIndex in pathStations)
            {
                Console.WriteLine($"{nodes[pathIndex].Payload}, {distances[pathIndex]}");
                Assert.AreEqual(stationsToValidate[resIndex], nodes[pathIndex].Id);
                resIndex++;
            }

            Console.WriteLine($"Total Length {Algorithms.GetShortestDistance(distances, secondCity)}");
        }

        private double CalculateGeoDistance(Edge<string> route)
        {
            var node1 = (City)route.From.Payload;
            var node2 = (City)route.To.Payload;
            return Helper.CalcDistInternal(node1.lng, node1.lat, node2.lng, node2.lat);
        }

        [Test]
        [TestCase("Brno", "Kotsyubyns’ke", new string[] { "Brno", "Prague", "Kyiv", "Kotsyubyns’ke" })]
        public void ShortestDistanceAStar(string startCity,  string endCity, string[] stationsToValidate)
        {
            Console.WriteLine($"Total cities {cityGraph.GetNodeCount()}");
            Console.WriteLine($"Total routes {cityGraph.GetEdgeCount()}");

            int firstCity = cityGraph.GetNodeIndex(startCity);
            int secondCity = cityGraph.GetNodeIndex(endCity);
            //"Kotsyubyns’ke"

            var nodes = cityGraph.Nodes;

            Console.WriteLine($"A* Starts here");
            var dejikstraWatchTime = new System.Diagnostics.Stopwatch();
            dejikstraWatchTime.Start();

            int[] shortestIndexes;
            var distances = Algorithms.FindShortestPathAStarFromNode<string>(
                firstCity, 
                secondCity,
                cityGraph, 
                route => CalculateGeoDistance(route),
                routeFinal => CalculateGeoDistance(routeFinal),
                out shortestIndexes);

            dejikstraWatchTime.Stop();
            Console.WriteLine($"A* Execution Time: {dejikstraWatchTime.Elapsed.TotalSeconds} secs");

            var pathStations = Algorithms.GetShortestPath(shortestIndexes, firstCity, secondCity);

            int resIndex = 0;
            foreach (var pathIndex in pathStations)
            {   
                Console.WriteLine($"{nodes[pathIndex].Payload}, {distances[pathIndex]}");
                Assert.AreEqual(stationsToValidate[resIndex], nodes[pathIndex].Id);
                resIndex++;
            }
        }

        #region Helper Funcs
        public Graph<string> PrepareCityGraph()
        {
            var gr = new Graph<string>();
            var cities = LoadCities();

            foreach (var city in cities)
            {
                var newCity = gr.AddNode(city.city);
                newCity.Payload = city;
            }

            var countryGroups =
                cities.GroupBy(city => city.country).ToList();

            foreach(var countryGroup in countryGroups)
            {
                var adminAndCapitalGroup =
                    countryGroup.Where(
                        city => string.Equals(city.capital, "primary", StringComparison.InvariantCultureIgnoreCase) ||
                        string.Equals(city.capital, "admin", StringComparison.InvariantCultureIgnoreCase));

                CreateCrossJoindEdges(adminAndCapitalGroup, gr);

                var adminGroups = countryGroup.
                    GroupBy(city => city.admin_name).ToList();

                foreach(var adminGroup in adminGroups)
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

        private void CreateCrossJoindEdges(IEnumerable<City> cities, Graph<string> gr)
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

        private List<City> LoadCities()
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

        private string GetFolderPath(string relPath)
        {
            var currentAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace(@"file:\", string.Empty);
            var relativePath = Path.Combine(currentAssemblyPath, relPath);
            return Path.GetFullPath(relativePath);
        }
        #endregion
    }
}