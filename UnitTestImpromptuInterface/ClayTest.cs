﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using ClaySharp;
using ClaySharp.Behaviors;
using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
#if !SELFRUNNER
using NUnit.Framework;
#endif

namespace UnitTestImpromptuInterface
{   
    
    //Test data modified from MS-PL Clay http://clay.codeplex.com
    /// <summary>
    /// Testing Integration of Clay with Impromptu-Interface
    /// </summary>
    [TestClass,TestFixture]
    public class ClayTest : Helper
    {

        [Test, TestMethod]
        public void InvokeMemberContainsNameWithImpromptuInterface()
        {
            var clay = new Clay(new TestBehavior()).ActLike<ISimpeleClassMeth>();
            var result = clay.Action3();
            Assert.IsTrue(result.Contains("[name:Action3]"), "Does Not Match Argument Name");
            Assert.IsTrue(result.Contains("[count:0]"), "Does Not Match Argument Count");

        }

        [Test, TestMethod]
        public void InvokeMemberContainsNameWithImpromptuInvoke()
        {
            var clay = new Clay(new TestBehavior());
            var result = Impromptu.InvokeMember(clay, "Help", "Test");
            Assert.IsTrue(result.Contains("[name:Help]"), "Does Not Match Argument Name");
            Assert.IsTrue(result.Contains("[count:1]"), "Does Not Match Argument Count");

        }

      

        [Test,TestMethod]
        public void TestClay()
        {
            dynamic New = new ClayFactory();

            var directory = New.Array(
                   New.Person().Name("Louis").Aliases(new[] { "Lou" }),
                   New.Person().Name("Bertrand").Aliases("bleroy", "boudin")
                   ).Name("Orchard folks");

            Assert.AreEqual(2, directory.Count);
            Assert.AreEqual("Orchard folks", directory.Name);
            Assert.AreEqual("Louis",directory[0].Name);
            Assert.AreEqual(1, directory[0].Aliases.Count);
            Assert.AreEqual("Lou",directory[0].Aliases[0]);
            Assert.AreEqual("Bertrand",directory[1].Name);
            Assert.AreEqual(2, directory[1].Aliases.Count);
            Assert.AreEqual("bleroy",directory[1].Aliases[0]);
            Assert.AreEqual("boudin",directory[1].Aliases[1]);
        }


        [Test,TestMethod]
        public void TestBuilderWithClay()
          {
              var New = Builder.New<Clay>()
                  .ObjectSetup(
                   Return<object[]>.Arguments(
                  () => new object[]{new IClayBehavior[]{
                    new InterfaceProxyBehavior(),
                    new PropBehavior(),   
                    new ArrayPropAssignmentBehavior(),
                    new NilResultBehavior()}}))
                  .ArraySetup(
                  Return<object[]>.Arguments(
                  ()=>new object[]{ new IClayBehavior[]{
                    new InterfaceProxyBehavior(),
                    new PropBehavior(),
                    new ArrayPropAssignmentBehavior(),
                    new ArrayBehavior(),
                    new NilResultBehavior()}}));



              var directory = New.Array(
                        New.Person().Name("Louis").Aliases(new[] { "Lou" }),
                        New.Person().Name("Bertrand").Aliases("bleroy", "boudin")
                        ).Name("Orchard folks");

             Assert.AreEqual(2, directory.Count);
             Assert.AreEqual("Orchard folks", directory.Name);
             Assert.AreEqual("Louis", directory[0].Name);
             Assert.AreEqual(1, directory[0].Aliases.Count);
             Assert.AreEqual("Lou", directory[0].Aliases[0]);
             Assert.AreEqual("Bertrand", directory[1].Name);
             Assert.AreEqual(2, directory[1].Aliases.Count);
             Assert.AreEqual("bleroy", directory[1].Aliases[0]);
             Assert.AreEqual("boudin", directory[1].Aliases[1]);
        }


         /// <summary>
         /// Impromptu's Interface Proxy is about the same Speed as Clay's
         /// </summary>
        [Test, TestMethod]
        public void SpeedTestInterface()
        {   
            dynamic New = new ClayFactory();
            IRobot tRobot = New.Robot().Name("Bender");
            IRobot tRobotI = Impromptu.ActLike<IRobot>(New.Robot().Name("Bender"));
            
            var tWatchC = TimeIt.Go(() =>
                                         {
                                             var tOut =
                                                 Impromptu.ActLike<IRobot>(New.Robot().Name("Bender"));
                                         }, 50000);
            var tWatchC2 = TimeIt.Go(() =>
                                         {
                                             IRobot tOut = New.Robot().Name("Bender");
                                         },50000 );

            Console.WriteLine("Impromptu: " + tWatchC.Elapsed);
            Console.WriteLine("Clay: " + tWatchC2.Elapsed);

            Assert.Less(tWatchC.Elapsed, tWatchC2.Elapsed);

            var tWatch = TimeIt.Go(() => { var tOut = tRobotI.Name; }, 50000);

            var tWatch2 = TimeIt.Go(() => { var tOut = tRobot.Name; }, 50000);

            Console.WriteLine("Impromptu: " + tWatch.Elapsed);
            Console.WriteLine("Clay: " + tWatch2.Elapsed);

             var tDiffernce = (tWatch.Elapsed - tWatch2.Elapsed);
             Console.WriteLine("50000 Difference: " + tDiffernce);

             Assert.Less(tWatch.Elapsed, tWatch2.Elapsed);
        }

        [Test, TestMethod]
        public void SpeedTestPrototype()
        {
            dynamic NewI = Builder.New();
            dynamic NewE = Builder.New<ExpandoObject>();
            dynamic NewP = Builder.New<Robot>();
            dynamic NewC = new ClayFactory();

            var tRobotI = NewI.Robot(Name: "Bender");
            var tRobotC = NewC.Robot(Name: "Bender");
            var tRobotE = NewE.Robot(Name: "Bender");
            Robot tRobotP = NewP.Robot(Name: "Bender");

            var tWatchI = TimeIt.Go(() =>
            {
                var tOut = tRobotI.Name;
            });
   
            var tWatchC = TimeIt.Go(() =>
             {
                 var tOut =tRobotC.Name;
             } );

            var tWatchE = TimeIt.Go(() =>
            {
                var tOut = tRobotE.Name;
            });

            var tWatchP = TimeIt.Go(() =>
            {
                var tOut = tRobotP.Name;
            });
            Console.WriteLine("Impromptu: " + tWatchI.Elapsed);
            Console.WriteLine("Clay: " + tWatchC.Elapsed);
            Console.WriteLine("Expando: " + tWatchE.Elapsed);
            Console.WriteLine("POCO: " + tWatchP.Elapsed);

            Assert.Less(tWatchI.Elapsed, tWatchC.Elapsed);

            Console.WriteLine("Impromptu VS Clay: {0:0.0} x faster", (double)tWatchC.ElapsedTicks / tWatchI.ElapsedTicks);
            Console.WriteLine("Expando  VS Clay:{0:0.0}  x faster", (double)tWatchC.ElapsedTicks / tWatchE.ElapsedTicks);
            Console.WriteLine("POCO  VS Clay:{0:0.0}  x faster", (double)tWatchC.ElapsedTicks / tWatchP.ElapsedTicks);
            Console.WriteLine("POCO  VS Impromptu:{0:0.0}  x faster", (double)tWatchI.ElapsedTicks / tWatchP.ElapsedTicks);
            Console.WriteLine("POCO  VS Expando:{0:0.0}  x faster", (double)tWatchE.ElapsedTicks / tWatchP.ElapsedTicks);
            Console.WriteLine("Expando  VS Impromptu:{0:0.0}  x faster", (double)tWatchI.ElapsedTicks / tWatchE.ElapsedTicks);
        }

        //TestBehavoir from MS-PL ClaySharp http://clay.codeplex.com
        class TestBehavior : ClayBehavior
        {
            public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args)
            {
                return string.Format("[name:{0}] [count:{1}]", name ?? "<null>", args.Count());
            }
        }
    }
    
}
