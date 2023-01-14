﻿using GraphEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WorldCitiesNet.Models
{
    //    where TNodeKey :  IComparable<TNodeKey>
    //where TNode : Node<TNodeKey>, new ()
    //where TEdge : Edge<Node<TNodeKey>>, new ()

    public class Route : Edge<Node<string>>
    {
    }

    public class Station : Node<string>
    {
        //public string Name; //The full name of the stop, between quote marks"
        public string Desc; //The description of the stop (not used)
        public double Lat; //The latitude of the stop (in degrees)
        public double Lon; //The longitude of the stop (in degrees)
        public string Zone; //The identifier of the zone (not used)
        public string Url; //The url of the stop (not used)
        public string Type; //The type of stop
        public string Mother; //The mother station (not used)
        public string OrigString;
    }
}
