using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl
{
    public struct TableValuedParameter
    {
        public string TypeName
        {
            get;
        }

        public IEnumerable<SqlDataRecord> Records
        {
            get;
        }

        internal TableValuedParameter(string typeName, IEnumerable<SqlDataRecord> records)
        {
            this.TypeName = typeName;
            this.Records = records;
        }
    }
}
