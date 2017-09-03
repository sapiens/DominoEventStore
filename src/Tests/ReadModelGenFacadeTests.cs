 
using FluentAssertions;
using Xunit;
using System;
using Xunit.Abstractions;


namespace Tests
{
    public class ReadModelGenFacadeTests
    {
        private readonly ITestOutputHelper _h;

        public ReadModelGenFacadeTests(ITestOutputHelper h)
        {
            _h = h;
        }

        [Fact]
        public void r()
        {
            Action<dynamic> e = ev => _h.WriteLine(ev.GetType().Name);
            e(new Event1());
            e(new Event2());
        }


    }

    public class Event1
    {
        
    }

    public class Event2
    {
        
    }
} 
