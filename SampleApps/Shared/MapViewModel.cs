using MapControl;
using System.Collections.Generic;

namespace SampleApplication
{
    public class PointItem
    {
        public string Name { get; set; }

        public Location Location { get; set; }
    }

    public class PolylineItem
    {
        public LocationCollection Locations { get; set; }
    }

    public class MapViewModel
    {
        public List<PointItem> Points { get; } = new List<PointItem>();
        public List<PointItem> Pushpins { get; } = new List<PointItem>();
        public List<PolylineItem> Polylines { get; } = new List<PolylineItem>();

        public MapViewModel()
        {
            Points.Add(new PointItem
            {
                Name = "Steinbake Leitdamm",
                Location = new Location(53.51217, 8.16603)
            });

            Points.Add(new PointItem
            {
                Name = "Buhne 2",
                Location = new Location(53.50926, 8.15815)
            });

            Points.Add(new PointItem
            {
                Name = "Buhne 4",
                Location = new Location(53.50468, 8.15343)
            });

            Points.Add(new PointItem
            {
                Name = "Buhne 6",
                Location = new Location(53.50092, 8.15267)
            });

            Points.Add(new PointItem
            {
                Name = "Buhne 8",
                Location = new Location(53.49871, 8.15321)
            });

            Points.Add(new PointItem
            {
                Name = "Buhne 10",
                Location = new Location(53.49350, 8.15563)
            });

            Pushpins.Add(new PointItem
            {
                Name = "WHV - Eckwarderhörne",
                Location = new Location(53.5495, 8.1877)
            });

            Pushpins.Add(new PointItem
            {
                Name = "JadeWeserPort",
                Location = new Location(53.5914, 8.14)
            });

            Pushpins.Add(new PointItem
            {
                Name = "Kurhaus Dangast",
                Location = new Location(53.447, 8.1114)
            });

            Pushpins.Add(new PointItem
            {
                Name = "Eckwarderhörne",
                Location = new Location(53.5207, 8.2323)
            });

            Polylines.Add(new PolylineItem
            {
                Locations = LocationCollection.Parse("53.5140,8.1451 53.5123,8.1506 53.5156,8.1623 53.5276,8.1757 53.5491,8.1852 53.5495,8.1877 53.5426,8.1993 53.5184,8.2219 53.5182,8.2386 53.5195,8.2387")
            });

            Polylines.Add(new PolylineItem
            {
                Locations = LocationCollection.Parse("53.5978,8.1212 53.6018,8.1494 53.5859,8.1554 53.5852,8.1531 53.5841,8.1539 53.5802,8.1392 53.5826,8.1309 53.5867,8.1317 53.5978,8.1212")
            });
        }
    }
}
