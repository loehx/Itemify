using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Itemify.Core.Item;
using Itemify.Logging;
using Newtonsoft.Json;
using Xunit;


namespace Itemify
{ 
    public class BuildingStructureTest
    {
        private RegionBasedLogWriter log;
        private Itemify itemify;

        public BuildingStructureTest()
        {
            var logData = new CustomLogData(l =>
            {
                Debug.WriteLine(l);
                Console.WriteLine(l);
            });
            this.log = new RegionBasedLogWriter(logData, nameof(BuildingStructureTest));
            
            var settings = new ItemifySettings(host: "134.168.62.120",
                port: 5432,
                username: "postgres_dawid",
                password: "LustitiaDev",
                database: "postgres_dawid",
                connectionPoolSize: 50,
                timeout: TimeSpan.FromSeconds(5).Milliseconds);

            this.itemify = new Itemify(settings, this.log);
        }


        [Fact]
        public void Szenario_A()
        {
            var locationItem = new Item(Guid.NewGuid(), EntityTypes.Location);
            var location = new Coords(locationItem);

            location.Latitude = 54.342432;
            location.Longitude = 51.246532;
            this.itemify.SaveNew(location.GetItem());

            var sameLocation = GetLocation(location.Guid);
            Assert.Equal(location.Guid, sameLocation.Guid);
            Assert.Equal(location.Latitude, sameLocation.Latitude);
            Assert.Equal(location.Longitude, sameLocation.Longitude);

            location.Latitude = 64.343241;
            location.Longitude = 32.342353;
            this.itemify.SaveExisting(location.GetItem());

            sameLocation = GetLocation(location.Guid);
            Assert.Equal(location.Guid, sameLocation.Guid);
            Assert.Equal(location.Latitude, sameLocation.Latitude);
            Assert.Equal(location.Longitude, sameLocation.Longitude);
        }



        public class EntityTypes
        {
            public static string Building => "Building";
            public static string Room => "Room";
            public static string Location => "Location";
        }



        public Coords CreateLocation(double lat, double lng)
        {
            var location = new Item(EntityTypes.Location);
            var coords = new Coords(location);

            coords.Latitude = lat;
            coords.Longitude = lng;

            this.itemify.SaveNew(coords.GetItem());

            return coords;
        }

        public Coords GetLocation(Guid guid)
        {
            return this.itemify.GetItemByReference(guid, EntityTypes.Location)
                .Wrap<Coords>();
        }

        public class Coords
        {
            private readonly Item item;
            private KeyValuePair<double, double> body;

            public Coords(Item item)
            {
                this.item = item;
                this.body = item.TryGetBody<KeyValuePair<double, double>>();
            }

            internal Item GetItem()
            {
                item.SetBody(body);
                return item;
            }

            public Guid Guid => item.Guid;

            public double Latitude
            {
                get { return body.Key; }
                set { body = new KeyValuePair<double, double>(value, body.Value); }
            }

            public double Longitude
            {
                get { return body.Value; }
                set { body = new KeyValuePair<double, double>(body.Key, value); }
            }
        }
    }
}
