using GraphEx;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldCitiesNet.Models
{
    public class City
    {
        public string city;
        public string city_ascii;
        public double lat;
        public double lng;
        public string country;
        public string iso2;
        public string iso3;
        public string admin_name;
        public string capital;
        public double? population;
        public long id;

        public override string ToString()
        {
            return $"{city}, lat({lat}), lng({lng}), {country}, {admin_name}, {capital}, {population}";
        }

        public void CopyFrom(City refCity)
        {
            city = refCity.city;
            city_ascii = refCity.city_ascii;
            lat = refCity.lat;
            lng = refCity.lng;
            country = refCity.country;
            iso2 = refCity.iso2;
            iso3 = refCity.iso3;
            admin_name = refCity.admin_name;
            capital = refCity.capital;
            population = refCity.population;
            id = refCity.id;
        }
    }
}
