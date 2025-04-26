using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mappah.Tests.Models
{
    namespace Mappah.Tests.Models
    {
        public class InnerObject
        {
            public int TestField { get; set; }
        }

        public class InnerObjectDto
        {
            public int TestField { get; set; }
        }

        public class OuterObject
        {
            public int OuterField { get; set; }
            public InnerObject Inner { get; set; }
            public DifferentInner DifferentInner { get; set; }
        }

        public class OuterObjectDto
        {
            public int OuterField { get; set; }
            public InnerObjectDto Inner { get; set; }
            public DifferentInnerDto ManualInnerDto { get; set; }
        }

        public class DifferentInner
        {
            public string SomeValue { get; set; }
        }

        public class DifferentInnerDto
        {
            public string AnotherValue { get; set; }
        }
    }

}
