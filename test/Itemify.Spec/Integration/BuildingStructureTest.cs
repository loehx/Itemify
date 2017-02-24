using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Itemify.Core.Item;
using Itemify.Core.Typing;
using Itemify.Logging;
using Newtonsoft.Json;
using NUnit.Framework.Internal;
using NUnit.Framework;

namespace Itemify
{
    [TestFixture]
    public class BuildingStructureTest
    {
        private RegionBasedLogWriter log;
        private Itemify itemify;

        [SetUp]
        public void Setup()
        {
            var logData = new CustomLogData(l =>
            {
                Debug.WriteLine(l);
                Console.WriteLine(l);
            });
            this.log = new RegionBasedLogWriter(logData, nameof(BuildingStructureTest));

            var settings = new ItemifySettings("Server=127.0.0.1;Port=5432;Database=itemic;User Id=postgres;Password=abc;");
            settings.Register<EntityTypes>();

            this.itemify = new Itemify(settings, this.log);
        }


        [Test]
        public void Szenario_A()
        {
            var locationItem = this.itemify.NewItem(EntityTypes.Location);
            var location = new Coords(locationItem);

            location.Latitude = 54.342432;
            location.Longitude = 51.246532;
            this.itemify.SaveNew(location.GetItem());

            var sameLocation = GetLocation(location.Guid);
            Assert.AreEqual(location.Guid, sameLocation.Guid);
            Assert.AreEqual(location.Latitude, sameLocation.Latitude);
            Assert.AreEqual(location.Longitude, sameLocation.Longitude);

            location.Latitude = 64.343241;
            location.Longitude = 32.342353;
            this.itemify.SaveExisting(location.GetItem());

            sameLocation = GetLocation(location.Guid);
            Assert.AreEqual(location.Guid, sameLocation.Guid);
            Assert.AreEqual(location.Latitude, sameLocation.Latitude);
            Assert.AreEqual(location.Longitude, sameLocation.Longitude);
        }



        [TypeDefinition("entities")]
        public enum EntityTypes
        {
            [TypeValue("building")]
            Building,

            [TypeValue("room")]
            Room,

            [TypeValue("location")]
            Location
        }



        public Coords CreateLocation(double lat, double lng)
        {
            var location = this.itemify.NewItem(EntityTypes.Location);
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
            private readonly IItem item;
            private KeyValuePair<double, double> body;

            public Coords(IItem item)
            {
                this.item = item;
                this.body = item.TryGetBody<KeyValuePair<double, double>>();
            }

            internal IItem GetItem()
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
