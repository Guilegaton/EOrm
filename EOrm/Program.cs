using EOrm.DBModels;
using EOrm.Repositories.ADO;
using EOrm.Repositories.EF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EOrm
{
    class Program
    {
        static void Main(string[] args)
        {
            var adoRepo = new AdoRepository<Truck>(@"Data Source=DESKTOP-C5BMDJ8\SQLEXPRESS;Initial Catalog=Shipment;Integrated Security=True");
            var adoStopwatch = new Stopwatch();
            adoStopwatch.Start();

            var rnd = new Random();
            var obj = new object();
            Parallel.For(0, 100000, (i) =>
            {
                var model = new Truck
                {
                    BrandName = $"ado brand {i}",
                    Volume = rnd.Next(10000, 100000),
                    FuelConsumption = rnd.Next(10, 30),
                    Payload = rnd.Next(9000, 50000),
                    RegistrationNumber = $"TEST{i}",
                    Year = 2021
                };
                lock (obj)
                {
                    adoRepo.Create(model);
                    adoRepo.CommitChanges();
                    model = adoRepo.GetByField(new Dictionary<System.Linq.Expressions.Expression<Func<Truck, object>>, object> { { tr => tr.BrandName, model.BrandName } });
                }
                Console.WriteLine($"{i}");
            });
            adoStopwatch.Stop();
            adoRepo.Dispose();

            var adoDiconnectedRepo = new AdoRepositoryDisconnectedAproach<Truck>(@"Data Source=DESKTOP-C5BMDJ8\SQLEXPRESS;Initial Catalog=Shipment;Integrated Security=True");
            var adoDiconnectedStopwatch = new Stopwatch();
            adoDiconnectedStopwatch.Start();
            Parallel.For(0, 100000, (i) =>
            {
                var model = new Truck
                {
                    BrandName = $"ado dis brand {i}",
                    Volume = rnd.Next(10000, 100000),
                    FuelConsumption = rnd.Next(10, 30),
                    Payload = rnd.Next(9000, 50000),
                    RegistrationNumber = $"TEST{i}",
                    Year = 2021
                };
                lock (obj)
                {
                    adoDiconnectedRepo.Create(model);
                    adoDiconnectedRepo.CommitChanges();
                    model = adoDiconnectedRepo.GetByField(new Dictionary<System.Linq.Expressions.Expression<Func<Truck, object>>, object> { { tr => tr.BrandName, model.BrandName } });
                }
                Console.WriteLine($"{i}");
            });
            adoDiconnectedStopwatch.Stop();
            adoDiconnectedRepo.Dispose();

            var efRepo = new EFRepository<Truck>();
            var efStopwatch = new Stopwatch();
            efStopwatch.Start();
            Parallel.For(0, 100000, (i) =>
            {
                var model = new Truck
                {
                    BrandName = $"ef brand {i}",
                    Volume = rnd.Next(10000, 100000),
                    FuelConsumption = rnd.Next(10, 30),
                    Payload = rnd.Next(9000, 50000),
                    RegistrationNumber = $"TEST{i}",
                    Year = 2021
                };
                lock (obj)
                {
                    efRepo.Create(model);
                    efRepo.CommitChanges();
                    model = efRepo.GetByField(new Dictionary<System.Linq.Expressions.Expression<Func<Truck, object>>, object> { { tr => tr.BrandName, model.BrandName } });
                }
                Console.WriteLine($"{i}");
            });
            efStopwatch.Stop();
            efRepo.Dispose();

            var dpRepo = new EFRepository<Truck>();
            var dpStopwatch = new Stopwatch();
            dpStopwatch.Start();
            Parallel.For(0, 100000, (i) =>
            {
                var model = new Truck
                {
                    BrandName = $"dapper brand {i}",
                    Volume = rnd.Next(10000, 100000),
                    FuelConsumption = rnd.Next(10, 30),
                    Payload = rnd.Next(9000, 50000),
                    RegistrationNumber = $"TEST{i}",
                    Year = 2021
                };
                lock (obj)
                {
                    dpRepo.Create(model);
                    dpRepo.CommitChanges();
                    model = dpRepo.GetByField(new Dictionary<System.Linq.Expressions.Expression<Func<Truck, object>>, object> { { tr => tr.BrandName, model.BrandName } });
                }
                Console.WriteLine($"{i}");
            });
            dpStopwatch.Stop();
            dpRepo.Dispose();

            Console.WriteLine($"ADO: {adoStopwatch.Elapsed:'HH':'mm':'ss.fffffff'}\r\nADO disc: {adoDiconnectedStopwatch.Elapsed:'HH':'mm':'ss.fffffff'}\r\nEF: {efStopwatch.Elapsed:'HH':'mm':'ss.fffffff'}\r\nDapper: {dpStopwatch.Elapsed:'HH':'mm':'ss.fffffff'}");
        }
    }
}
