using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace leke.entity
{
    public class User
    {
        public string Account { get; set; }
        public string Pass { get; set; }
        public bool IsComplete { get; set; }
        public CancellationTokenSource cancelToken { get; set; }
    }
}
