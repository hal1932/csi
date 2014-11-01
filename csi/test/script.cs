///require: hoge.cs
///require: hoge1.cs
///require: .\hoge2.cs
///addref: testlib.dll
using System;
using testlib;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach(var arg in args)
            {
                Console.WriteLine(arg.ToString());
            }
            Console.WriteLine("Hello, world");

            hoge.f();
            hoge1.f();
            hoge2.f();

            var c = new Class1();
            c.f();

#if DEBUG
            Console.WriteLine("DEBUG");
#else
            Console.WriteLine("no DEBUG");
#endif
        }
    }
}
